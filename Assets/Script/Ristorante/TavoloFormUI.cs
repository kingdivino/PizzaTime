using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Collections.Generic;

public class TavoloFormUI : MonoBehaviour
{
    [Header("Numero Tavolo")]
    public Button prevButton;
    public Button nextButton;
    public TextMeshProUGUI numeroText;

    [Header("Posti")]
    public TMP_InputField inputPosti;

    [Header("Azioni")]
    public Button confermaButton;
    public Button annullaButton;

    [Header("UI Navigazione")]
    public GameObject Salatavoli; // pannello lista tavoli da riattivare alla chiusura

    [Header("Riferimenti")]
    public SalaSelector salaSelector; // 🔹 qui colleghi SalaSelector dall’Inspector

    [Header("Azioni extra")]
    public Button eliminaButton;

    private Action<string, int> onConferma;
    private Action onAnnulla;

    private int numeroCorrente = 1;
    private int maxNumero = 50;

    

    // insieme dei nomi occupati (es. "Tavolo 1", "Tavolo 2", ...)
    private HashSet<string> tavoliEsistenti;

    private bool isEditMode = false;
    private Tavolo tavoloCorrente;

    // -------------------- CREAZIONE --------------------
    public void ApriPerCreazione(Action<string, int> confermaCallback, Action annullaCallback)
    {
        isEditMode = false;
        tavoloCorrente = null;
        
        if (eliminaButton != null)
            eliminaButton.gameObject.SetActive(false);

        var manager = FindObjectOfType<TavoloManagerRuntime>();
        if (manager != null && salaSelector != null && salaSelector.salaCorrente != null)
        {
            manager.CaricaNomiTavoli((listaNomi) =>
            {
                tavoliEsistenti = new HashSet<string>(listaNomi);
                numeroCorrente = TrovaPrimoDisponibile(1);
                SetupUIBase(confermaCallback, annullaCallback);
                inputPosti.text = "4";
                AggiornaNumero();
            });
        }
        else
        {
            Debug.LogError("❌ Manager o SalaSelector non trovati");
        }


        SetupUIBase(confermaCallback, annullaCallback);
        inputPosti.text = "4";
        AggiornaNumero();

        // listeners frecce
        prevButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();
        prevButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);

        prevButton.onClick.AddListener(() =>
        {
            numeroCorrente = TrovaPrecedenteDisponibile(numeroCorrente);
            AggiornaNumero();
            Debug.Log($"🔵 [TavoloFormUI] Prev → Tavolo {numeroCorrente}");
        });

        nextButton.onClick.AddListener(() =>
        {
            numeroCorrente = TrovaProssimoDisponibile(numeroCorrente);
            AggiornaNumero();
            Debug.Log($"🔵 [TavoloFormUI] Next → Tavolo {numeroCorrente}");
        });
    }

    // -------------------- MODIFICA --------------------
    public void ApriPerModifica(Tavolo tavolo, Action<string, int> confermaCallback, Action annullaCallback)
{
    isEditMode = true;
    tavoloCorrente = tavolo;
    CaricaOccupati(true);
    numeroCorrente = EstraiNumeroDaNome(tavolo.nominativo);
    SetupUIBase(confermaCallback, annullaCallback);
    inputPosti.text = Mathf.Max(1, tavolo.numeroPosti).ToString();
    AggiornaNumero();

    // eliminaButton visibile solo in MODIFICA
    if (eliminaButton != null)
    {
        eliminaButton.gameObject.SetActive(true);
        eliminaButton.onClick.RemoveAllListeners();
        eliminaButton.onClick.AddListener(() =>
        {
            if (tavoloCorrente != null)
            {
                // trova il manager nella scena
                var manager = FindObjectOfType<TavoloManagerRuntime>();
                if (manager != null)
                {
                    manager.EliminaTavoloNelDB(tavoloCorrente.id);
                }
                else
                {
                    Debug.LogError("❌ TavoloManagerRuntime non trovato!");
                }

                gameObject.SetActive(false);
                if (Salatavoli) Salatavoli.SetActive(true);
            }
        });
    }

    // prev / next listener...
}

    // -------------------- COMUNE --------------------
    private void SetupUIBase(Action<string, int> confermaCallback, Action annullaCallback)
    {
        if (Salatavoli) Salatavoli.SetActive(false);

        gameObject.SetActive(true);
        onConferma = confermaCallback;
        onAnnulla = annullaCallback;

        confermaButton.onClick.RemoveAllListeners();
        annullaButton.onClick.RemoveAllListeners();

        confermaButton.onClick.AddListener(() =>
        {
            int posti = 4;
            int.TryParse(inputPosti.text, out posti);
            posti = Mathf.Max(1, posti);

            string nome = $"Tavolo {numeroCorrente}";
            Debug.Log($"🟢 [TavoloFormUI] Conferma → Nome={nome}, Posti={posti}");

            if (tavoliEsistenti.Contains(nome))
            {
                Debug.LogError($"❌ '{nome}' esiste già!");
                return;
            }

            Debug.Log($"👉 [TavoloFormUI] Invoco onConferma, target={onConferma?.Target}");
            onConferma?.Invoke(nome, posti);

            gameObject.SetActive(false);
            if (Salatavoli) Salatavoli.SetActive(true);
            prevButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
        });

        annullaButton.onClick.AddListener(() =>
        {
            Debug.Log("🟡 [TavoloFormUI] Annulla");
            onAnnulla?.Invoke();
            gameObject.SetActive(false);
            if (Salatavoli) Salatavoli.SetActive(true);
            prevButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
        });
    }

    private void AggiornaNumero()
    {
        numeroText.text = $"Tavolo {numeroCorrente}";
        numeroText.color = Color.green;
    }

    // 🔹 ora prende i tavoli dalla sala corrente
    private void CaricaOccupati(bool escludiCorrente)
    {
        tavoliEsistenti = new HashSet<string>();

        if (salaSelector != null && salaSelector.salaCorrente != null && salaSelector.salaCorrente.tavoli != null)
        {
            foreach (var t in salaSelector.salaCorrente.tavoli)
            {
                if (t == null || string.IsNullOrEmpty(t.nominativo)) continue;

                if (escludiCorrente && tavoloCorrente != null && t == tavoloCorrente)
                    continue;

                tavoliEsistenti.Add(t.nominativo);
            }
        }

        Debug.Log($"📋 [TavoloFormUI] Tavoli esistenti caricati: {string.Join(", ", tavoliEsistenti)}");
    }

    // -------------- Ricerca pross/prec disponibili --------------
    private int TrovaPrimoDisponibile(int start)
    {
        for (int i = start; i <= maxNumero; i++)
            if (!tavoliEsistenti.Contains($"Tavolo {i}"))
                return i;
        return start;
    }

    private int TrovaProssimoDisponibile(int current)
    {
        for (int i = current + 1; i <= maxNumero; i++)
            if (!tavoliEsistenti.Contains($"Tavolo {i}"))
                return i;
        return current; // nessun altro libero
    }

    private int TrovaPrecedenteDisponibile(int current)
    {
        for (int i = current - 1; i >= 1; i--)
            if (!tavoliEsistenti.Contains($"Tavolo {i}"))
                return i;
        return current; // nessun altro libero
    }

    private int EstraiNumeroDaNome(string nominativo)
    {
        if (string.IsNullOrEmpty(nominativo)) return 1;
        var parts = nominativo.Split(' ');
        if (parts.Length > 1 && int.TryParse(parts[1], out int num))
            return num;
        return 1;
    }
}

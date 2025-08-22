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

        CaricaOccupati(escludiCorrente:false);
        numeroCorrente = TrovaPrimoDisponibile(1);

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
        });

        nextButton.onClick.AddListener(() =>
        {
            numeroCorrente = TrovaProssimoDisponibile(numeroCorrente);
            AggiornaNumero();
        });
    }

    // -------------------- MODIFICA --------------------
    public void ApriPerModifica(Tavolo tavolo, Action<string, int> confermaCallback, Action annullaCallback)
    {
        isEditMode = true;
        tavoloCorrente = tavolo;

        // escludo il tavolo corrente dall’elenco occupati, così posso tenere lo stesso numero
        CaricaOccupati(escludiCorrente:true);

        numeroCorrente = EstraiNumeroDaNome(tavolo.nominativo);

        SetupUIBase(confermaCallback, annullaCallback);
        inputPosti.text = Mathf.Max(1, tavolo.numeroPosti).ToString();
        AggiornaNumero();

        // abilito le frecce ANCHE in modifica
        prevButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();
        prevButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);

        prevButton.onClick.AddListener(() =>
        {
            numeroCorrente = TrovaPrecedenteDisponibile(numeroCorrente);
            AggiornaNumero();
        });

        nextButton.onClick.AddListener(() =>
        {
            numeroCorrente = TrovaProssimoDisponibile(numeroCorrente);
            AggiornaNumero();
        });
    }

    // -------------------- COMUNE --------------------
    private void SetupUIBase(Action<string, int> confermaCallback, Action annullaCallback)
    {
        if (Salatavoli) Salatavoli.SetActive(false);

        gameObject.SetActive(true);
        onConferma = confermaCallback;
        onAnnulla  = annullaCallback;

        confermaButton.onClick.RemoveAllListeners();
        annullaButton.onClick.RemoveAllListeners();

        confermaButton.onClick.AddListener(() =>
        {
            int posti = 4;
            int.TryParse(inputPosti.text, out posti);
            posti = Mathf.Max(1, posti);

            string nome = $"Tavolo {numeroCorrente}";

            // controllo duplicati anche in MODIFICA (se ho cambiato numero)
            if (tavoliEsistenti.Contains(nome))
            {
                Debug.LogError($"'{nome}' esiste già!");
                return;
            }

            onConferma?.Invoke(nome, posti);

            // close
            gameObject.SetActive(false);
            if (Salatavoli) Salatavoli.SetActive(true);

            // ripristino visibilità frecce per la prossima apertura
            prevButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
        });

        annullaButton.onClick.AddListener(() =>
        {
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

    private void CaricaOccupati(bool escludiCorrente)
    {
        tavoliEsistenti = new HashSet<string>();
        var assets = Resources.LoadAll<Tavolo>("Tavoli");
        foreach (var t in assets)
        {
            if (t == null || string.IsNullOrEmpty(t.nominativo)) continue;

            // se richiesto, escludo il nome del tavolo che sto modificando
            if (escludiCorrente && tavoloCorrente != null && t == tavoloCorrente)
                continue;

            tavoliEsistenti.Add(t.nominativo);
        }
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

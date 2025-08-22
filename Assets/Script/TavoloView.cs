using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TavoloView : MonoBehaviour
{
    [Header("UI Riferimenti")]
    public TextMeshProUGUI numeroTxt;
    public TextMeshProUGUI statoTxt;
    public Image background; // opzionale, sfondo del tavolo

    [Header("Bottoni")]
    public Button liberaButton;
    public Button modificaButton; // 👈 nuovo pulsante Modifica

    private Tavolo data;
    private TavoloDettaglioView dettaglioUI;
    private TavoloPrenotazioneView prenotazioneUI;
    private TavoloFormUI tavoloFormUI; // 👈 riferimento al form (assegna in Inspector)

    public void Bind(
        Tavolo tavolo,
        TavoloDettaglioView dettaglio,
        TavoloPrenotazioneView prenotazione,
        TavoloFormUI formUI // 👈 passiamo il form al bind
    )
    {
        data = tavolo;
        dettaglioUI = dettaglio;
        prenotazioneUI = prenotazione;
        tavoloFormUI = formUI;

        AggiornaUI();

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();

            // 🔹 Listener unico → decide al click in base allo stato attuale
            btn.onClick.AddListener(() =>
            {
                if (data.disponibile)
                {
                    // tavolo libero → apri pannello prenotazione
                    prenotazioneUI.Apri(data,
                        (persone, cognome, orario) =>
                        {
                            Debug.Log($"Prenotato {data.nominativo} da {cognome} alle {orario} per {persone} persone");
                            AggiornaUI(); // refresh immediato
                        },
                        () => Debug.Log("Prenotazione annullata")
                    );
                }
                else
                {
                    // tavolo occupato → apri dettaglio
                    dettaglioUI.MostraDettaglio(data);
                }
            });
        }

        // pulsante Libera
        if (liberaButton != null)
        {
            liberaButton.onClick.RemoveAllListeners();
            liberaButton.onClick.AddListener(LiberaTavolo);
        }

        // pulsante Modifica (solo se libero)
        if (modificaButton != null)
        {
            modificaButton.onClick.RemoveAllListeners();
            modificaButton.onClick.AddListener(() =>
            {
                if (data.disponibile && tavoloFormUI != null)
                {
                    tavoloFormUI.ApriPerModifica(data, (nuovoNome, nuoviPosti) =>
                    {
                        data.nominativo = nuovoNome;
                        data.numeroPosti = nuoviPosti;

#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(data);
                        UnityEditor.AssetDatabase.SaveAssets();
#endif
                        AggiornaUI();
                        Debug.Log($"Tavolo modificato: {nuovoNome}, posti: {nuoviPosti}");
                    },
                    () => Debug.Log("Modifica annullata"));
                }
            });
        }
    }

    public void AggiornaUI()
    {
        if (data == null) return;

        numeroTxt.text = string.IsNullOrEmpty(data.nominativo)
            ? $"Tavolo {data.id}"
            : data.nominativo;

        if (data.disponibile)
        {
            statoTxt.text = $"Libero\n({data.numeroPosti} posti)";
            if (background != null) background.color = new Color(0.7f, 1f, 0.7f);

            if (liberaButton != null) liberaButton.gameObject.SetActive(false);
            if (modificaButton != null) modificaButton.gameObject.SetActive(true); // 👈 mostra Modifica solo se libero
        }
        else
        {
            statoTxt.text = $"Occupato\n{data.postiOccupati}/{data.numeroPosti}";
            if (background != null) background.color = new Color(1f, 0.8f, 0.6f);

            if (liberaButton != null) liberaButton.gameObject.SetActive(true);
            if (modificaButton != null) modificaButton.gameObject.SetActive(false); // 👈 nascondi Modifica se occupato
        }
    }

    void Update()
    {
        // refresh continuo durante playmode
        AggiornaUI();
    }

    private void LiberaTavolo()
    {
        if (data == null) return;

        data.disponibile = true;
        data.postiOccupati = 0;
        data.cognomePrenotazione = "";
        data.orarioPrenotazione = "";

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(data);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        AggiornaUI();
        Debug.Log($"{data.nominativo} è stato liberato!");
    }
}

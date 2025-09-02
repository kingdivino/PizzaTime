using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TavoloView : MonoBehaviour
{
    [Header("UI Riferimenti")]
    public TextMeshProUGUI numeroTxt;
    public TextMeshProUGUI statoTxt;
    public Image background;

    [Header("Bottoni")]
    public Button liberaButton;
    public Button modificaButton;

    private Tavolo data;
    private TavoloDettaglioView dettaglioUI;
    private TavoloPrenotazioneView prenotazioneUI;
    private TavoloFormUI tavoloFormUI;
    private TavoloManagerRuntime tavoloManager; // ✅ nuovo

    public void Bind(
        Tavolo tavolo,
        TavoloDettaglioView dettaglio,
        TavoloPrenotazioneView prenotazione,
        TavoloFormUI formUI,
        TavoloManagerRuntime manager
    )
    {
        data = tavolo;
        dettaglioUI = dettaglio;
        prenotazioneUI = prenotazione;
        tavoloFormUI = formUI;
        tavoloManager = manager;

        AggiornaUI();

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (data.disponibile)
                {
                    prenotazioneUI.Apri(data,
                        (persone, cognome, orario) =>
                        {
                            Debug.Log($"Prenotato {data.nominativo} da {cognome} alle {orario} per {persone} persone");
                            AggiornaUI();
                        },
                        () => Debug.Log("Prenotazione annullata")
                    );
                }
                else
                {
                    dettaglioUI.MostraDettaglio(data);
                }
            });
        }

        if (liberaButton != null)
        {
            liberaButton.onClick.RemoveAllListeners();
            liberaButton.onClick.AddListener(LiberaTavolo);
        }

        if (modificaButton != null)
        {
            modificaButton.onClick.RemoveAllListeners();
            modificaButton.onClick.AddListener(() =>
            {
                if (data.disponibile && tavoloFormUI != null && tavoloManager != null)
                {
                    Debug.Log($"🟡 Modifica tavolo {data.nominativo}");

                    tavoloFormUI.ApriPerModifica(data, (nuovoNome, nuoviPosti) =>
                    {
                        // chiamiamo il manager per aggiornare il tavolo nel DB
                        tavoloManager.AggiornaTavoloNelDB(data.id, nuovoNome, nuoviPosti);

                    },
                    () => Debug.Log("Modifica tavolo annullata"));
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
            if (modificaButton != null) modificaButton.gameObject.SetActive(true);
        }
        else
        {
            statoTxt.text = $"Occupato\n{data.postiOccupati}/{data.numeroPosti}";
            if (background != null) background.color = new Color(1f, 0.8f, 0.6f);

            if (liberaButton != null) liberaButton.gameObject.SetActive(true);
            if (modificaButton != null) modificaButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        AggiornaUI();
    }

private void LiberaTavolo()
{
    if (data == null) return;

    data.disponibile = true;
    data.postiOccupati = 0;
    data.cognomePrenotazione = "";
    data.orarioPrenotazione = "";

    // 🔹 Chiamata al DB tramite manager
    if (tavoloManager != null)
    {
        tavoloManager.LiberaTavoloNelDB(data.id);
    }

    AggiornaUI();
    Debug.Log($"{data.nominativo} è stato liberato!");
}

}

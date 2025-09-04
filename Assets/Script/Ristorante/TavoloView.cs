using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TavoloView : MonoBehaviour
{
    [Header("UI Riferimenti")]
    public TextMeshProUGUI numeroTxt;
    public TextMeshProUGUI statoTxt;
    public Image background;

    [Header("Bottoni")]
    public Button liberaButton;
    public Button modificaButton;
    public TavoloOrdiniOpener tavoloOrdiniOpener; // assegna via Inspector


    private Tavolo data;
    private TavoloDettaglioView dettaglioUI;
    private TavoloPrenotazioneView prenotazioneUI;
    private TavoloFormUI tavoloFormUI;
    private TavoloManagerRuntime tavoloManager;

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
            btn.onClick.AddListener(OnTavoloClicked);
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
                        tavoloManager.AggiornaTavoloNelDB(data.id, nuovoNome, nuoviPosti);
                    },
                    () => Debug.Log("Modifica tavolo annullata"));
                }
            });
        }
    }

    private void OnTavoloClicked()
    {
        if (data == null) return;

        if (data.stato == StatoTavolo.Aperto)
        {
            if (tavoloOrdiniOpener != null)
            {
                tavoloOrdiniOpener.Bind(data);
                tavoloOrdiniOpener.apriButton.onClick.Invoke(); // forza il click
            }
            else
            {
                // fallback (non consigliato)
                TavoloCorrenteRegistry.tavoloAttivo = ScriptableObject.Instantiate(data);
                SceneManager.LoadScene("OrdiniScene");
            }
        }
        else if (data.stato == StatoTavolo.OrdineInviato)
        {
            if (tavoloOrdiniOpener != null)
            {
                tavoloOrdiniOpener.Bind(data);
                tavoloOrdiniOpener.apriButton.onClick.Invoke(); // forza il click
            }
            else
            {
                // fallback (non consigliato)
                TavoloCorrenteRegistry.tavoloAttivo = ScriptableObject.Instantiate(data);
                SceneManager.LoadScene("OrdiniScene");
            }
        }
        else if (data.stato == StatoTavolo.Prenotato)
        {
            dettaglioUI.MostraDettaglio(data);
        }
        else if (data.stato == StatoTavolo.Libero)
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
    }


    public void AggiornaUI()
    {
        if (data == null) return;

        numeroTxt.text = string.IsNullOrEmpty(data.nominativo)
            ? $"Tavolo {data.id}"
            : data.nominativo;

        switch (data.stato)
        {
            case StatoTavolo.Libero:
                statoTxt.text = $"Libero\n({data.numeroPosti} posti)";
                if (background != null) background.color = Color.gray;
                if (liberaButton != null) liberaButton.gameObject.SetActive(false);
                if (modificaButton != null) modificaButton.gameObject.SetActive(true);
                break;

            case StatoTavolo.Prenotato:
                statoTxt.text = $"{data.cognomePrenotazione}\nOre {data.orarioPrenotazione}\n{data.postiOccupati}/{data.numeroPosti}";
                if (background != null) background.color = Color.yellow;
                if (liberaButton != null) liberaButton.gameObject.SetActive(true);
                if (modificaButton != null) modificaButton.gameObject.SetActive(false);
                break;

            case StatoTavolo.Aperto:
                statoTxt.text = $"Aperto\n{data.postiOccupati}/{data.numeroPosti}";
                if (background != null) background.color = Color.green;
                if (liberaButton != null) liberaButton.gameObject.SetActive(true);
                if (modificaButton != null) modificaButton.gameObject.SetActive(false);
                break;
            case StatoTavolo.OrdineInviato:
                statoTxt.text = "Ordine Inviato";
                background.color = Color.blue; // o altro colore distintivo
                liberaButton?.gameObject.SetActive(false);
                modificaButton?.gameObject.SetActive(false);
                break;


        }
        
    }

    private void LiberaTavolo()
    {
        if (data == null) return;

        data.stato = StatoTavolo.Libero;
        data.postiOccupati = 0;
        data.cognomePrenotazione = "";
        data.orarioPrenotazione = "";

        if (tavoloManager != null)
            tavoloManager.LiberaTavoloNelDB(data.id);

        AggiornaUI();
        Debug.Log($"{data.nominativo} è stato liberato!");
    }

    void Update()
    {
        AggiornaUI();
    }
}

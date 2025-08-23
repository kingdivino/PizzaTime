using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TavoloDettaglioView : MonoBehaviour
{
    public TextMeshProUGUI titoloTxt;
    public TextMeshProUGUI statoTxt;
    public RectTransform ordiniContainer;
    public GameObject ordinePrefab;
    public Button btnChiudi;

    public GameObject salaViewPanel;   // ⬅️ il panel con la lista tavoli

    private Tavolo data;
    public TavoloOrdiniOpener ordiniOpener; // drag&drop

    void Awake()
    {
        btnChiudi.onClick.AddListener(Chiudi);
    }

    public void MostraDettaglio(Tavolo tavolo)
    {
        data = tavolo;

        // Attivo pannello dettaglio
        gameObject.SetActive(true);

        if (ordiniOpener != null) ordiniOpener.Bind(data); // abilita “Apri” anche su tavolo prenotato/occupato

        // Nascondo la lista tavoli
        if (salaViewPanel != null) salaViewPanel.SetActive(false);

        titoloTxt.text = $"Tavolo {data.id}";
        string statoBase = data.disponibile
            ? $"Libero ({data.numeroPosti} posti)"
            : $"Occupato {data.postiOccupati}/{data.numeroPosti}";

        if (!data.disponibile)
        {
            string cognome = string.IsNullOrWhiteSpace(data.cognomePrenotazione) ? "-" : data.cognomePrenotazione;
            string orario = string.IsNullOrWhiteSpace(data.orarioPrenotazione) ? "-" : data.orarioPrenotazione;
            statoTxt.text = $"{statoBase}\nPrenotazione: {cognome} • {orario}";
        }
        else
        {
            statoTxt.text = statoBase;
        }
        foreach (Transform c in ordiniContainer)
            Destroy(c.gameObject);

        if (data.prodottiOrdinati != null)
        {
            foreach (var p in data.prodottiOrdinati)
            {
                var go = Instantiate(ordinePrefab, ordiniContainer);
                go.GetComponent<TextMeshProUGUI>().text = $"{p.nome} - {p.prezzo}€";
            }
        }

        if (data.pizzeOrdinate != null)
        {
            foreach (var pizza in data.pizzeOrdinate)
            {
                var go = Instantiate(ordinePrefab, ordiniContainer);
                go.GetComponent<TextMeshProUGUI>().text = $"Pizza {pizza.name} - {pizza.prezzoTotale}€";
            }
        }
    }

    public void Chiudi()
    {
        gameObject.SetActive(false);

        // Riattivo la lista tavoli
        if (salaViewPanel != null) salaViewPanel.SetActive(true);
    }
}

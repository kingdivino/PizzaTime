using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class TavoloDettaglioView : MonoBehaviour
{
    public TextMeshProUGUI titoloTxt;
    public TextMeshProUGUI statoTxt;
    public RectTransform ordiniContainer;
    public GameObject ordinePrefab;
    public Button btnChiudi;

    public GameObject salaViewPanel;
    public TavoloOrdiniOpener ordiniOpener; // drag&drop

    private Tavolo data;
    private string apiUrl = "http://localhost:3000/tavoli"; // endpoint API

    void Awake()
    {
        btnChiudi.onClick.AddListener(Chiudi);
    }

    public void MostraDettaglio(Tavolo tavolo)
    {
        data = tavolo;

        gameObject.SetActive(true);

        if (ordiniOpener != null) ordiniOpener.Bind(data);
        if (salaViewPanel != null) salaViewPanel.SetActive(false);

        titoloTxt.text = $"Tavolo {data.id}";

        // 🔹 chiede al DB i dati aggiornati
        StartCoroutine(CaricaDettagliDalDB(data.id));
    }

private IEnumerator CaricaDettagliDalDB(int tavoloId)
{
    string url = $"{apiUrl}/{tavoloId}";

    using (UnityWebRequest req = UnityWebRequest.Get(url))
    {
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento tavolo: " + req.error);
            yield break;
        }

        TavoloDTO dto = JsonUtility.FromJson<TavoloDTO>(req.downloadHandler.text);

        // aggiorna oggetto in memoria
        data.nominativo = dto.nominativo;
        data.numeroPosti = dto.numero_posti;
        data.disponibile = dto.disponibile;
        data.postiOccupati = dto.posti_occupati;
        data.cognomePrenotazione = dto.cognome_prenotazione;
        data.orarioPrenotazione = dto.orario_prenotazione;

        AggiornaUI();
    }
}


    private void AggiornaUI()
    {
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
        if (salaViewPanel != null) salaViewPanel.SetActive(true);
    }

    [System.Serializable]
    private class TavoloDTO
    {
        public int id;
        public string nominativo;
        public int numero_posti;
        public bool disponibile;
        public int posti_occupati;
        public string cognome_prenotazione;
        public string orario_prenotazione;
        // aggiungi qui anche array di ordini se l’API li manda
    }
}

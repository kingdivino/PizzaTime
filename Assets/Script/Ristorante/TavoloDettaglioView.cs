using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class TavoloDettaglioView : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titoloTxt;
    public TextMeshProUGUI statoTxt;
    public RectTransform ordiniContainer;
    public GameObject ordinePrefab;
    public Button btnChiudi;

    public GameObject salaViewPanel;
    public TavoloOrdiniOpener ordiniOpener;

    [Header("Ordini JSON")]
    public Transform contenitoreOrdine;
    public GameObject rigaOrdinePrefab;

    private Tavolo data;
    private string apiUrl = "http://localhost:3000/tavoli";

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

        StartCoroutine(CaricaDettagliDalDB(data.id));
        MostraDettaglioOrdine(data.id);
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

    public void MostraDettaglioOrdine(int tavoloId)
    {
        StartCoroutine(CaricaOrdineDalDB(tavoloId));
    }

    private IEnumerator CaricaOrdineDalDB(int tavoloId)
    {
        string url = "http://localhost:3000/ordini?tavoloId=" + tavoloId;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Errore caricamento ordine: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            Debug.Log("📥 JSON ricevuto: " + json);

            OrderDTO ordine = JsonUtility.FromJson<OrderDTO>(json);
            PizzaDTO[] pizze = ParsePizze(ordine.pizze);

            foreach (Transform child in contenitoreOrdine)
                Destroy(child.gameObject);

            foreach (var pizza in pizze)
            {
                GameObject riga = Instantiate(rigaOrdinePrefab, contenitoreOrdine);
                ComponentiReference comp = riga.GetComponent<ComponentiReference>();

                if (comp == null)
                {
                    Debug.LogError("❌ Manca ComponentiReference sul prefab");
                    continue;
                }

                comp.nome.text = $"Pizza di {pizza.nome}";
                comp.prezzo.text = $"{pizza.prezzo:F2}€";
                comp.ingredienti.text = $"Impasto: {pizza.impasto_nome}\nIngredienti: {string.Join(", ", pizza.ingredienti ?? new string[0])}";
            }

            Debug.Log("✅ Ordine mostrato nel dettaglio tavolo");
        }
    }

    private PizzaDTO[] ParsePizze(string pizzeJsonString)
    {
        if (string.IsNullOrEmpty(pizzeJsonString))
            return new PizzaDTO[0];

        string cleanedJson = pizzeJsonString.Replace("\\\"", "\"").Trim();
        string wrappedJson = "{\"array\":" + cleanedJson + "}";

        PizzaWrapper wrapper = JsonUtility.FromJson<PizzaWrapper>(wrappedJson);
        return wrapper.array ?? new PizzaDTO[0];
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
    }

    [System.Serializable]
    public class PizzaDTO
    {
        public string nome;
        public float prezzo;
        public string impasto_nome;
        public string[] ingredienti;
    }

    [System.Serializable]
    private class PizzaWrapper
    {
        public PizzaDTO[] array;
    }

    [System.Serializable]
    public class OrderDTO
    {
        public int id;
        public int tavolo_id;
        public float prezzo_totale;
        public string pizze;      // stringa JSON dal DB
        public string prodotti;   // stringa JSON dal DB
    }
}

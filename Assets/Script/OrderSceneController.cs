using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OrderSceneController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI txtTavolo;
    public Button btnChiudi;        // torna alla scena Sala (o chiudi conto)
    public Button btnInviaOrdine;   // invia e resta in scena, o torna: scegli tu

    public TextMeshProUGUI txtTotale; // 🎯 Drag & Drop in Inspector

    public Transform contenitorePizzeOrdinate;
    public GameObject rigaPizza;

    public GameObject contenitoreProdotti;
    public GameObject rigaProdotto;

    private Tavolo tavolo;
    private string statoOrdineCorrente = "";

    private float prezzoEsistente = 0f;

    
    public TextMeshProUGUI txtStatoOrdine; // 🟢 Drag & drop in Inspector

    private List<ProdottoDB> prodottiDisponibili = new List<ProdottoDB>();
    private Dictionary<int, int> prodottiSelezionati = new Dictionary<int, int>();

    void Start()
    {
        tavolo = TavoloCorrenteRegistry.tavoloAttivo;
        if (tavolo == null)
        {
            Debug.LogWarning("Nessun tavolo attivo. Torno alle sale.");
            SceneManager.LoadScene("SaleScene");
            return;
        }

        if (txtTavolo) txtTavolo.text = tavolo.nominativo;

        if (btnChiudi)
            btnChiudi.onClick.AddListener(() => SceneManager.LoadScene("Ristorante"));

        if (btnInviaOrdine)
            btnInviaOrdine.onClick.AddListener(() =>
            {
                Debug.Log($"Ordine inviato per {tavolo.nominativo}");
                // SceneManager.LoadScene("SaleScene");
            });

        // 🔹 1. Carica dal DB e visualizza
        StartCoroutine(CaricaOrdineEsistente(tavolo.id));

        // 🔹 2. Mostra anche pizze create a runtime
        if (tavolo.ListaPizzeOrdinate.Count != 0)
        {
            foreach (Pizza p in tavolo.ListaPizzeOrdinate)
            {
                GameObject newriga = Instantiate(rigaPizza, contenitorePizzeOrdinate);
                ComponentiReference comp = newriga.GetComponent<ComponentiReference>();
                comp.nome.text = $"Pizza di {p.proprietario}";
                comp.prezzo.text = $"{p.prezzoTotale:F2}€";
                comp.ingredienti.text = "Impasto:" + p.impasto + "\nIngredienti: " + p.ingredienti.ToCommaSeparatedString();
            }
        }
        InvokeRepeating(nameof(RefreshStatoOrdine), 5f, 5f); // ogni 5 secondi

        AggiornaPrezzoTotale();

        StartCoroutine(CaricaProdotti());
    }


    public void OnClickOrdina()
    {
        StartCoroutine(SalvaOrdineNelDB(() =>
        {
            tavolo.stato = StatoTavolo.OrdineInviato;
            StartCoroutine(AggiornaStatoTavolo(tavolo.id, "OrdineInviato"));
        }));
    }

    public void onClickRichiediConto()
    {
        StartCoroutine(AggiornaStatoTavolo(tavolo.id, "RichiestaConto"));
        StartCoroutine(AggiornaStatoUltimoOrdine(tavolo.id, "RichiestaConto"));
    }





    private void AggiornaPrezzoTotale()
    {
        float totalePizze = tavolo.ListaPizzeOrdinate.Sum(p => p.GetPrezzo());
        float totaleProdotti = prodottiDisponibili
            .Where(p => prodottiSelezionati.ContainsKey(p.id))
            .Sum(p => p.prezzo * prodottiSelezionati[p.id]);

        float totaleFinale = prezzoEsistente + totalePizze + totaleProdotti;
        txtTotale.text = $"Totale: {totaleFinale:F2}€";
    }




    private IEnumerator SalvaOrdineNelDB(System.Action onSuccess = null)
    {
        string urlPost = "http://localhost:3000/ordini";

        var tavolo = TavoloCorrenteRegistry.tavoloAttivo;
        if (tavolo == null)
        {
            Debug.LogError("Nessun tavolo attivo!");
            yield break;
        }

        // 🚀 Invia SOLO le pizze nuove
        List<PizzaDTO> nuovePizze = new List<PizzaDTO>();
        foreach (var p in tavolo.ListaPizzeOrdinate)
        {
            PizzaDTO nuova = new PizzaDTO
            {
                nome = p.proprietario,
                prezzo = p.GetPrezzo(),
                impasto_nome = p.impasto.nome,
                ingredienti = p.ingredienti.Select(i => i.nome).ToArray()
            };
            nuovePizze.Add(nuova);
        }

        // 🔹 QUI inserisci il codice per i prodotti
        List<ProdottoDTO> prodottiOrdine = new List<ProdottoDTO>();
        foreach (var prod in prodottiDisponibili)
        {
            if (prodottiSelezionati.ContainsKey(prod.id) && prodottiSelezionati[prod.id] > 0)
            {
                prodottiOrdine.Add(new ProdottoDTO
                {
                    id = prod.id,
                    nome = prod.nome,
                    prezzo = prod.prezzo,
                    quantita = prodottiSelezionati[prod.id]
                });
            }
        }

        OrderDTO payload = new OrderDTO
        {
            tavolo_id = tavolo.id,
            prezzo_totale = nuovePizze.Sum(x => x.prezzo) + prodottiOrdine.Sum(p => p.prezzo * p.quantita),
            pizze = nuovePizze.ToArray(),
            prodotti = prodottiOrdine.ToArray()
        };

        string json = JsonUtility.ToJson(payload, true);
        Debug.Log("📤 JSON inviato: " + json);

        UnityWebRequest request = new UnityWebRequest(urlPost, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore salvataggio ordine: " + request.error +
                           "\nRisposta: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("✅ Ordine salvato nel DB: " + request.downloadHandler.text);
            tavolo.ListaPizzeOrdinate.Clear(); // pulisco runtime
            onSuccess?.Invoke();
        }
    }



    private IEnumerator CaricaOrdineEsistente(int tavoloId)
{
    string url = $"http://localhost:3000/ordini?tavoloId={tavoloId}";

    UnityWebRequest req = UnityWebRequest.Get(url);
    yield return req.SendWebRequest();

    if (req.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("❌ Errore caricamento ordine: " + req.error);
        yield break;
    }

    string json = req.downloadHandler.text;
    Debug.Log("📥 Ordine JSON: " + json);

    // ✅ Usa JsonHelper per leggere array
    OrdineDB[] ordini = JsonHelper.FromJson<OrdineDB>(json);

        foreach (var ordine in ordini)
        {
            if (ordine.stato == "Chiuso") continue; // ⛔ Salta ordini chiusi

            PizzaDTO[] pizze = ParsePizzeJson(ordine.pizze);

            foreach (var pizza in pizze)
            {
                GameObject riga = Instantiate(rigaPizza, contenitorePizzeOrdinate);
                ComponentiReference comp = riga.GetComponent<ComponentiReference>();

                comp.nome.text = $"Pizza di {pizza.nome}";
                comp.prezzo.text = $"{pizza.prezzo:F2}€";
                comp.ingredienti.text = $"Impasto: {pizza.impasto_nome}\nIngredienti: {string.Join(", ", pizza.ingredienti ?? new string[0])}";
            }

            prezzoEsistente += ordine.prezzo_totale;
        }

        AggiornaPrezzoTotale();
}

   private IEnumerator AggiornaStatoTavolo(int tavoloId, string nuovoStato)
{
    string url = $"http://localhost:3000/tavoli/{tavoloId}/stato";

    string json = $@"{{ ""stato"": ""{nuovoStato}"" }}";

    UnityWebRequest req = UnityWebRequest.Put(url, json);
    req.method = "PUT";
    req.SetRequestHeader("Content-Type", "application/json");
    req.downloadHandler = new DownloadHandlerBuffer();

    yield return req.SendWebRequest();

    if (req.result != UnityWebRequest.Result.Success)
        Debug.LogError($"❌ Errore aggiornamento stato '{nuovoStato}' per tavolo {tavoloId}: " + req.error);
    else
        Debug.Log($"✅ Stato tavolo {tavoloId} aggiornato a '{nuovoStato}'");
}

private void AggiornaStatoVisuale(string stato)
{
    if (txtStatoOrdine == null) return;

    txtStatoOrdine.text = $"Stato ordine: {stato}";

    switch (stato)
    {
        case "InAttesa":
            txtStatoOrdine.color = Color.yellow; // 🟡
            break;
        case "InPreparazione":
            txtStatoOrdine.color = new Color(1f, 0.6f, 0f); // 🟠 arancione
            break;
        case "Consegnato":
            txtStatoOrdine.color = Color.green; // 🟢
            break;
        case "Annullato":
            txtStatoOrdine.color = Color.red; // 🔴
            break;
        default:
            txtStatoOrdine.color = Color.white; // ⚪ fallback
            break;
    }
}

    private void RefreshStatoOrdine()
    {
        StartCoroutine(CaricaUltimoStatoOrdine(tavolo.id));
        StartCoroutine(VerificaPrioritàOrdineEAvviaPreparazione(tavolo.id));
}

    private IEnumerator CaricaUltimoStatoOrdine(int tavoloId)
    {
        string url = $"http://localhost:3000/ordini?tavoloId={tavoloId}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore refresh stato: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        OrdineDB[] ordini = JsonHelper.FromJson<OrdineDB>(json);

        // 🔄 Mostra lo stato dell'ultimo ordine (o il più recente "non consegnato")
        var ultimo = ordini.LastOrDefault();
        if (ultimo != null && ultimo.stato != statoOrdineCorrente)
        {
            statoOrdineCorrente = ultimo.stato;
            AggiornaStatoVisuale(ultimo.stato);
        }
    }

    private IEnumerator VerificaPrioritàOrdineEAvviaPreparazione(int tavoloId)
    {
        string url = $"http://localhost:3000/ordini?tavoloId={tavoloId}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore nel recupero ordini: " + req.error);
            yield break;
        }

        OrdineDB[] ordiniTavolo = JsonHelper.FromJson<OrdineDB>(req.downloadHandler.text);
        var ordineAttivo = ordiniTavolo.LastOrDefault(o => o.stato != "Consegnato");

        if (ordineAttivo == null)
        {
            Debug.Log("⚠️ Nessun ordine attivo da valutare.");
            yield break;
        }

        // 🔹 Carichiamo tutti gli ordini dalla cucina ordinati cronologicamente
        string allUrl = "http://localhost:3000/ordini/inviati";
        UnityWebRequest allReq = UnityWebRequest.Get(allUrl);
        yield return allReq.SendWebRequest();

        if (allReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore nel recupero ordini globali: " + allReq.error);
            yield break;
        }

        OrdineDB[] ordiniInviati = JsonHelper.FromJson<OrdineDB>(allReq.downloadHandler.text);

        // 🔎 Ordina per orario e prendi i primi 3
        var primiTre = ordiniInviati
            .Where(o => o.stato == "InAttesa" || o.stato == "OrdineInviato" || o.stato == "RichiestaConto" || o.stato == "InPreparazione")
            .OrderBy(o => o.orario_ordine)
            .Take(3)
            .ToList();

        if (primiTre.Any(o => o.id == ordineAttivo.id) && ordineAttivo.stato == "InAttesa")
        {
            Debug.Log($"📦 Ordine {ordineAttivo.id} è tra i primi 3 e in attesa → lo mettiamo in preparazione");

            string updateUrl = $"http://localhost:3000/ordini/{ordineAttivo.id}/stato";
            string json = $@"{{ ""stato"": ""InPreparazione"" }}";

            UnityWebRequest updateReq = UnityWebRequest.Put(updateUrl, json);
            updateReq.method = "PUT";
            updateReq.SetRequestHeader("Content-Type", "application/json");
            updateReq.downloadHandler = new DownloadHandlerBuffer();

            yield return updateReq.SendWebRequest();

            if (updateReq.result != UnityWebRequest.Result.Success)
                Debug.LogError("❌ Errore aggiornamento stato: " + updateReq.error);
            else
                Debug.Log($"✅ Ordine {ordineAttivo.id} aggiornato a InPreparazione");
        }
        else
        {
            Debug.Log($"ℹ️ Ordine {ordineAttivo.id} non aggiornato: stato attuale = {ordineAttivo.stato}");
        }

    }


    private IEnumerator AggiornaStatoUltimoOrdine(int tavoloId, string nuovoStato)
    {
        // recupera l’ultimo ordine NON consegnato
        string url = $"http://localhost:3000/ordini?tavoloId={tavoloId}";

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore nel recupero ordine: " + req.error);
            yield break;
        }

        OrdineDB[] ordini = JsonHelper.FromJson<OrdineDB>(req.downloadHandler.text);
        var ultimoAttivo = ordini.LastOrDefault(o => o.stato == "Consegnato");

        if (ultimoAttivo == null)
        {
            Debug.LogWarning("⚠️ Nessun ordine attivo da aggiornare.");
            yield break;
        }

        // ora aggiorna lo stato
        string updateUrl = $"http://localhost:3000/ordini/{ultimoAttivo.id}/stato";
        string json = $@"{{ ""stato"": ""{nuovoStato}"" }}";

        UnityWebRequest updateReq = UnityWebRequest.Put(updateUrl, json);
        updateReq.method = "PUT";
        updateReq.SetRequestHeader("Content-Type", "application/json");
        updateReq.downloadHandler = new DownloadHandlerBuffer();

        yield return updateReq.SendWebRequest();

        if (updateReq.result != UnityWebRequest.Result.Success)
            Debug.LogError("❌ Errore aggiornamento stato ordine: " + updateReq.error);
        else
            Debug.Log($"✅ Stato ordine {ultimoAttivo.id} aggiornato a '{nuovoStato}'");
    }



    [System.Serializable]
    public class OrdineDB
    {
        public int id;
        public int tavolo_id;
        public float prezzo_totale;
        public string pizze;
        public string prodotti;
        public string stato;
        public string orario_ordine;
    }




    [System.Serializable]
    public class PizzaWrapper
    {
        public PizzaDTO[] array;
    }


    private PizzaDTO[] ParsePizzeJson(string pizzeJson)
    {
        if (string.IsNullOrEmpty(pizzeJson)) return new PizzaDTO[0];
        string cleaned = pizzeJson.Replace("\\\"", "\"");
        string wrapped = "{\"array\":" + cleaned + "}";
        PizzaWrapper wrapper = JsonUtility.FromJson<PizzaWrapper>(wrapped);
        return wrapper.array;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            return JsonUtility.FromJson<Wrapper<T>>(newJson).array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }


    
    // idProdotto → quantità scelta

    private IEnumerator CaricaProdotti()
    {
        string url = "http://localhost:3000/prodotti"; // endpoint backend
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento prodotti: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        ProdottoDB[] prodotti = JsonHelper.FromJson<ProdottoDB>(json);
        prodottiDisponibili = new List<ProdottoDB>(prodotti);

        // Instanzia righe
        foreach (var prod in prodottiDisponibili)
        {
            GameObject riga = Instantiate(rigaProdotto, contenitoreProdotti.transform);
            RigaProdotti comp = riga.GetComponent<RigaProdotti>();

            comp.nome.text = prod.nome;
            comp.prezzo.text = $"{prod.prezzo:F2}€";
            comp.quantita.text = "0";

            prodottiSelezionati[prod.id] = 0;

            // 🔘 Gestione pulsanti
            comp.add.onClick.AddListener(() =>
            {
                prodottiSelezionati[prod.id]++;
                comp.quantita.text = prodottiSelezionati[prod.id].ToString();
                AggiornaPrezzoTotale();
            });

            comp.remove.onClick.AddListener(() =>
            {
                if (prodottiSelezionati[prod.id] > 0)
                {
                    prodottiSelezionati[prod.id]--;
                    comp.quantita.text = prodottiSelezionati[prod.id].ToString();
                    AggiornaPrezzoTotale();
                }
            });
        }
    }


}
[System.Serializable]
public class PizzaDTO
{
    public string nome;        // proprietario o nome pizza
    public float prezzo;
    public string impasto_nome;  // ora usiamo il nome, non l’id
    public string[] ingredienti;
}

[System.Serializable]
public class OrderDTO
{
    public int tavolo_id;
    public float prezzo_totale;
    public PizzaDTO[] pizze;
    public ProdottoDTO[] prodotti;
}


[System.Serializable]
public class ProdottoDB
{
    public int id;
    public string nome;
    public float prezzo;
    public int giacenza;
}

[System.Serializable]
public class ProdottoDTO
{
    public int id;
    public string nome;
    public float prezzo;
    public int quantita;
}


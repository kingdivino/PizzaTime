using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CassaSceneController : MonoBehaviour
{
    public TextMeshProUGUI txtTavoloNome;
    public TextMeshProUGUI txtTotale;
    public Transform contenitorePizze;
    public GameObject prefabRigaPizza;
    public Button btnPaga;

    private Tavolo tavolo;
    private float totaleOrdine = 0f;

    void Start()
    {
        tavolo = TavoloCorrenteRegistry.tavoloAttivo;

        if (tavolo == null)
        {
            Debug.LogError("❌ Nessun tavolo attivo, torno alla sala.");
            SceneManager.LoadScene("Ristorante"); // fallback
            return;
        }

        if (txtTavoloNome) txtTavoloNome.text = tavolo.nominativo;

        StartCoroutine(CaricaOrdiniDelTavolo(tavolo.id));

        btnPaga.onClick.AddListener(() =>
        {
            StartCoroutine(CompletaPagamento(tavolo.id));
        });
    }

    private IEnumerator CaricaOrdiniDelTavolo(int tavoloId)
    {
        string url = $"http://localhost:3000/ordini?tavoloId={tavoloId}";

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento ordini: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        OrdineDTO[] ordini = JsonHelper.FromJson<OrdineDTO>(json);

        foreach (Transform child in contenitorePizze)
            Destroy(child.gameObject);

        totaleOrdine = 0f;

        foreach (var ordine in ordini)
        {
            PizzaDTO[] pizze = ParsePizzeJson(ordine.pizze);

            foreach (var pizza in pizze)
            {
                GameObject riga = Instantiate(prefabRigaPizza, contenitorePizze);
                var comp = riga.GetComponent<ComponentiReference>();
                comp.nome.text = $"Pizza di {pizza.nome}";
                comp.prezzo.text = $"{pizza.prezzo:F2}€";
                comp.ingredienti.text = $"Impasto: {pizza.impasto_nome}\nIngredienti: {string.Join(", ", pizza.ingredienti)}";

                totaleOrdine += pizza.prezzo;
            }
        }

        if (txtTotale)
            txtTotale.text = $"Totale: {totaleOrdine:F2}€";
    }

private IEnumerator CompletaPagamento(int tavoloId)
{
    // 1. Libera tavolo
    string urlLibera = $"http://localhost:3000/tavoli/{tavoloId}/libera";
    UnityWebRequest req1 = UnityWebRequest.Put(urlLibera, "");
    req1.method = "PUT";
    req1.SetRequestHeader("Content-Type", "application/json");
    req1.downloadHandler = new DownloadHandlerBuffer();

    yield return req1.SendWebRequest();

    if (req1.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("❌ Errore liberazione tavolo: " + req1.error);
        yield break;
    }

    Debug.Log("✅ Tavolo liberato");

    // 2. Recupera gli ordini attivi del tavolo
    string urlGet = $"http://localhost:3000/ordini?tavoloId={tavoloId}";
    UnityWebRequest reqGet = UnityWebRequest.Get(urlGet);
    yield return reqGet.SendWebRequest();

    if (reqGet.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("❌ Errore nel recupero ordini: " + reqGet.error);
        yield break;
    }

    OrdineDTO[] ordini = JsonHelper.FromJson<OrdineDTO>(reqGet.downloadHandler.text);

    // 3. Aggiorna stato di ogni ordine a "Chiuso"
    foreach (var ordine in ordini)
    {
        string urlStato = $"http://localhost:3000/ordini/{ordine.id}/stato";
        string json = $@"{{ ""stato"": ""Chiuso"" }}";

        UnityWebRequest reqStato = UnityWebRequest.Put(urlStato, json);
        reqStato.method = "PUT";
        reqStato.SetRequestHeader("Content-Type", "application/json");
        reqStato.downloadHandler = new DownloadHandlerBuffer();

        yield return reqStato.SendWebRequest();

        if (reqStato.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Errore aggiornamento ordine {ordine.id}: " + reqStato.error);
        }
        else
        {
            Debug.Log($"✅ Ordine {ordine.id} aggiornato a 'Chiuso'");
        }
    }

    // 4. Salva ID sala per tornare correttamente
    if (tavolo != null)
    {
        SalaCorrenteRegistry.salaIdAttiva = tavolo.salaId;
        Debug.Log($"✅ Impostato SalaCorrenteRegistry.salaIdAttiva a {tavolo.salaId}");
    }

    // 5. Torna alla sala
    SceneManager.LoadScene("Ristorante");
}


    private PizzaDTO[] ParsePizzeJson(string pizzeJson)
    {
        if (string.IsNullOrEmpty(pizzeJson)) return new PizzaDTO[0];
        string cleaned = pizzeJson.Replace("\\\"", "\"").Trim();
        string wrapped = "{\"array\":" + cleaned + "}";
        PizzaWrapper wrapper = JsonUtility.FromJson<PizzaWrapper>(wrapped);
        return wrapper.array ?? new PizzaDTO[0];
    }

    [System.Serializable] public class PizzaWrapper { public PizzaDTO[] array; }

    [System.Serializable]
    public class OrdineDTO
    {
        public int id;
        public int tavolo_id;
        public string pizze;
        public string prodotti;
        public float prezzo_totale;
        public string stato;
    }

    [System.Serializable]
    public class PizzaDTO
    {
        public string nome;
        public float prezzo;
        public string impasto_nome;
        public string[] ingredienti;
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
}

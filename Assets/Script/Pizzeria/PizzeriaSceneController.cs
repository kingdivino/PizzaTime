using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class PizzeriaSceneController : MonoBehaviour
{
    public Transform contenitoreOrdini;
    public GameObject prefabOrdine;
    public GameObject prefabRigaPizza;

    void Start()
    {
        StartCoroutine(CaricaOrdiniInviati());
    }

    private IEnumerator CaricaOrdiniInviati()
    {
        string url = "http://localhost:3000/ordini/inviati";
        UnityWebRequest req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento ordini: " + req.error);
            yield break;
        }
        Debug.Log("📥 JSON ricevuto dal DB:\n" + req.downloadHandler.text);

        string json = req.downloadHandler.text;
        OrdineInviatoDTO[] ordini = JsonHelper.FromJson<OrdineInviatoDTO>(json);

        foreach (var ordine in ordini)
        {
            GameObject card = Instantiate(prefabOrdine, contenitoreOrdini);
            var refUI = card.GetComponent<OrdineCardUI>();
            if (refUI == null)
            {
                Debug.LogError("❌ Il prefabOrdine non contiene OrdineCardUI!");
                continue;
            }
            if (refUI == null || refUI.tavoloNome == null || refUI.orario == null || refUI.contenitorePizze == null)
            {
                Debug.LogError("❌ OrdineCardUI non è completo, controlla il prefab.");
                continue;
            }



            refUI.tavoloNome.text = ordine.tavolo_nome;
            if (System.DateTime.TryParse(ordine.orario_ordine, out var dt))
                refUI.orario.text = dt.ToString("Ora HH:mm");
            else
                refUI.orario.text = ordine.orario_ordine;

            var pizze = ParsePizzeJson(ordine.pizze);
            foreach (var pizza in pizze)
            {
                GameObject riga = Instantiate(prefabRigaPizza, refUI.contenitorePizze);
                var comp = riga.GetComponent<ComponentiReference>();
                comp.nome.text = $"Pizza di {pizza.nome}";
                comp.prezzo.text = $"{pizza.prezzo:F2}€";
                comp.ingredienti.text = $"Impasto: {pizza.impasto_nome}\nIngredienti: {string.Join(", ", pizza.ingredienti)}";
            }

            refUI.pronto.onClick.RemoveAllListeners();
            refUI.pronto.onClick.AddListener(() =>
            {
                Debug.Log($"✅ Tavolo {ordine.tavolo_id} segnato come 'Aperto'");

                StartCoroutine(CambiaStatoTavolo(ordine.tavolo_id, "Aperto"));

                // Rimuovi visivamente la card dalla lista
                Destroy(card);
            });


        }
    }
    private IEnumerator CambiaStatoTavolo(int tavoloId, string nuovoStato)
    {
        string url = $"http://localhost:3000/tavoli/{tavoloId}/apri"; // usa la tua rotta esistente

        string json = $@"
    {{
        ""stato"": ""{nuovoStato}""
    }}";

        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.method = "PUT";
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("❌ Errore aggiornamento stato tavolo: " + req.error);
        else
            Debug.Log("🔁 Stato tavolo aggiornato con successo");
    }



    [System.Serializable]
    public class OrdineInviatoDTO
    {
        public int id;
        public int tavolo_id;
        public float prezzo_totale;
        public string pizze;
        public string prodotti;
        public string orario_ordine;
        public string tavolo_nome;
    }

    private PizzaDTO[] ParsePizzeJson(string pizzeJson)
    {
        if (string.IsNullOrEmpty(pizzeJson)) return new PizzaDTO[0];

        string cleanedJson = pizzeJson.Replace("\\\"", "\"").Trim();
        string wrappedJson = "{\"array\":" + cleanedJson + "}";

        PizzaWrapper wrapper = JsonUtility.FromJson<PizzaWrapper>(wrappedJson);
        return wrapper.array ?? new PizzaDTO[0];
    }



    [System.Serializable] public class PizzaWrapper { public PizzaDTO[] array; }
    [System.Serializable]
    public class PizzaDTO
    {
        public string nome;
        public float prezzo;
        public string impasto_nome;
        public string[] ingredienti;
    }
}

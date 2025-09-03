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

    private Tavolo tavolo;
    private float prezzoEsistente = 0f;

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
    AggiornaPrezzoTotale();
}


    public void OnClickOrdina()
    {
        StartCoroutine(SalvaOrdineNelDB());
    }

private void AggiornaPrezzoTotale()
{
    float totaleNuovo = tavolo.ListaPizzeOrdinate.Sum(p => p.GetPrezzo());
    float totaleFinale = prezzoEsistente + totaleNuovo;
    txtTotale.text = $"Totale: {totaleFinale:F2}€";
}



private IEnumerator SalvaOrdineNelDB()
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

    OrderDTO payload = new OrderDTO
    {
        tavolo_id = tavolo.id,
        prezzo_totale = nuovePizze.Sum(x => x.prezzo),
        pizze = nuovePizze.ToArray(),
        prodotti = new string[0]
    };

    string json = JsonUtility.ToJson(payload, true);
    Debug.Log("📤 Solo pizze nuove JSON inviato: " + json);

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

        OrdineDB ordine = JsonUtility.FromJson<OrdineDB>(json);
        PizzaDTO[] pizze = ParsePizzeJson(ordine.pizze);

        foreach (var pizza in pizze)
        {
            GameObject riga = Instantiate(rigaPizza, contenitorePizzeOrdinate);
            ComponentiReference comp = riga.GetComponent<ComponentiReference>();

            comp.nome.text = $"Pizza di {pizza.nome}";
            comp.prezzo.text = $"{pizza.prezzo:F2}€";
            comp.ingredienti.text = $"Impasto: {pizza.impasto_nome}\nIngredienti: {string.Join(", ", pizza.ingredienti ?? new string[0])}";
        }
        prezzoEsistente = ordine.prezzo_totale;
        AggiornaPrezzoTotale();
}


[System.Serializable]
public class OrdineDB
{
    public int id;
    public int tavolo_id;
    public float prezzo_totale;
    public string pizze;
    public string prodotti;
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
    public string[] prodotti;
}




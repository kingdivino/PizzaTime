using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using System;
using System.Collections.Generic;
public class TavoloManagerRuntime : MonoBehaviour
{
    [Header("UI")]
    public SalaSelector salaSelector;
    public SalaView salaView;
    public TavoloFormUI tavoloFormUI;

    [Header("Prefab e contenitori")]
    public GameObject tavoloPrefab;
    public RectTransform contenitore;
    public Button addTavoloButton;

    [Header("Riferimenti")]
    public TavoloManagerRuntime tavoloManager; // Drag & drop in Inspector

    private string apiUrl = "http://localhost:3000/tavoli";

  public void OnAddTavoloClicked()
{
    Debug.Log("🟠 [+Tavolo] cliccato");

    if (tavoloFormUI == null || salaSelector == null || salaSelector.salaCorrente == null)
    {
        Debug.LogError("❌ Impossibile aprire il form: riferimenti mancanti.");
        return;
    }

    tavoloFormUI.ApriPerCreazione((nome, posti) =>
    {
        Debug.Log($"🟢 Conferma creazione → {nome}, {posti}");
        CreaTavoloNelDB(salaSelector.salaCorrente.id, nome, posti);
    },
    () =>
    {
        Debug.Log("🟡 Creazione tavolo annullata");
    });
}

    // 🔹 Metodo chiamabile anche da SalaView
    public void CreaTavoloNelDB(int salaId, string nome, int numeroPosti)
    {
        StartCoroutine(CreaRoutine(salaId, nome, numeroPosti));
    }

    private IEnumerator CreaRoutine(int salaId, string nome, int numeroPosti)
    {
        Debug.Log($"📤 [TavoloManager] Invio tavolo al DB: {nome} - {numeroPosti} posti");

        string json = $@"
        {{
            ""sala_id"": {salaId},
            ""nominativo"": ""{nome}"",
            ""numero_posti"": {numeroPosti},
            ""disponibile"": true,
            ""posti_occupati"": 0,
            ""cognome_prenotazione"": """",
            ""orario_prenotazione"": """"
        }}";

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore HTTP: " + request.error);
        }
        else
        {
            Debug.Log("✅ Tavolo salvato con successo nel DB");

            TavoloDTO creato = JsonUtility.FromJson<TavoloDTO>(request.downloadHandler.text);

            var nuovoSO = ScriptableObject.CreateInstance<Tavolo>();
            nuovoSO.id = creato.id;
            nuovoSO.nominativo = creato.nominativo;
            nuovoSO.numeroPosti = creato.numero_posti;
            nuovoSO.disponibile = creato.disponibile;
            nuovoSO.postiOccupati = creato.posti_occupati;
            nuovoSO.salaId = salaId;
            if (Enum.TryParse<StatoTavolo>(creato.stato, out var statoParsed))
                nuovoSO.stato = statoParsed;
            else
                nuovoSO.stato = StatoTavolo.Libero; // fallback di sicurezza


            var lista = salaSelector.salaCorrente.tavoli?.ToList() ?? new System.Collections.Generic.List<Tavolo>();
            lista.Add(nuovoSO);
            salaSelector.salaCorrente.tavoli = lista.ToArray();

            salaView.MostraSala(salaSelector.salaCorrente);
        }
    }
    public void AggiornaTavoloNelDB(int tavoloId, string nuovoNome, int nuoviPosti)
{
    StartCoroutine(UpdateRoutine(tavoloId, nuovoNome, nuoviPosti));
}

    private IEnumerator UpdateRoutine(int id, string nome, int numeroPosti)
    {
        Debug.Log($"✏️ [TavoloManager] Aggiorno tavolo ID={id} → Nome={nome}, Posti={numeroPosti}");

        string url = $"{apiUrl}/{id}"; // → es: http://localhost:3000/tavoli/5

        string json = $@"
    {{
        ""nominativo"": ""{nome}"",
        ""numero_posti"": {numeroPosti}
    }}";

        UnityWebRequest request = UnityWebRequest.Put(url, json);
        request.method = "PUT";
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore aggiornamento tavolo: " + request.error);
        }
        else
        {
            Debug.Log("✅ Tavolo aggiornato nel DB");

            // aggiorna oggetto in memoria
            var tavolo = salaSelector.salaCorrente.tavoli.FirstOrDefault(t => t.id == id);
            if (tavolo != null)
            {
                tavolo.nominativo = nome;
                tavolo.numeroPosti = numeroPosti;
            }

            salaView.MostraSala(salaSelector.salaCorrente);
        }
    
    
}

public void EliminaTavoloNelDB(int tavoloId)
{
    StartCoroutine(EliminaRoutine(tavoloId));
}

    private IEnumerator EliminaRoutine(int tavoloId)
    {
        string url = $"{apiUrl}/{tavoloId}";

        UnityWebRequest request = UnityWebRequest.Delete(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Errore eliminazione tavolo ID={tavoloId}: " + request.error);
        }
        else
        {
            Debug.Log($"✅ Tavolo ID={tavoloId} eliminato dal DB");

            // rimuovilo da salaCorrente
            var lista = salaSelector.salaCorrente.tavoli.ToList();
            var tavoloDaRimuovere = lista.FirstOrDefault(t => t.id == tavoloId);
            if (tavoloDaRimuovere != null)
                lista.Remove(tavoloDaRimuovere);

            salaSelector.salaCorrente.tavoli = lista.ToArray();
            salaView.MostraSala(salaSelector.salaCorrente);
        }
    }

public void CaricaNomiTavoli(Action<List<string>> callback)
{
    StartCoroutine(CaricaNomiRoutine(callback));
}

    private IEnumerator CaricaNomiRoutine(Action<List<string>> callback)
    {
        string url = "http://localhost:3000/tavoli/nominativi";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento nominativi: " + request.error);
            callback?.Invoke(new List<string>());
        }
        else
        {
            string json = request.downloadHandler.text;
            string[] nomi = JsonHelper.FromJson<string>(json); // o usa un parser JSON custom
            callback?.Invoke(nomi.ToList());
        }
    }
public void LiberaTavoloNelDB(int tavoloId)
{
    StartCoroutine(LiberaRoutine(tavoloId));
}

private IEnumerator LiberaRoutine(int tavoloId)
{
    string url = $"{apiUrl}/{tavoloId}/libera";

    UnityWebRequest request = UnityWebRequest.Put(url, "{}");
    request.method = "PUT";
    request.SetRequestHeader("Content-Type", "application/json");
    request.downloadHandler = new DownloadHandlerBuffer();

    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("❌ Errore liberazione tavolo: " + request.error);
    }
    else
    {
        Debug.Log("✅ Tavolo liberato nel DB");
    }
}


// fix per array JSON root non supportati da Unity
private string FixJsonArray(string json)
{
    return "{\"Items\":" + json + "}";
}

[System.Serializable]
private class JsonArrayWrapper<T>
{
    public T[] Items;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>("{\"Items\":" + json + "}");
        return wrapper.Items;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}


    [System.Serializable]
    public class TavoloDTO
    {
        public int id;
        public string nominativo;
        public int numero_posti;
        public bool disponibile;
        public int posti_occupati;
        public int sala_id;
        public string cognome_prenotazione;
        public string orario_prenotazione;
        public string stato; // ✅ AGGIUNTO

    }
}

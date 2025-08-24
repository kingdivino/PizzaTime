using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TavoliAPI : MonoBehaviour
{
    private string baseUrl = "http://localhost:3000";

    void Start()
    {
        StartCoroutine(GetTavoli());
    }

    IEnumerator GetTavoli()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/tavoli"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Errore: " + www.error);
            }
            else
            {
                Debug.Log("Risposta: " + www.downloadHandler.text);
                // qui puoi deserializzare JSON → oggetti C#
            }
        }
    }

    public IEnumerator InviaOrdine(int tavoloId, int prodottoId, int quantita)
    {
        string json = JsonUtility.ToJson(new {
            tavolo_id = tavoloId,
            prodotto_id = prodottoId,
            quantita = quantita
        });

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(baseUrl + "/ordini", json))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Errore: " + www.error);
            else
                Debug.Log("Ordine inviato: " + www.downloadHandler.text);
        }
    }
}

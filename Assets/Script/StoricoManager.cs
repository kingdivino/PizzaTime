using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class StoricoManager : MonoBehaviour
{
    [Header("UI")]
    public Transform contenitore;      // contenitore verticale (ScrollView/Panel)
    public GameObject rigaPrefab;      // prefab con 3 campi TextMeshProUGUI: data, num_pizze, totale

    private string apiUrl = "http://localhost:3000/storico";

    void Start()
    {
        StartCoroutine(CaricaStorico());
    }

    private IEnumerator CaricaStorico()
    {
        UnityWebRequest req = UnityWebRequest.Get(apiUrl);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento storico: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        StoricoDTO[] storico = JsonHelper.FromJson<StoricoDTO>(json);

        foreach (var s in storico)
        {
            GameObject riga = Instantiate(rigaPrefab, contenitore);
            var texts = riga.GetComponentsInChildren<TextMeshProUGUI>();

            // assumiamo ordine: Data - NumPizze - Totale
            texts[0].text = s.data.Length >= 10 ? s.data.Substring(0, 10) : s.data;
            texts[1].text = s.num_pizze.ToString();
            texts[2].text = $"{s.totale:F2}€";
        }
    }
}

[System.Serializable]
public class StoricoDTO {
    public string data;
    public int num_pizze;
    public float totale;
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

// DTO per ricevere le sale da JSON
[System.Serializable]
public class SalaDTO {
    public int id;
    public string nome;
}

public class SalaLoader : MonoBehaviour
{
    [Header("UI")]
    public Transform menuSaleContainer;   // contenitore dei bottoni sale
    public Button salaButtonTemplate;     // prefab pulsante sala

    [Header("Riferimenti")]
    public SalaSelector salaSelector;     // gestore UI sale

    private string apiUrl = "http://localhost:3000/sale";

    void Start()
    {
        StartCoroutine(CaricaSale());
    }

    private IEnumerator CaricaSale()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Errore API: " + www.error);
                yield break;
            }

            // ottengo risposta JSON
            string json = www.downloadHandler.text;
            SalaDTO[] sale = JsonHelper.FromJson<SalaDTO>(json);

            Debug.Log("Sale trovate dal DB: " + sale.Length);

            foreach (var s in sale)
            {
                var btnObj = Instantiate(salaButtonTemplate, menuSaleContainer);
                btnObj.name = $"Btn_Sala_{s.id}";

                var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null)
                    txt.text = s.nome;

                var salaLocal = s;
                btnObj.onClick.RemoveAllListeners();
                btnObj.onClick.AddListener(() =>
                {
                    // nuovo metodo su SalaSelector che gestisce sala dal DB
                    salaSelector.EntraInSalaDB(salaLocal.id);
                });

                Debug.Log($"Creato bottone per sala '{s.nome}'");
            }
        }
    }
}

// helper per array JSON (Unity JsonUtility non supporta array di root)
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

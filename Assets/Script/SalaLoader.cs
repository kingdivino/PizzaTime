using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class SalaDTO {
    public int id;
    public string nome;
}

public class SalaLoader : MonoBehaviour
{
    public Transform menuSaleContainer;
    public Button salaButtonTemplate;
    public SalaSelector salaSelector;

    private string apiUrl = "http://localhost:3000/sale";

    void Start() {
        StartCoroutine(CaricaSale());
    }

    IEnumerator CaricaSale()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Errore API: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                SalaDTO[] sale = JsonHelper.FromJson<SalaDTO>(json);

                Debug.Log("Sale trovate: " + sale.Length);

                foreach (var s in sale)
                {
                    var btnObj = Instantiate(salaButtonTemplate, menuSaleContainer);
                    btnObj.name = $"Btn_Sala_{s.id}";
                    var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null) txt.text = s.nome;

                    var salaLocal = s;
                    btnObj.onClick.RemoveAllListeners();
                    btnObj.onClick.AddListener(() => {
                        salaSelector.EntraInSalaDB(salaLocal.id);
                    });

                    Debug.Log($"Creato bottone per sala '{s.nome}'");
                }
            }
        }
    }
}

// helper per array JSON
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

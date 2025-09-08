using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class MagazzinoManager : MonoBehaviour
{
    public Transform contenitore;
    public GameObject rigaPrefab;

    void Start()
    {
        StartCoroutine(CaricaIngredienti());
    }

    private IEnumerator CaricaIngredienti()
    {
        string url = "http://localhost:3000/ingredienti";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento ingredienti: " + req.error);
            yield break;
        }

        IngredienteDTO[] ingredienti = JsonHelper.FromJson<IngredienteDTO>(req.downloadHandler.text);

        foreach (var ingr in ingredienti)
        {
            GameObject riga = Instantiate(rigaPrefab, contenitore);
            RigaIngredienteUI ui = riga.GetComponent<RigaIngredienteUI>();
            rigaPrefab.SetActive(true);
            ui.Bind(ingr);
        }
    }
}
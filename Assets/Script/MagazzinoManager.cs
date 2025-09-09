using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class MagazzinoManager : MonoBehaviour
{
public Transform contenitore;
public GameObject rigaPrefab;

public Transform contenitoreProdotti;
public GameObject rigaProdottoPrefab;

void Start()
{
    StartCoroutine(CaricaIngredienti());
    StartCoroutine(CaricaProdotti());
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
        ui.Bind(ingr);
    }
}

private IEnumerator CaricaProdotti()
{
    string url = "http://localhost:3000/prodotti";
    UnityWebRequest req = UnityWebRequest.Get(url);
    yield return req.SendWebRequest();

    if (req.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("❌ Errore caricamento prodotti: " + req.error);
        yield break;
    }

    ProdottoDB[] prodotti = JsonHelper.FromJson<ProdottoDB>(req.downloadHandler.text);

    foreach (var prod in prodotti)
    {
        GameObject riga = Instantiate(rigaProdottoPrefab, contenitoreProdotti);
        RigaProdottoUI ui = riga.GetComponent<RigaProdottoUI>();
        ui.Bind(prod);
    }
}

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class ConsumoIngredienteDTO
{
    public int id;
    public int quantita;
}


public class Riepilogo : MonoBehaviour
{
    public IngredientSelector IngredientSelector;
    public ImpastoSelector ImpastoSelector;
    public TextMeshProUGUI testoprezzo;
    private float totPrezzo = 0f;

    public Transform areaComponenti;
    public GameObject componente;
    public TMP_InputField inputField;
    private string nome;

    private List<GameObject> listaComponenti = new List<GameObject>();
    private Pizza newPizza = null;

    private string apiUrlConsumo = "http://localhost:3000/ingredienti";

    public void UpdateRiepilogo()
    {
        CancellaComponenti();
        Impasto impasto = null;
        if (ImpastoSelector.GetImpastoSelezionato() != null)
        {
            impasto = ImpastoSelector.GetImpastoSelezionato();
            GameObject impastocomp = Instantiate(componente, areaComponenti);
            impastocomp.GetComponent<ComponentiReference>().nome.text = impasto.nome;
            impastocomp.GetComponent<ComponentiReference>().prezzo.text = $"{impasto.prezzo:F2}€";
            listaComponenti.Add(impastocomp);

        }

        foreach (Ingrediente ingrediente in IngredientSelector.GetSelectedIngredients())
        {
            GameObject newcomp = Instantiate(componente, areaComponenti);
            newcomp.GetComponent<ComponentiReference>().nome.text = ingrediente.nome;
            newcomp.GetComponent<ComponentiReference>().prezzo.text = $"{ingrediente.prezzo:F2}€";
            listaComponenti.Add(newcomp);
        }
        if (inputField.text == "")
            nome = "Baccal�";
        else
            nome = inputField.text;
        newPizza = ScriptableObject.CreateInstance<Pizza>();
        newPizza.Init(impasto, IngredientSelector.GetSelectedIngredients().ToArray(), nome);
        
        
        totPrezzo = newPizza.GetPrezzo();
        testoprezzo.text = $"{totPrezzo:F2}€";
    }

    public Pizza GetSelectedPizza()
    {
        return newPizza;
    }

    private void CancellaComponenti()
    {
        foreach (GameObject newcomp in listaComponenti)
        {
            Destroy(newcomp);
        }
        listaComponenti.Clear();
        totPrezzo = 0f;
    }

    public void OrdinaPizza()
    {
        if (newPizza.impasto == null)
        {
            Debug.Log("Seleziona almeno un impasto per ordinare");
            return;
        }

        Debug.Log($"Pizza creata da {newPizza.proprietario}: {newPizza.impasto.nome}, prezzo = {newPizza.GetPrezzo()}€");

        TavoloCorrenteRegistry.tavoloAttivo.ListaPizzeOrdinate.Add(newPizza);

        // 📌 Consuma ogni ingrediente secondo lo stile OrderSceneController
        foreach (var ingrediente in newPizza.ingredienti)
        {
            DatabaseManager.Instance.ConsumaIngrediente(ingrediente);
        }

        // Vai alla scena ordini
        SceneManager.LoadScene("OrdiniScene");
    }


    private IEnumerator ConsumaIngrediente(Ingrediente ingrediente, int quantita)
    {
        ConsumoIngredienteDTO dto = new ConsumoIngredienteDTO
        {
            id = ingrediente.id,
            quantita = quantita
        };

        string jsonData = JsonUtility.ToJson(dto);

        using (UnityWebRequest www = UnityWebRequest.Put(apiUrlConsumo, jsonData))
        {
            www.method = "POST"; // 🔹 forza POST
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Errore consumo ingrediente {ingrediente.nome}: {www.error}");
            }
            else
            {
                Debug.Log($"✅ Consumata 1 unità di {ingrediente.nome} (ID={ingrediente.id})");
            }
        }
    }


}

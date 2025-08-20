
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Riepilogo : MonoBehaviour
{
    public IngredientSelector IngredientSelector;
    public ImpastoSelector ImpastoSelector;
    public TextMeshProUGUI testoprezzo;
    private float totPrezzo = 0f;

    public Transform areaComponenti;
    public GameObject componente;

    private List<GameObject> listaComponenti = new List<GameObject>();
    private Pizza newPizza = null;

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
            //totPrezzo += impasto.prezzo;
            listaComponenti.Add(impastocomp);

        }

        foreach (Ingrediente ingrediente in IngredientSelector.GetSelectedIngredients())
        {
            GameObject newcomp = Instantiate(componente, areaComponenti);
            newcomp.GetComponent<ComponentiReference>().nome.text = ingrediente.nome;
            newcomp.GetComponent<ComponentiReference>().prezzo.text = $"{ingrediente.prezzo:F2}€";
            listaComponenti.Add(newcomp);
            //totPrezzo += ingrediente.prezzo;
        }

        //newPizza = new Pizza(impasto, IngredientSelector.GetSelectedIngredients().ToArray(), totPrezzo);
        newPizza = ScriptableObject.CreateInstance<Pizza>();
        newPizza.Init(impasto, IngredientSelector.GetSelectedIngredients().ToArray());
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
            Debug.Log("Seleziona Almeno un Impasto per ordinare");
            return;
        }
        Debug.Log($"Pizza creata: {newPizza.impasto.nome}, prezzo = {newPizza.GetPrezzo()}€");
    }
}


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

    public GameObject pannelloErrore;
    public TextMeshProUGUI testoErrore; 

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

        DatabaseManager.Instance.ConsumaIngredientiEOrdina(
            newPizza,
            () =>
            {
                // ✅ Ordine ok, aggiungi pizza al tavolo e cambia scena
                TavoloCorrenteRegistry.tavoloAttivo.ListaPizzeOrdinate.Add(newPizza);
                SceneManager.LoadScene("OrdiniScene");
            },
            (ingredientiMancanti) =>
            {
                // ❌ Mostra pannello errore
                pannelloErrore.SetActive(true);
                testoErrore.text = "Ingredienti mancanti:\n" + string.Join("\n", ingredientiMancanti);
            }
        );
    }


}

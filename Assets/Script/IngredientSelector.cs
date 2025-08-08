using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IngredientSelector : MonoBehaviour
{
    private List<GameObject> ingredientToggles = new List<GameObject>(); 
    public int maxIngredients = 8;

    private List<string> selectedIngredients = new List<string>();

    public Menu menu; 
    public GameObject togglePrefab; // un prefab di un Toggle
    public Transform toggleParent; // il contenitore con Vertical Layout Group

    void Start()
    {
        foreach (var ingrediente in menu.ingredienti)
        {
            GameObject newToggle = Instantiate(togglePrefab, toggleParent);
            newToggle.GetComponentInChildren<Text>().text = ingrediente.nome;
            ingredientToggles.Add(newToggle);
            // se vuoi collegare dati extra puoi fare:
            Toggle toggleComp = newToggle.GetComponent<Toggle>();
            toggleComp.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    Debug.Log("Selezionato: " + ingrediente.nome);

                }
            });
        }

        //foreach (var toggle in ingredientToggles)
        //{
        //    toggle.GetComponent<Toggle>().onValueChanged.AddListener(delegate { OnToggleChanged(toggle.GetComponent<Toggle>()); });
        //}
    }

    void OnToggleChanged(Toggle changedToggle)
    {
        string ingredient = changedToggle.GetComponentInChildren<Text>().text;

        if (changedToggle.isOn)
        {
            if (selectedIngredients.Count < maxIngredients)
            {
                selectedIngredients.Add(ingredient);
            }
            else
            {
                changedToggle.isOn = false; // blocco selezione
            }
        }
        else
        {
            selectedIngredients.Remove(ingredient);
        }
    }

    public List<string> GetSelectedIngredients()
    {
        return new List<string>(selectedIngredients);
    }

}

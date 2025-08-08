using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IngredientSelector : MonoBehaviour
{
    private List<GameObject> ingredientToggles = new List<GameObject>(); 
    public int maxIngredients = 8;

    private List<string> selectedIngredients = new List<string>();

    private float totale = 0f;
    public TextMeshProUGUI totaleText;
    public Menu menu; 
    public GameObject togglePrefab; // un prefab di un Toggle
    public Transform toggleParent; // il contenitore con Vertical Layout Group

    void Start()
    {
        foreach (var ingrediente in menu.ingredienti)
        {
            GameObject newToggle = Instantiate(togglePrefab, toggleParent);
            newToggle.GetComponentInChildren<ToggleIngredienti>().nome.text = ingrediente.nome;
            newToggle.GetComponent<ToggleIngredienti>().prezzo.text = $"{ingrediente.prezzo:F2}€";
            ingredientToggles.Add(newToggle);
            // se vuoi collegare dati extra puoi fare:
            Toggle toggleComp = newToggle.GetComponent<Toggle>();
            toggleComp.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    Debug.Log("Selezionato: " + ingrediente.nome);
                    totale += ingrediente.prezzo;
                    totaleText.text = $"{totale:F2}€";
                }
                else
                {
                    totale -= ingrediente.prezzo;
                    totaleText.text = $"{totale:F2}€";
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

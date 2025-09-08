using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IngredientSelector : MonoBehaviour
{
    private List<GameObject> ingredientToggles = new List<GameObject>();
    public int maxIngredients = 8;
    private int ingredientCount = 0;

    // uso HashSet per evitare duplicati e ricerche costose
    private HashSet<string> selectedIngredients = new HashSet<string>();
    private List<Ingrediente> ingredientiSelezionati = new List<Ingrediente>();

    private float totale = 0f;
    public Riepilogo riepilogo;
    public TextMeshProUGUI totaleText;
    public Menu menu;
    public GameObject togglePrefab; 
    public Transform toggleParent; 
    public PizzaBuilder pizzaVisual;

    // fallback guard (usato solo se non vuoi/puoi usare SetIsOnWithoutNotify)
    private bool suppressToggleCallbacks = false;

    void Start()
    {
        if (menu == null || menu.ingredienti == null)
        {
            Debug.LogError("Menu o menu.ingredienti non assegnati.");
            return;
        }

        foreach (var ingrediente in menu.ingredienti)
        {
            // copia locale per la closure
            var ingr = ingrediente;

            GameObject newToggle = Instantiate(togglePrefab, toggleParent);

            // Se hai un componente che contiene i riferimenti UI, impostali
            var toggleView = newToggle.GetComponentInChildren<ToggleIngredienti>();
            if (toggleView != null)
            {
                toggleView.nome.text = ingr.nome;
                toggleView.prezzo.text = $"{ingr.prezzo:F2}�";
            }

            ingredientToggles.Add(newToggle);

            Toggle toggleComp = newToggle.GetComponent<Toggle>();
            if (toggleComp == null) continue;

            // Listener principale
            toggleComp.onValueChanged.AddListener(isOn =>
            {
                if (suppressToggleCallbacks) return; // fallback guard

                if (isOn)
                {
                    if (ingredientCount >= maxIngredients)
                    {
                        // disattiva il toggle senza richiamare il listener
                        toggleComp.SetIsOnWithoutNotify(false);

                        Debug.Log($"Hai raggiunto il massimo di {maxIngredients} ingredienti.");
                        return;
                    }

                    // aggiungo solo se non � gi� presente
                    //if (selectedIngredients.Add(ingr.nome))
                    //{
                    //    ingredientCount++;
                    //    totale += ingr.prezzo;
                    //    totale = Mathf.Max(0f, totale);
                    //    UpdateTotaleText();
                    //    Debug.Log($"Selezionato: {ingr.nome} - Tot: {ingredientCount}");
                    //}
                    selectedIngredients.Add(ingr.nome);
                    ingredientiSelezionati.Add(ingr);
                    pizzaVisual.AddIngredient(ingrediente);
                    
                    ingredientCount++;
                    totale += ingr.prezzo;
                    totale = Mathf.Max(0f, totale);
                    UpdateTotaleText();
                    Debug.Log($"Selezionato: {ingr.nome} - Tot: {ingredientCount}");
                    riepilogo.UpdateRiepilogo();
                    
                }
                else // deselect
                {
                    // rimuovo solo se era stato realmente selezionato
                    //if (selectedIngredients.Remove(ingr.nome))
                    //{
                    //    ingredientCount = Mathf.Max(0, ingredientCount - 1);
                    //    totale -= ingr.prezzo;
                    //    totale = Mathf.Max(0f, totale);
                    //    UpdateTotaleText();
                    //    Debug.Log($"Deselezionato: {ingr.nome} - Tot: {ingredientCount}");
                    //}
                    selectedIngredients.Remove(ingr.nome);
                    ingredientiSelezionati.Remove(ingr);
                    pizzaVisual.RemoveIngredient(ingrediente);
                    
                    ingredientCount = Mathf.Max(0, ingredientCount - 1);
                    totale -= ingr.prezzo;
                    totale = Mathf.Max(0f, totale);
                    UpdateTotaleText();
                    Debug.Log($"Deselezionato: {ingr.nome} - Tot: {ingredientCount}");
                    riepilogo.UpdateRiepilogo();
                }
            });
        }

        UpdateTotaleText();
    }

    private void UpdateTotaleText()
    {
        if (totaleText != null) totaleText.text = $"{totale:F2}�";
    }

    public List<Ingrediente> GetSelectedIngredients()
    {
        return new List<Ingrediente>(ingredientiSelezionati);
    }
}

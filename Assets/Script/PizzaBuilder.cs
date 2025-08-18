using System.Collections.Generic;
using UnityEngine;

public class PizzaBuilder : MonoBehaviour
{
    [Header("Riferimenti gerarchia")]
    public Transform ingredientParent; 
    public float pizzaRadius = 1.5f;   // raggio entro cui distribuire gli ingredienti

    private List<GameObject> ingredientInstances = new List<GameObject>();

    // Aggiunge un ingrediente visivo alla pizza.
    public void AddIngredient(Ingrediente ingrediente)
    {
        if (ingrediente.sprite == null)
        {
            Debug.LogWarning($"Ingrediente {ingrediente.nome} non ha sprite!");
            return;
        }

        if (ingrediente.singolo)
        {
            // caso unico: 1 sprite centrato
            Quaternion randRot = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            CreateIngredientInstance(ingrediente, Vector3.zero, randRot);
        }
        else
        {
            // caso multiplo: N istanze random
            int count = Random.Range(4, 9); // range 4–8 incluso
            for (int i = 0; i < count; i++)
            {
                Vector2 randPos = Random.insideUnitCircle * pizzaRadius;
                Quaternion randRot = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

                CreateIngredientInstance(ingrediente, randPos, randRot);
            }
        }
    }

    // Rimuove tutte le istanze di un certo ingrediente.
    public void RemoveIngredient(Ingrediente ingrediente)
    {
        var toRemove = ingredientInstances.FindAll(obj => obj.name.StartsWith(ingrediente.nome));
        foreach (var obj in toRemove)
        {
            ingredientInstances.Remove(obj);
            Destroy(obj);
        }
    }

    // Svuota completamente la pizza.
    public void ClearPizza()
    {
        foreach (var obj in ingredientInstances)
            Destroy(obj);

        ingredientInstances.Clear();
    }

    // Crea un'istanza di ingrediente sulla pizza.
    private void CreateIngredientInstance(Ingrediente ingrediente, Vector3 localPos, Quaternion rot)
    {
        GameObject newIng = new GameObject(ingrediente.nome);
        newIng.transform.SetParent(ingredientParent, false);
        newIng.transform.localPosition = localPos;
        newIng.transform.localRotation = rot;

        SpriteRenderer sr = newIng.AddComponent<SpriteRenderer>();
        sr.sprite = ingrediente.sprite;
        sr.sortingOrder = ingrediente.layer;

        ingredientInstances.Add(newIng);
    }
}

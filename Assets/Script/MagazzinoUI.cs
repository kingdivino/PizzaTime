// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class MagazzinoUI : MonoBehaviour
// {
//     [Header("Refs")]
//     public RectTransform panelIngredienti; // il PanelIngredienti (parent)
//     public GameObject rigaPrefab;          // il prefab RigaIngrediente
//     public List<Ingrediente> ingredienti;  // trascina qui gli asset

//     void Start()
//     {
//         if (panelIngredienti == null || rigaPrefab == null)
//         {
//             Debug.LogError("[MagazzinoUI] Mancano riferimenti (panelIngredienti o rigaPrefab).");
//             return;
//         }

//         if (ingredienti == null || ingredienti.Count == 0)
//         {
//             Debug.LogWarning("[MagazzinoUI] Lista ingredienti vuota.");
//             return;
//         }

//         foreach (var ing in ingredienti)
//         {
//             var go = Instantiate(rigaPrefab, panelIngredienti);
//             var riga = go.GetComponent<RigaIngredienteUI>();
//             riga.Bind(ing);
//         }
//     }
// }

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ImpastoSelector : MonoBehaviour
{
    [Header("Riferimenti dati")]
    public Menu menu;  // collega qui il tuo ScriptableObject "Menu" con dentro impasti
    public Riepilogo riepilogo;

    [Header("UI")]
    public TMP_Dropdown dropdown;       // il dropdown nella UI
    public TextMeshProUGUI prezzoText;  // testo per mostrare il prezzo selezionato
    private Sprite impastobaseSprite;
    public SpriteRenderer impastobase;

    private Impasto impastoSelezionato;

    void Start()
    {
        if (menu == null || menu.impasti == null || menu.impasti.Length == 0)
        {
            Debug.LogError("Menu o lista impasti non assegnati!");
            return;
        }

        impastobaseSprite = impastobase.sprite;

        dropdown.ClearOptions();

        List<string> options = new List<string>();

        options.Add("Seleziona impasto");

        foreach (var impasto in menu.impasti)
        {
            options.Add(impasto.nome);
        }

        dropdown.AddOptions(options);

        dropdown.onValueChanged.AddListener(OnImpastoChanged);

        dropdown.value = 0;
        prezzoText.text = ""; // nessun prezzo finché non scelgono
    }

    void OnImpastoChanged(int index)
    {
        if (index == 0)
        {
            impastoSelezionato = null;
            //cambio sprite all'originale
            impastobase.sprite = impastobaseSprite;
            prezzoText.text = "";
            Debug.Log("Nessun impasto selezionato");
            riepilogo.UpdateRiepilogo();
            return;
        }

        // altrimenti prendi l'impasto corrispondente (index - 1 perché 0 è il placeholder)
        impastoSelezionato = menu.impasti[index - 1];
        //assegnazione sprite impasto
        impastobase.sprite = menu.impasti[index-1].sprite;

        if (prezzoText != null)
            prezzoText.text = $"{impastoSelezionato.prezzo:F2}€";

        Debug.Log($"Impasto selezionato: {impastoSelezionato.nome}");
        riepilogo.UpdateRiepilogo();
    }


    public Impasto GetImpastoSelezionato()
    {
        return impastoSelezionato;
    }
}

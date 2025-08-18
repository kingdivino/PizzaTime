using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ImpastoSelector : MonoBehaviour
{
    [Header("Riferimenti dati")]
    public Menu menu;  // collega qui il tuo ScriptableObject "Menu" con dentro impasti

    [Header("UI")]
    public TMP_Dropdown dropdown;       // il dropdown nella UI
    public TextMeshProUGUI prezzoText;  // testo per mostrare il prezzo selezionato

    private Impasto impastoSelezionato;

    void Start()
    {
        if (menu == null || menu.impasti == null || menu.impasti.Length == 0)
        {
            Debug.LogError("Menu o lista impasti non assegnati!");
            return;
        }

        dropdown.ClearOptions();

        List<string> options = new List<string>();

        // 👇 prima voce "placeholder"
        options.Add("Seleziona impasto");

        // poi aggiungi gli impasti reali
        foreach (var impasto in menu.impasti)
        {
            options.Add(impasto.nome);
        }

        dropdown.AddOptions(options);

        // listener
        dropdown.onValueChanged.AddListener(OnImpastoChanged);

        // imposta default sul placeholder
        dropdown.value = 0;
        prezzoText.text = ""; // nessun prezzo finché non scelgono
    }

    void OnImpastoChanged(int index)
    {
        // 👇 se è la prima voce, non fare nulla
        if (index == 0)
        {
            impastoSelezionato = null;
            prezzoText.text = "";
            Debug.Log("Nessun impasto selezionato");
            return;
        }

        // altrimenti prendi l'impasto corrispondente (index - 1 perché 0 è il placeholder)
        impastoSelezionato = menu.impasti[index - 1];

        if (prezzoText != null)
            prezzoText.text = $"{impastoSelezionato.prezzo:F2}€";

        Debug.Log($"Impasto selezionato: {impastoSelezionato.nome}");
    }


    public Impasto GetImpastoSelezionato()
    {
        return impastoSelezionato;
    }
}

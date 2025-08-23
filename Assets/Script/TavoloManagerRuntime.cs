using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TavoloManagerRuntime : MonoBehaviour
{
    [Header("UI")]
    public SalaSelector salaSelector;
    public SalaView salaView;

    public GameObject tavoloPrefab;   // prefab del tavolo
    public RectTransform contenitore; // contenitore dei tavoli nella SalaView
    public Button addTavoloButton;    // bottone “+ Tavolo”

    private void Start()
    {
        if (addTavoloButton != null)
            addTavoloButton.onClick.AddListener(CreaNuovoTavolo);
    }

    public void CreaNuovoTavolo()
    {
        if (salaSelector.salaCorrente == null) return;

        Sala sala = salaSelector.salaCorrente;

        // id progressivo
        int nuovoId = (sala.tavoli != null && sala.tavoli.Length > 0) 
            ? sala.tavoli.Max(t => t.id) + 1 
            : 1;

        // creo nuovo ScriptableObject Tavolo
        var nuovoTavolo = ScriptableObject.CreateInstance<Tavolo>();
        nuovoTavolo.id = nuovoId;
        nuovoTavolo.nominativo = $"Tavolo {nuovoId}";
        nuovoTavolo.numeroPosti = 4;
        nuovoTavolo.disponibile = true;

#if UNITY_EDITOR
        // salva come asset in Resources/Tavoli (così resta persistente)
        string path = $"Assets/Resources/Tavoli/{sala.name}_Tavolo{nuovoId}.asset";
        path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
        UnityEditor.AssetDatabase.CreateAsset(nuovoTavolo, path);
        UnityEditor.EditorUtility.SetDirty(nuovoTavolo);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif

        // aggiorna lista tavoli della sala
        var lista = sala.tavoli != null ? sala.tavoli.ToList() : new System.Collections.Generic.List<Tavolo>();
        lista.Add(nuovoTavolo);
        sala.tavoli = lista.ToArray();

        // aggiorna UI
        salaView.MostraSala(sala);
    }
}

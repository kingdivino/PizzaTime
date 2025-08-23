using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TavoloEditView : MonoBehaviour
{
    [Header("Campi")]
    public TMP_InputField inputNome;
    public TMP_InputField inputPosti;

    [Header("Azioni")]
    public Button salvaButton;
    public Button annullaButton;

    [Header("Navigazione (facoltativo)")]
    public GameObject salaViewPanel;

    private Tavolo tavolo;
    private Action onSaved;
    private Action onCancel;

    public void Apri(Tavolo tavoloDaEditare, Action savedCallback, Action cancelCallback)
    {
        tavolo   = tavoloDaEditare;
        onSaved  = savedCallback;
        onCancel = cancelCallback;

        gameObject.SetActive(true);
        if (salaViewPanel) salaViewPanel.SetActive(false);

        // precompila
        inputNome.text  = string.IsNullOrEmpty(tavolo.nominativo) ? $"Tavolo {tavolo.id}" : tavolo.nominativo;
        inputPosti.text = Mathf.Max(1, tavolo.numeroPosti).ToString();

        salvaButton.onClick.RemoveAllListeners();
        annullaButton.onClick.RemoveAllListeners();

        salvaButton.onClick.AddListener(Salva);
        annullaButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            if (salaViewPanel) salaViewPanel.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    void Salva()
    {
        string nuovoNome = inputNome.text.Trim();
        int    nuoviPosti;
        int.TryParse(inputPosti.text, out nuoviPosti);
        nuoviPosti = Mathf.Max(1, nuoviPosti);

        if (string.IsNullOrEmpty(nuovoNome))
        {
            Debug.LogError("Il nome non può essere vuoto.");
            return;
        }

#if UNITY_EDITOR
        // evita duplicati di asset con lo stesso nominativo (diverso oggetto)
        string[] guids = AssetDatabase.FindAssets("t:Tavolo", new[] { "Assets/Resources/Tavoli" });
        foreach (var g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            var t = AssetDatabase.LoadAssetAtPath<Tavolo>(p);
            if (t != null && t != tavolo && t.nominativo == nuovoNome)
            {
                Debug.LogError($"Esiste già un tavolo asset chiamato '{nuovoNome}'.");
                return;
            }
        }
#endif

        bool renameAsset = (nuovoNome != tavolo.nominativo);

        tavolo.nominativo  = nuovoNome;
        tavolo.numeroPosti = nuoviPosti;

#if UNITY_EDITOR
        // rinomina il file .asset se cambi il nome
        if (renameAsset)
        {
            string assetPath = AssetDatabase.GetAssetPath(tavolo);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.RenameAsset(assetPath, nuovoNome);
            }
        }
        EditorUtility.SetDirty(tavolo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

        onSaved?.Invoke();

        if (salaViewPanel) salaViewPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}

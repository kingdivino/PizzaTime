using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SalaManagerRuntime : MonoBehaviour
{
    [Header("UI (MenuSale)")]
    public Transform menuSaleContainer;   // contenitore dei pulsanti sala
    public GameObject salaButtonTemplate; // prefab di un pulsante sala
    public TMP_InputField inputNomeSala;  // input per nome sala

    [Header("Logica sala")]
    public SalaSelector salaSelector;     // riferimento a SalaSelector

    private int salaCounter = 0;

    public void CreaNuovaSala()
    {
        string desiredName = inputNomeSala != null ? inputNomeSala.text.Trim() : "";

#if UNITY_EDITOR
        if (string.IsNullOrEmpty(desiredName))
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:Sala", new[] { "Assets/Resources/Sale" });
            var names = guids
                .Select(g => System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
                .ToHashSet();

            int i = 0;
            while (true)
            {
                string candidate = $"Sala {i}";
                if (!names.Contains(candidate))
                {
                    desiredName = candidate;
                    break;
                }
                i++;
            }
        }
#else
        if (string.IsNullOrEmpty(desiredName))
            desiredName = $"Sala {salaCounter}";
#endif

        var nuovaSala = ScriptableObject.CreateInstance<Sala>();
        nuovaSala.nome = desiredName;
        nuovaSala.name = desiredName;
        nuovaSala.tavoli = new Tavolo[0];

#if UNITY_EDITOR
        string path = $"Assets/Resources/Sale/{desiredName}.asset";
        path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
        UnityEditor.AssetDatabase.CreateAsset(nuovaSala, path);

        string fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(path);
        nuovaSala.name = fileNameNoExt;
        nuovaSala.nome = fileNameNoExt;

        UnityEditor.EditorUtility.SetDirty(nuovaSala);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log($"[SalaManager] Creata sala: campo nome='{nuovaSala.nome}', assetName='{nuovaSala.name}'");

        var btnGO = Instantiate(salaButtonTemplate, menuSaleContainer);
        btnGO.name = $"Btn_{nuovaSala.name}";

        // 🔹 Se il prefab ha già SalaButton → lo prendo, altrimenti lo aggiungo
        var salaBtn = btnGO.GetComponent<SalaButton>();
        if (salaBtn == null) salaBtn = btnGO.AddComponent<SalaButton>();

        salaBtn.sala = nuovaSala;

        var txt = btnGO.GetComponentInChildren<TextMeshProUGUI>(true);
        if (txt != null) txt.text = nuovaSala.name;

        var salaLocal = nuovaSala;
        var btn = btnGO.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            salaSelector.EntraInSala(salaLocal);
        });

        if (inputNomeSala != null) inputNomeSala.text = "";

        Debug.Log($"Creato bottone per sala '{salaBtn.sala.name}'");
    }
}

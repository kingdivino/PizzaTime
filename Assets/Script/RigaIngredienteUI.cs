using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RigaIngredienteUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nomeTxt;
    public TextMeshProUGUI quantitaTxt;
    public TMP_InputField inputN;
    public Button btnPiuUno;
    public Button btnMenoUno;
    public Button btnPiuN;

    private Ingrediente data; // usa la tua classe ScriptableObject

    public void Bind(Ingrediente ingrediente)
    {
        data = ingrediente;

        nomeTxt.text = data.nome;
        AggiornaUI();

        btnPiuUno.onClick.RemoveAllListeners();
        btnMenoUno.onClick.RemoveAllListeners();
        btnPiuN.onClick.RemoveAllListeners();

        btnPiuUno.onClick.AddListener(() => Modifica(1));
        btnMenoUno.onClick.AddListener(() => Modifica(-1));
        btnPiuN.onClick.AddListener(() =>
        {
            if (int.TryParse(inputN.text, out var n))
                Modifica(n);
        });
    }

    private void Modifica(int delta)
    {
        data.quantita = Mathf.Max(0, data.quantita + delta);
        AggiornaUI();

        // salvataggio nel file asset (solo in Editor)
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(data);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    private void AggiornaUI()
    {
        quantitaTxt.text = data.quantita.ToString();
    }
}

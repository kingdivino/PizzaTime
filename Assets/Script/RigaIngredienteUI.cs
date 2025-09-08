using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class RigaIngredienteUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nomeTxt;
    public TextMeshProUGUI quantitaTxt;
    public TMP_InputField inputN;
    public Button btnPiuUno;
    public Button btnMenoUno;
    public Button btnPiuN;

    private IngredienteDTO data;
    

    public void Bind(IngredienteDTO ingrediente)
    {
        data = ingrediente;

        nomeTxt.text = data.nome;
        AggiornaUI();

        btnPiuUno.onClick.RemoveAllListeners();
        btnMenoUno.onClick.RemoveAllListeners();
        btnPiuN.onClick.RemoveAllListeners();

        btnPiuUno.onClick.AddListener(() => StartCoroutine(ModificaQuantita(1)));
        btnMenoUno.onClick.AddListener(() => StartCoroutine(ModificaQuantita(-1)));
        btnPiuN.onClick.AddListener(() =>
        {
            if (int.TryParse(inputN.text, out var n))
                StartCoroutine(ModificaQuantita(n));
        });
    }

    private IEnumerator ModificaQuantita(int delta)
    {
        int nuovaQuantita = Mathf.Max(0, data.quantita + delta);

        // 🔁 Richiesta PUT al backend
        string url = $"http://localhost:3000/ingredienti/{data.id}";

        string json = $@"{{ ""quantita"": {nuovaQuantita} }}";

        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            data.quantita = nuovaQuantita;
            AggiornaUI();
            Debug.Log($"✅ Ingrediente {data.nome} aggiornato a {data.quantita}");
        }
        else
        {
            Debug.LogError("❌ Errore aggiornamento quantità ingrediente: " + req.error);
        }
    }

    private void AggiornaUI()
    {
        quantitaTxt.text = data.quantita.ToString();
    }
}

[System.Serializable]
public class IngredienteDTO
{
    public int id;
    public string nome;
    public float prezzo;
    public int quantita;
    public string sprite; // 🆕 Nome sprite
}



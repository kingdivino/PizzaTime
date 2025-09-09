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
    public TMP_InputField inputPrezzo;   // 👈 nuovo campo per prezzo

    private IngredienteDTO data;

    public void Bind(IngredienteDTO ingrediente)
    {
        data = ingrediente;

        nomeTxt.text = data.nome;
        AggiornaUI();

        inputPrezzo.text = data.prezzo.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        btnPiuUno.onClick.RemoveAllListeners();
        btnMenoUno.onClick.RemoveAllListeners();
        btnPiuN.onClick.RemoveAllListeners();
        inputPrezzo.onEndEdit.RemoveAllListeners();

        btnPiuUno.onClick.AddListener(() => StartCoroutine(ModificaQuantita(1)));
        btnMenoUno.onClick.AddListener(() => StartCoroutine(ModificaQuantita(-1)));
        btnPiuN.onClick.AddListener(() =>
        {
            if (int.TryParse(inputN.text, out var n))
                StartCoroutine(ModificaQuantita(n));
        });

        // listener prezzo
        inputPrezzo.onEndEdit.AddListener(value =>
        {
            value = value.Replace(',', '.'); // supporto virgola e punto
            if (float.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float nuovoPrezzo))
            {
                StartCoroutine(ModificaPrezzo(nuovoPrezzo));
            }
            else
            {
                inputPrezzo.text = data.prezzo.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            }
        });
    }

    private IEnumerator ModificaQuantita(int delta)
    {
        int nuovaQuantita = Mathf.Max(0, data.quantita + delta);

        string url = $"http://localhost:3000/ingredienti/{data.id}";
        string json = $@"{{ ""quantita"": {nuovaQuantita} }}";

        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

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

    private IEnumerator ModificaPrezzo(float nuovoPrezzo)
    {
        string url = $"http://localhost:3000/ingredienti/{data.id}";
        string json = $@"{{ ""prezzo"": {nuovoPrezzo.ToString(System.Globalization.CultureInfo.InvariantCulture)} }}";

        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            data.prezzo = nuovoPrezzo;
            Debug.Log($"✅ Prezzo ingrediente {data.nome} aggiornato a {nuovoPrezzo:F2}€");
        }
        else
        {
            Debug.LogError("❌ Errore aggiornamento prezzo ingrediente: " + req.error);
            inputPrezzo.text = data.prezzo.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
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
    public string sprite;
}

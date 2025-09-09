using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class RigaProdottoUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nomeTxt;
    public TextMeshProUGUI giacenzaTxt;
    public TMP_InputField inputN;
    public Button btnPiuUno;
    public Button btnMenoUno;
    public Button btnPiuN;
    private ProdottoDB prodotto;
    public TMP_InputField inputPrezzo;   // 👈 nuovo campo

    public void Bind(ProdottoDB p)
    {
        prodotto = p;

        nomeTxt.text = prodotto.nome;
        AggiornaUI();

        inputPrezzo.text = prodotto.prezzo.ToString("F2");

        // listeners quantità
        btnPiuUno.onClick.RemoveAllListeners();
        btnMenoUno.onClick.RemoveAllListeners();
        btnPiuN.onClick.RemoveAllListeners();
        inputPrezzo.onEndEdit.RemoveAllListeners();

        btnPiuUno.onClick.AddListener(() => StartCoroutine(Modificagiacenza(1)));
        btnMenoUno.onClick.AddListener(() => StartCoroutine(Modificagiacenza(-1)));
        btnPiuN.onClick.AddListener(() =>
        {
            if (int.TryParse(inputN.text, out int n))
                StartCoroutine(Modificagiacenza(n));
        });

        // listener prezzo (quando premi invio o perdi focus)
        inputPrezzo.onEndEdit.AddListener(value =>
        {
            // sostituisco la virgola con il punto
            value = value.Replace(',', '.');

            if (float.TryParse(
                value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float nuovoPrezzo))
            {
                StartCoroutine(ModificaPrezzo(nuovoPrezzo));
            }
            else
            {
                // rollback se input non valido
                inputPrezzo.text = prodotto.prezzo.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            }
        });




    }

    private IEnumerator ModificaPrezzo(float nuovoPrezzo)
    {
        string url = $"http://localhost:3000/prodotti/{prodotto.id}";
        string json = $@"{{ ""prezzo"": {nuovoPrezzo.ToString(System.Globalization.CultureInfo.InvariantCulture)} }}";

        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            prodotto.prezzo = nuovoPrezzo;
            Debug.Log($"✅ Prezzo di {prodotto.nome} aggiornato a {nuovoPrezzo:F2}€");
        }
        else
        {
            Debug.LogError("❌ Errore aggiornamento prezzo: " + req.error);
            inputPrezzo.text = prodotto.prezzo.ToString("F2"); // rollback
        }
    }




    private IEnumerator Modificagiacenza(int delta)
    {
        int nuovagiacenza = Mathf.Max(0, prodotto.giacenza + delta);

        string url = $"http://localhost:3000/prodotti/{prodotto.id}";
        string json = $@"{{ ""giacenza"": {nuovagiacenza} }}";

        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));


        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            prodotto.giacenza = nuovagiacenza;
            AggiornaUI();
            Debug.Log($"✅ Prodotto {prodotto.nome} aggiornato a {prodotto.giacenza}");
        }
        else
        {
            Debug.LogError("❌ Errore aggiornamento quantità prodotto: " + req.error);
        }
    }

    private void AggiornaUI()
    {
        giacenzaTxt.text = prodotto.giacenza.ToString();
    }
}

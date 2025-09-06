using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 🔹 resta vivo nei cambi scena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ConsumaIngrediente(Ingrediente ingrediente)
    {
        StartCoroutine(SalvaConsumoIngrediente(ingrediente));
    }

    private IEnumerator SalvaConsumoIngrediente(Ingrediente ingrediente, System.Action onSuccess = null)
    {
        // URL coerente con gli altri endpoint
        string urlPost = "http://localhost:3000/ingredienti"; // stesso pattern degli altri salvataggi DB

        ConsumoIngredienteDTO dto = new ConsumoIngredienteDTO
        {
            id = ingrediente.id,
            quantita = 1
        };

        string json = JsonUtility.ToJson(dto, true);
        Debug.Log("📤 Consumo ingrediente JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(urlPost, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("❌ Errore consumo ingrediente: " + request.error +
                           "\nRisposta: " + request.downloadHandler.text);
        else
        {
            Debug.Log($"✅ Consumata 1 unità di {ingrediente.nome}");
            onSuccess?.Invoke();
        }
    }

}

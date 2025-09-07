using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public void ConsumaIngredientiEOrdina(Pizza pizza, System.Action onSuccess, System.Action<List<string>> onIngredientiMancanti)
    {
        StartCoroutine(ConsumaIngredientiCoroutine(pizza, onSuccess, onIngredientiMancanti));
    }

    private IEnumerator ConsumaIngredientiCoroutine(Pizza pizza, System.Action onSuccess, System.Action<List<string>> onIngredientiMancanti)
    {
        List<string> ingredientiMancanti = new List<string>();

        foreach (var ingrediente in pizza.ingredienti)
        {
            bool completato = false;
            bool esito = false;

            yield return StartCoroutine(SalvaConsumoIngrediente(
                ingrediente,
                () => { esito = true; completato = true; },
                () => { esito = false; completato = true; }
            ));

            yield return new WaitUntil(() => completato);

            if (!esito)
                ingredientiMancanti.Add(ingrediente.nome);
        }

        if (ingredientiMancanti.Count > 0)
        {
            onIngredientiMancanti?.Invoke(ingredientiMancanti);
            yield break;
        }

        onSuccess?.Invoke();
    }

    private IEnumerator SalvaConsumoIngrediente(Ingrediente ingrediente, System.Action onSuccess, System.Action onFail)
    {
        string urlPost = "http://localhost:3000/ingredienti"; // come gli altri endpoint

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
        {
            Debug.Log($"❌ Errore consumo ingrediente {ingrediente.nome}: {request.error}\nRisposta: {request.downloadHandler.text}");
            onFail?.Invoke();
        }
        else
        {
            // Controlla se il backend ha risposto con un errore tipo "giacenza insufficiente"
            if (request.downloadHandler.text.Contains("insufficiente"))
            {
                Debug.LogWarning($"⚠️ Ingredienti insufficienti per {ingrediente.nome}");
                onFail?.Invoke();
            }
            else
            {
                Debug.Log($"✅ Consumata 1 unità di {ingrediente.nome}");
                onSuccess?.Invoke();
            }
        }
    }
}

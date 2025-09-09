using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class IngredienteManager : MonoBehaviour
{
    [Header("Riferimento al Menu SO")]
    public Menu menu;   // 👈 il tuo ScriptableObject Menu che contiene gli ingredienti

    private string apiUrl = "http://localhost:3000/ingredienti";

    void Start()
    {
        StartCoroutine(CaricaIngredientiDalDB());
    }

    private IEnumerator CaricaIngredientiDalDB()
    {
        UnityWebRequest req = UnityWebRequest.Get(apiUrl);
        yield return req.SendWebRequest();
        Debug.Log("JSON ricevuto ingredienti: " + req.downloadHandler.text);

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore caricamento ingredienti: " + req.error);
            yield break;
        }

        IngredienteDTO[] ingredientiDB = JsonHelper.FromJson<IngredienteDTO>(req.downloadHandler.text);

        foreach (var ingrDB in ingredientiDB)
        {
            var so = System.Array.Find(menu.ingredienti, i => i.id == ingrDB.id);
            if (so != null)
            {
                so.prezzo = ingrDB.prezzo;
                so.quantita = ingrDB.quantita;
                Debug.Log($"🔄 Aggiornato {so.nome}: prezzo={so.prezzo}, qta={so.quantita}");
            }
        }
    }
}



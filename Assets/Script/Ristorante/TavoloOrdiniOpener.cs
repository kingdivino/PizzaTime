using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class TavoloOrdiniOpener : MonoBehaviour
{
    public Button apriButton;
    [SerializeField] SalaSelector salaSelector; // assegna in Inspector
    public string scenaOrdini = "OrdiniScene";
    private Tavolo tavolo;

    public void Bind(Tavolo t)
    {
        tavolo = t;
        if (!apriButton) return;

        apriButton.onClick.RemoveAllListeners();
        apriButton.onClick.AddListener(() =>
        {
            if (tavolo == null) return;

            if (salaSelector != null && salaSelector.salaCorrente != null)
            {
                SalaCorrenteRegistry.salaIdAttiva = salaSelector.salaCorrente.id;
            }

            // ✅ Solo se lo stato NON è già OrdineInviato
            if (tavolo.stato != StatoTavolo.OrdineInviato)
            {
                tavolo.stato = StatoTavolo.Aperto;
                StartCoroutine(AggiornaStatoNelDB(tavolo.id, "Aperto", tavolo.postiOccupati));
            }

            TavoloCorrenteRegistry.tavoloAttivo = ScriptableObject.Instantiate(tavolo);
            SceneManager.LoadScene(scenaOrdini);
        });
    }



    private IEnumerator AggiornaStatoNelDB(int tavoloId, string nuovoStato, int postiOccupati)
    {
        string url = $"http://localhost:3000/tavoli/{tavoloId}/apri";

        string json = $@"
    {{
        ""stato"": ""{nuovoStato}"",
        ""posti_occupati"": {postiOccupati}
    }}";

        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.method = "PUT";
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer();

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("❌ Errore apertura tavolo: " + req.error);
        else
            Debug.Log("✅ Tavolo marcato come Aperto nel DB");
    }

}


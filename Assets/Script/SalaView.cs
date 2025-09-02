using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SalaView : MonoBehaviour
{
    public RectTransform contenitoreTavoli;
    public GameObject tavoloPrefab;
    public TavoloDettaglioView dettaglioUI;
    public TavoloFormUI tavoloFormUI;
    public TavoloPrenotazioneView prenotazioneUI;

    public Button addTavoloButton;
    public TavoloManagerRuntime tavoloManager; // 🔹 aggiunto per delegare al manager

    public void MostraSala(Sala sala)
{
    // 🔸 Proteggi il bottone +Tavolo durante la pulizia
    foreach (Transform c in contenitoreTavoli)
    {
        if (c.gameObject != addTavoloButton.gameObject)
            Destroy(c.gameObject);
    }

    if (contenitoreTavoli != null)
        contenitoreTavoli.gameObject.SetActive(true);

    if (sala == null || sala.tavoli == null) return;

    var tavoliOrdinati = sala.tavoli
        .OrderBy(t =>
        {
            if (t == null || string.IsNullOrEmpty(t.nominativo)) return int.MaxValue;
            var parts = t.nominativo.Split(' ');
            int num;
            return (parts.Length > 1 && int.TryParse(parts[1], out num)) ? num : int.MaxValue;
        })
        .ToList();

    foreach (var t in tavoliOrdinati)
    {
        var go = Instantiate(tavoloPrefab, contenitoreTavoli);
        var view = go.GetComponent<TavoloView>();
        view.Bind(t, dettaglioUI, prenotazioneUI, tavoloFormUI, tavoloManager);
    }
}

    // 🔹 questo viene richiamato se SalaView vuole comunque aprire il form da sé
    // public void ApriFormCreazioneTavolo(Sala sala)
    // {
    //     if (tavoloFormUI == null || tavoloManager == null)
    //     {
    //         Debug.LogError("⚠️ tavoloFormUI o tavoloManager non assegnati!");
    //         return;
    //     }

    //     tavoloFormUI.ApriPerCreazione(
    //         (nome, posti) =>
    //         {
    //             tavoloManager.CreaTavoloNelDB(sala.id, nome, posti);
    //         },
    //         () =>
    //         {
    //             Debug.Log("Creazione tavolo annullata");
    //             contenitoreTavoli.gameObject.SetActive(true);
    //         }
    //     );

    //     contenitoreTavoli.gameObject.SetActive(false);
    // }
}

using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SalaView : MonoBehaviour
{
    [Header("UI")]
    public RectTransform contenitoreTavoli;
    public GameObject tavoloPrefab;
    public Button addTavoloButton;

    [Header("Componenti")]
    public TavoloDettaglioView dettaglioUI;
    public TavoloFormUI tavoloFormUI;
    public TavoloPrenotazioneView prenotazioneUI;
    public TavoloManagerRuntime tavoloManager;

    public void MostraSala(Sala sala)
    {
        // 🔸 Pulisci il contenitore, lasciando il bottone +Tavolo
        foreach (Transform c in contenitoreTavoli)
        {
            if (c.gameObject != addTavoloButton.gameObject)
                Destroy(c.gameObject);
        }

        if (contenitoreTavoli != null)
            contenitoreTavoli.gameObject.SetActive(true);

        if (sala == null || sala.tavoli == null) return;

        // 🔹 Ordina i tavoli per numero nel nome (es. Tavolo 1, Tavolo 2, ...)
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
}

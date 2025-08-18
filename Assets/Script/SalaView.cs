using UnityEngine;

public class SalaView : MonoBehaviour
{
    public RectTransform contenitoreTavoli;
    public GameObject tavoloPrefab;
    public TavoloDettaglioView dettaglioUI; // ⬅️ collega qui il prefab istanziato in Canvas

    public void MostraSala(Sala sala)
    {
        foreach (Transform c in contenitoreTavoli)
            Destroy(c.gameObject);

        if (sala == null || sala.tavoli == null) return;

        foreach (var t in sala.tavoli)
        {
            var go = Instantiate(tavoloPrefab, contenitoreTavoli);
            var view = go.GetComponent<TavoloView>();
            view.Bind(t, dettaglioUI);
        }
    }
}

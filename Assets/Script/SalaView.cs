using UnityEngine;

public class SalaView : MonoBehaviour
{
    public RectTransform contenitoreTavoli; // metti QUI lo stesso SalaViewPanel
    public GameObject tavoloPrefab;         // il prefab TavoloView

    public void MostraSala(Sala sala)
    {
        // pulisci
        foreach (Transform c in contenitoreTavoli) Destroy(c.gameObject);
        if (sala == null || sala.tavoli == null) return;

        // istanzia
        foreach (var t in sala.tavoli)
        {
            var go = Instantiate(tavoloPrefab, contenitoreTavoli);
            go.GetComponent<TavoloView>().Bind(t);
        }
    }
}

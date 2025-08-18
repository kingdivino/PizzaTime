using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TavoloView : MonoBehaviour
{
    public TextMeshProUGUI numeroTxt;
    public TextMeshProUGUI statoTxt;
    public Button apriDettaglioBtn;

    Tavolo data;

    public void Bind(Tavolo tavolo)
    {
        data = tavolo;
        numeroTxt.text = $"Tavolo {data.id}";
        statoTxt.text = data.disponibile
            ? $"Libero • Posti: {data.numeroPosti}"
            : $"Occupato • {data.postiOccupati}/{data.numeroPosti}";

        if (apriDettaglioBtn)
        {
            apriDettaglioBtn.onClick.RemoveAllListeners();
            apriDettaglioBtn.onClick.AddListener(() => Debug.Log($"Dettaglio tavolo {data.id}"));
        }
    }
}

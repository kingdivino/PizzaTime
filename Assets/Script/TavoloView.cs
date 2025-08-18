using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TavoloView : MonoBehaviour
{
    public TextMeshProUGUI numeroTxt;
    public TextMeshProUGUI statoTxt;

    Tavolo data;
    TavoloDettaglioView dettaglioUI;

    public void Bind(Tavolo tavolo, TavoloDettaglioView dettaglio)
    {
        data = tavolo;
        dettaglioUI = dettaglio;

        numeroTxt.text = $"Tavolo {data.id}";
        statoTxt.text = data.disponibile 
            ? $"Libero ({data.numeroPosti} posti)"
            : $"Occupato {data.postiOccupati}/{data.numeroPosti}";

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => dettaglioUI.MostraDettaglio(data));
        }
    }
}

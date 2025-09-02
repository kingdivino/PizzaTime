using UnityEngine;
using UnityEngine.UI;

public class ScrollStartTop : MonoBehaviour
{
    public ScrollRect scroll;

    void OnEnable()
    {
        // forza calcolo layout e porta in cima
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 1f;   // 1 = top, 0 = bottom
        Canvas.ForceUpdateCanvases();
    }
}

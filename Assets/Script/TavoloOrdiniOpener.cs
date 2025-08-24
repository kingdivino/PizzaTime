using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TavoloOrdiniOpener : MonoBehaviour
{
    [Header("UI")]
    public Button apriButton;          // pulsante “Apri”
    [Header("Config")]
    public string scenaOrdini = "OrdiniScene"; // nome scena ordini

    private Tavolo tavolo;

    public void Bind(Tavolo t)
    {
        tavolo = t;
        if (apriButton == null) return;

        apriButton.onClick.RemoveAllListeners();
        apriButton.onClick.AddListener(() =>
        {
            if (tavolo == null) return;
            TavoloCorrenteRegistry.tavoloAttivo = ScriptableObject.Instantiate(tavolo);  // passa il tavolo
            SceneManager.LoadScene(scenaOrdini);           // vai alla scena ordini
        });
    }
}

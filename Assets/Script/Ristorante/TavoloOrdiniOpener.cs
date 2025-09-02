using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        // 🔹 prendi l'id sala dalla sala corrente
        if (salaSelector != null && salaSelector.salaCorrente != null)
        {
            SalaCorrenteRegistry.salaIdAttiva = salaSelector.salaCorrente.id;
        }

        TavoloCorrenteRegistry.tavoloAttivo = ScriptableObject.Instantiate(tavolo);
        SceneManager.LoadScene(scenaOrdini);
    });
}
}


using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class OrderSceneController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI txtTavolo;
    public Button btnChiudi;        // torna alla scena Sala (o chiudi conto)
    public Button btnInviaOrdine;   // invia e resta in scena, o torna: scegli tu

    public Transform contenitorePizzeOrdinate;
    public GameObject rigaPizza;

    private Tavolo tavolo;

    void Start()
    {
        tavolo = TavoloCorrenteRegistry.tavoloAttivo;
        if (tavolo == null)
        {
            Debug.LogWarning("Nessun tavolo attivo. Torno alle sale.");
            SceneManager.LoadScene("SaleScene"); // nome della scena sale
            return;
        }

        if (txtTavolo) txtTavolo.text = tavolo.nominativo; // es. "Tavolo 12"

        if (btnChiudi)
            btnChiudi.onClick.AddListener(() =>
            {
                // se vuoi solo tornare alla sala:
                SceneManager.LoadScene("SaleScene");
            });

        if (btnInviaOrdine)
            btnInviaOrdine.onClick.AddListener(() =>
            {
                // TODO: salva ordine, stampa comanda, ecc.
                Debug.Log($"Ordine inviato per {tavolo.nominativo}");

                // scelta A: resti in scena (il tavolo rimane aperto per aggiunte)
                // scelta B: torni alla sala automaticamente:
                // SceneManager.LoadScene("SaleScene");
            });

        if(tavolo.ListaPizzeOrdinate.Count != 0)
        {
            foreach(Pizza p in tavolo.ListaPizzeOrdinate)
            {
                GameObject newriga = Instantiate(rigaPizza, contenitorePizzeOrdinate);
                ComponentiReference comp = newriga.GetComponent<ComponentiReference>();
                comp.nome.text = $"Pizza di {p.proprietario}";
                comp.prezzo.text = $"{p.prezzoTotale:F2}€";
                comp.ingredienti.text = "Impasto:" + p.impasto + "\nIngredienti: "+ p.ingredienti.ToCommaSeparatedString();
            }
        }
    }
}

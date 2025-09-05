using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;


public class SalaManagerRuntime : MonoBehaviour
{
    [Header("UI (MenuSale)")]
    public Transform menuSaleContainer;
    public GameObject salaButtonTemplate;
    public GameObject menuSalePanel;
    public TMP_InputField inputNomeSala;

    [Header("Logica sala")]
    public SalaSelector salaSelector;
    public GameObject panelAggiuntaSala;
    public Button btnConferma;
    public Button btnAnnulla;
    public Button btnApriPanelAggiungi; // il bottone “Aggiungi Sala” principale
    public Button indietro;

    private string apiUrl = "http://localhost:3000/sale"; // API per creare sala


    public void CreaNuovaSala()
    {
        string nomeSala = inputNomeSala != null ? inputNomeSala.text.Trim() : "";
        if (string.IsNullOrEmpty(nomeSala))
        {
            Debug.LogWarning("Inserire un nome per la sala.");
            return;
        }

        StartCoroutine(CreaSalaNelDatabase(nomeSala));
    }

    IEnumerator CreaSalaNelDatabase(string nomeSala)
    {
        var salaJson = JsonUtility.ToJson(new SalaDTO { nome = nomeSala });
        var request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(salaJson);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Errore creazione sala: " + request.error);
        }
        else
        {
            string json = request.downloadHandler.text;

            try
            {
                SalaDTO nuovaSala = JsonUtility.FromJson<SalaDTO>(json);
                Debug.Log($"Sala creata nel DB con id={nuovaSala.id}, nome={nuovaSala.nome}");

                // 🔹 CREA ScriptableObject Sala
                var salaSO = ScriptableObject.CreateInstance<Sala>();
                salaSO.id = nuovaSala.id;
                salaSO.nome = nuovaSala.nome;
                salaSO.name = nuovaSala.nome;
                salaSO.tavoli = new Tavolo[0];

                // 🔹 CREA pulsante UI
                var btnGO = Instantiate(salaButtonTemplate, menuSaleContainer);
                btnGO.name = $"Btn_Sala_{nuovaSala.id}";

                // 🔹 Assegna testo
                var txt = btnGO.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null)
                    txt.text = nuovaSala.nome;

                // 🔹 Assegna Sala allo script SalaButton
                var salaBtn = btnGO.GetComponent<SalaButton>();
                if (salaBtn == null) salaBtn = btnGO.AddComponent<SalaButton>();
                salaBtn.sala = salaSO;

                // 🔹 Listener pulsante → entra nella sala
                btnGO.GetComponent<Button>().onClick.AddListener(() =>
                {
                    salaSelector.EntraInSalaDB(nuovaSala.id);
                });

                if (inputNomeSala != null)
                    inputNomeSala.text = "";
            }
            catch (System.Exception e)
            {
                Debug.LogError("Errore nel parsing JSON della risposta: " + e.Message);
                Debug.LogError("JSON ricevuto: " + json);
            }
        }
    }

void Start()
{
    indietro.gameObject.SetActive(true); // ✅ mostralo quando entri

    indietro.onClick.AddListener(() =>
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    });
    panelAggiuntaSala.SetActive(false);
    btnApriPanelAggiungi.onClick.AddListener(() =>
    {
        menuSalePanel.SetActive(false);
        indietro.gameObject.SetActive(false);
        panelAggiuntaSala.SetActive(true);
        inputNomeSala.text = "";
    });

    btnAnnulla.onClick.AddListener(() =>
    {
        menuSalePanel.SetActive(true);
        panelAggiuntaSala.SetActive(false);
    });

    btnConferma.onClick.AddListener(() =>
    {
        menuSalePanel.SetActive(true);
        CreaNuovaSala();
        panelAggiuntaSala.SetActive(false);
    });
}


}

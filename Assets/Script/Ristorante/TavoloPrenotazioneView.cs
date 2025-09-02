using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.Networking;

public class TavoloPrenotazioneView : MonoBehaviour
{
    [Header("Campi")]
    public TMP_InputField inputCognome;
    public TMP_InputField inputPersone;
    public TMP_InputField inputOre;
    public TMP_InputField inputMin;
    public TavoloOrdiniOpener ordiniOpener; // assegna in Inspector il pulsante "Apri" del pannello prenotazione


    [Header("UI Extra")]
    public TextMeshProUGUI maxPostiTxt;
    public TextMeshProUGUI erroreTxt;

    [Header("Azioni")]
    public Button confermaButton;
    public Button annullaButton;

    [Header("Navigazione")]
    public GameObject salaViewPanel;


    private Action<int, string, string> onConferma;
    private Action onAnnulla;
    private Tavolo tavoloCorrente;

    private string apiUrl = "http://localhost:3000/tavoli"; // stesso endpoint del manager

    public void Apri(Tavolo tavolo, Action<int, string, string> confermaCallback, Action annullaCallback)
    {
        if (ordiniOpener != null) ordiniOpener.Bind(tavolo); // il pulsante "Apri" porterà a OrdiniScene con il tavolo

        gameObject.SetActive(true);
        if (salaViewPanel) salaViewPanel.SetActive(false);

        tavoloCorrente = tavolo;
        onConferma = confermaCallback;
        onAnnulla = annullaCallback;

        // reset UI
        inputCognome.text = "";
        inputPersone.text = "";
        inputOre.text = "";
        inputMin.text = "";
        if (maxPostiTxt) maxPostiTxt.text = $"/{tavoloCorrente.numeroPosti}";
        if (erroreTxt) { erroreTxt.text = ""; erroreTxt.gameObject.SetActive(false); }

        // listeners
        confermaButton.onClick.RemoveAllListeners();
        annullaButton.onClick.RemoveAllListeners();
        inputCognome.onValueChanged.RemoveAllListeners();
        inputPersone.onValueChanged.RemoveAllListeners();
        inputOre.onValueChanged.RemoveAllListeners();
        inputMin.onValueChanged.RemoveAllListeners();

        inputCognome.onValueChanged.AddListener(_ => Validazione());
        inputPersone.onValueChanged.AddListener(_ => Validazione());
        inputOre.onValueChanged.AddListener(_ => Validazione());
        inputMin.onValueChanged.AddListener(_ => Validazione());
        confermaButton.onClick.AddListener(() =>
        {
            if (!Validazione()) return;

            int persone = int.Parse(inputPersone.text);
            string cognome = inputCognome.text.Trim();
            string orario = FormattaOrario();

            // aggiorna oggetto in memoria
            tavoloCorrente.disponibile = false;
            tavoloCorrente.postiOccupati = persone;
            tavoloCorrente.cognomePrenotazione = cognome;
            tavoloCorrente.orarioPrenotazione = orario;

            // 🔹 Salva nel DB
            StartCoroutine(SalvaPrenotazioneNelDB(tavoloCorrente.id, persone, cognome, orario));

            onConferma?.Invoke(persone, cognome, orario);

            if (salaViewPanel) salaViewPanel.SetActive(true);
            gameObject.SetActive(false);
        });

        annullaButton.onClick.AddListener(() =>
        {
            onAnnulla?.Invoke();
            if (salaViewPanel) salaViewPanel.SetActive(true);
            gameObject.SetActive(false);
        });

        Validazione();
    }

    private IEnumerator SalvaPrenotazioneNelDB(int tavoloId, int persone, string cognome, string orario)
    {
    string url = $"{apiUrl}/{tavoloId}/prenota"; // nuova rotta

        string json = $@"
        {{
            ""disponibile"": false,
            ""posti_occupati"": {persone},
            ""cognome_prenotazione"": ""{cognome}"",
            ""orario_prenotazione"": ""{orario}""
        }}";

        UnityWebRequest request = UnityWebRequest.Put(url, json);
        request.method = "PUT";
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Errore salvataggio prenotazione: " + request.error);
        }
        else
        {
            Debug.Log("✅ Prenotazione salvata nel DB");
        }
    }

    bool Validazione()
    {   
        Debug.Log("VALIDAZIONE CHIAMATA");
        int persone;
        bool personeOk = int.TryParse(inputPersone.text, out persone)
                         && persone >= 1
                         && persone <= Mathf.Max(1, tavoloCorrente.numeroPosti);

        bool cognomeOk = !string.IsNullOrWhiteSpace(inputCognome.text);

        int hh, mm;
        bool oreOk = int.TryParse(inputOre.text, out hh) && hh >= 0 && hh <= 23;
        bool minOk = int.TryParse(inputMin.text, out mm) && mm >= 0 && mm <= 59;
        bool orarioOk = oreOk && minOk;

        Debug.Log($"VALIDAZIONE → PersoneOk={personeOk}, CognomeOk={cognomeOk}, OrarioOk={orarioOk}");

        if (erroreTxt)
        {
            if (!personeOk)
            {
                erroreTxt.gameObject.SetActive(true);
                erroreTxt.text = $"Persone: 1–{tavoloCorrente.numeroPosti}";
            }
            else if (!cognomeOk)
            {
                erroreTxt.gameObject.SetActive(true);
                erroreTxt.text = "Inserisci il cognome";
            }
            else if (!orarioOk)
            {
                erroreTxt.gameObject.SetActive(true);
                erroreTxt.text = "Orario non valido (0–23 / 0–59)";
            }
            else
            {
                erroreTxt.gameObject.SetActive(false);
                erroreTxt.text = "";
            }
        }

        bool ok = personeOk && cognomeOk && orarioOk;
        if (confermaButton) confermaButton.interactable = ok;
        return ok;
    }


    string FormattaOrario()
    {
        int hh = 0, mm = 0;
        int.TryParse(inputOre.text, out hh);
        int.TryParse(inputMin.text, out mm);
        hh = Mathf.Clamp(hh, 0, 23);
        mm = Mathf.Clamp(mm, 0, 59);
        return $"{hh:00}:{mm:00}";
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TavoloPrenotazioneView : MonoBehaviour
{
    [Header("Campi")]
    public TMP_InputField inputCognome;     // Cognome/Text Area
    public TMP_InputField inputPersone;     // numeroPersone/Text Area
    public TMP_InputField inputOre;         // ore/Text Area
    public TMP_InputField inputMin;         // min/Text Area

    [Header("UI Extra (opzionali)")]
    public TextMeshProUGUI maxPostiTxt;     // "Max: X"
    public TextMeshProUGUI erroreTxt;       // messaggi errore

    [Header("Azioni")]
    public Button confermaButton;           // Conferma
    public Button annullaButton;            // Annulla

    [Header("Navigazione")]
    public GameObject salaViewPanel;        // pannello lista tavoli da riattivare alla chiusura

    private Action<int, string, string> onConferma;
    private Action onAnnulla;
    private Tavolo tavoloCorrente;

    public void Apri(Tavolo tavolo, Action<int, string, string> confermaCallback, Action annullaCallback)
    {
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

            // aggiorna il tavolo
            tavoloCorrente.disponibile = false;
            tavoloCorrente.postiOccupati = persone;

            // se hai aggiunto i campi in Tavolo.cs li valorizziamo:
            tavoloCorrente.cognomePrenotazione = cognome;
            tavoloCorrente.orarioPrenotazione = orario;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(tavoloCorrente);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
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

        // stato iniziale del bottone
        Validazione();
    }

    bool Validazione()
    {
        // persone
        int persone;
        bool personeOk = int.TryParse(inputPersone.text, out persone)
                         && persone >= 1
                         && persone <= Mathf.Max(1, tavoloCorrente.numeroPosti);

        // cognome
        bool cognomeOk = !string.IsNullOrWhiteSpace(inputCognome.text);

        // orario
        int hh, mm;
        bool oreOk = int.TryParse(inputOre.text, out hh) && hh >= 0 && hh <= 23;
        bool minOk = int.TryParse(inputMin.text, out mm) && mm >= 0 && mm <= 59;
        bool orarioOk = oreOk && minOk;

        // messaggi
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

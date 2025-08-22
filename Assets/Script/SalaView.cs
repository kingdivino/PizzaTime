using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class SalaView : MonoBehaviour
{
    public RectTransform contenitoreTavoli;
    public GameObject tavoloPrefab;
    public GameObject tavoloAddButtonPrefab;
    public TavoloDettaglioView dettaglioUI;
    public TavoloFormUI tavoloFormUI;       // 👈 form creazione/modifica
    public TavoloPrenotazioneView prenotazioneUI; // 👈 pannello prenotazione

    public void MostraSala(Sala sala)
    {
        if (contenitoreTavoli != null)
            contenitoreTavoli.gameObject.SetActive(true);

        if (tavoloAddButtonPrefab != null)
            tavoloAddButtonPrefab.SetActive(true);

        // pulizia contenitore
        foreach (Transform c in contenitoreTavoli)
            Destroy(c.gameObject);

        if (sala == null || sala.tavoli == null) return;

        // 🔹 ordina i tavoli in base al numero nel nominativo
        var tavoliOrdinati = sala.tavoli
            .OrderBy(t =>
            {
                if (t == null || string.IsNullOrEmpty(t.nominativo)) return int.MaxValue;
                var parts = t.nominativo.Split(' ');
                int num;
                return (parts.Length > 1 && int.TryParse(parts[1], out num)) ? num : int.MaxValue;
            })
            .ToList();

        // istanzia i tavoli ordinati
        foreach (var t in tavoliOrdinati)
        {
            var go = Instantiate(tavoloPrefab, contenitoreTavoli);
            var view = go.GetComponent<TavoloView>();
            view.Bind(t, dettaglioUI, prenotazioneUI, tavoloFormUI); // 👈 passiamo anche tavoloFormUI
        }

        // aggiungi il bottone “+ Tavolo” in fondo
        var addBtnGO = Instantiate(tavoloAddButtonPrefab, contenitoreTavoli);
        var btn = addBtnGO.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                ApriFormCreazioneTavolo(sala);
            });
        }
    }

    private void ApriFormCreazioneTavolo(Sala sala)
    {
        if (tavoloFormUI == null)
        {
            Debug.LogError("TavoloFormUI non è assegnato nell’Inspector!");
            return;
        }

        tavoloFormUI.ApriPerCreazione(
            (nome, posti) =>
            {
                CreaNuovoTavolo(sala, nome, posti);
            },
            () =>
            {
                Debug.Log("Creazione tavolo annullata");
                contenitoreTavoli.gameObject.SetActive(true); // riattiva la lista tavoli
            }
        );

        contenitoreTavoli.gameObject.SetActive(false); // nascondi i tavoli durante la creazione
    }

    private void CreaNuovoTavolo(Sala sala, string nome, int posti)
    {
        if (sala == null) return;

        // Se il nome non viene inserito, blocco subito
        if (string.IsNullOrWhiteSpace(nome))
        {
            Debug.LogError("Il nome del tavolo non può essere vuoto.");
            return;
        }

        // ⚠️ Controllo duplicati nella sala
        if (sala.tavoli != null && sala.tavoli.Any(t => t.nominativo == nome))
        {
            Debug.LogError($"Esiste già un tavolo chiamato '{nome}' in questa sala!");
            return;
        }

        // id sempre univoco e progressivo
        int nuovoId = (sala.tavoli != null && sala.tavoli.Length > 0)
            ? sala.tavoli.Max(t => t.id) + 1
            : 1;

        var nuovoTavolo = ScriptableObject.CreateInstance<Tavolo>();
        nuovoTavolo.id = nuovoId;
        nuovoTavolo.nominativo = nome;
        nuovoTavolo.numeroPosti = posti;
        nuovoTavolo.disponibile = true;

#if UNITY_EDITOR
        string path = $"Assets/Resources/Tavoli/{nome}.asset";
        path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
        UnityEditor.AssetDatabase.CreateAsset(nuovoTavolo, path);

        UnityEditor.EditorUtility.SetDirty(nuovoTavolo);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif

        // aggiorna lista sala
        var lista = sala.tavoli != null ? sala.tavoli.ToList() : new System.Collections.Generic.List<Tavolo>();
        lista.Add(nuovoTavolo);
        sala.tavoli = lista.ToArray();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(sala);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif

        MostraSala(sala);
        contenitoreTavoli.gameObject.SetActive(true); // torna a mostrare i tavoli
    }
}

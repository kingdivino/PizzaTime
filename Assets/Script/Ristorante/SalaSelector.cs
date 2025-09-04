using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class SalaSelector : MonoBehaviour
{
    [Header("UI")]
    public SalaView salaView;
    public GameObject menuSale;
    public GameObject salaViewPanel;
    public GameObject aggiungi;

    public Sala salaCorrente { get; private set; }

    public void EntraInSala(Sala sala)
    {
        salaCorrente = sala;
        salaView.MostraSala(sala);
        menuSale.SetActive(false);
        aggiungi.SetActive(false);
        salaViewPanel.SetActive(true);
    }

    public void TornaAlMenu()
    {
        salaCorrente = null;
        salaViewPanel.SetActive(false);
        menuSale.SetActive(true);
        aggiungi.SetActive(true);
    }

    public void EntraInSalaDB(int salaId)
    {
        Debug.Log($"Entrato nella sala con ID={salaId} (dal DB)");
        StartCoroutine(CaricaTavoli(salaId));
    }

    private IEnumerator CaricaTavoli(int salaId)
    {
        string url = $"http://localhost:3000/tavoli?salaId={salaId}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Errore caricamento tavoli: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            TavoloDTO[] tavoli = JsonHelper.FromJson<TavoloDTO>(json);

            Sala sala = ScriptableObject.CreateInstance<Sala>();
            sala.id = salaId;
            sala.nome = $"Sala {salaId}";
            sala.tavoli = tavoli.Select(t =>
            {
                var tavolo = ScriptableObject.CreateInstance<Tavolo>();
                tavolo.id = t.id;
                tavolo.nominativo = t.nominativo;
                tavolo.numeroPosti = t.numero_posti;
                tavolo.disponibile = t.disponibile;
                tavolo.postiOccupati = t.posti_occupati;
                tavolo.cognomePrenotazione = t.cognome_prenotazione;
                tavolo.orarioPrenotazione = t.orario_prenotazione;
                    if (System.Enum.TryParse<StatoTavolo>(t.stato, out var parsedStato))
                    tavolo.stato = parsedStato;
                else
                    tavolo.stato = StatoTavolo.Libero;

                return tavolo;
            }).ToArray();

            salaCorrente = sala;
            salaView.MostraSala(sala);
            menuSale.SetActive(false);
            aggiungi.SetActive(false);
            salaViewPanel.SetActive(true);

            // ✅ Inizializzazione manager tavoli DOPO salaCorrente
            if (salaViewPanel != null)
            {
                var tavoloManager = salaViewPanel.GetComponentInChildren<TavoloManagerRuntime>(true);
                if (tavoloManager != null)
                {
                    Debug.Log($"📦 salaCorrente al momento di Init: {salaCorrente.nome}");
                    //tavoloManager.Init();
                    Debug.Log("✅ TavoloManagerRuntime inizializzato manualmente");
                }
            }
        }
    }

    [System.Serializable]
    public class TavoloDTO
    {
        public int id;
        public string nominativo;
        public int numero_posti;
        public bool disponibile;
        public int posti_occupati;
        public string stato;

        public string cognome_prenotazione;     // ✅ aggiunto
        public string orario_prenotazione;      // ✅ aggiunto
    }



}

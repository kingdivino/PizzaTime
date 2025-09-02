using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking; // ⬅️ IMPORTANTE

public class SalaManagerDelete : MonoBehaviour
{
    [Header("UI")]
    public GameObject confermaPanel;
    public TextMeshProUGUI confermaTesto;
    public GameObject salaviewPanel;

    [Header("Riferimenti")]
    public SalaSelector salaSelector;

    private Sala salaAttuale;

    public void RichiediEliminazione()
    {
        salaAttuale = salaSelector.salaCorrente;
        if (salaAttuale == null) return;

        if (confermaTesto != null)
            confermaTesto.text = $"Vuoi veramente eliminare la sala \"{salaAttuale.name}\"?";

        if (confermaPanel != null)
        {
            confermaPanel.SetActive(true);
            salaviewPanel.SetActive(false);
        }
    }

    public void ConfermaEliminazione()
    {
        if (salaAttuale == null)
        {
            Debug.LogWarning("Nessuna sala selezionata per l'eliminazione.");
            return;
        }

        int salaId = salaAttuale.id;

        StartCoroutine(EliminaSalaDalDB(salaId));
    }


    private IEnumerator EliminaSalaDalDB(int salaId)
    {
        string url = $"http://localhost:3000/sale/{salaId}";
        UnityWebRequest request = UnityWebRequest.Delete(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Errore eliminazione sala: " + request.error);
        }
        else
        {
            Debug.Log($"Sala con ID={salaId} eliminata con successo.");

            // 🔸 Rimuove il pulsante sala dalla UI
            var allButtons = FindObjectsOfType<SalaButton>(true);
            foreach (var sb in allButtons)
            {
                if (sb != null && sb.sala != null)
                {
                    Debug.Log($"Controllo bottone {sb.sala.nome} con ID={sb.sala.id}");

                    if (sb.sala.id == salaId)
                    {
                        Destroy(sb.gameObject);
                        Debug.Log($"✅ Bottone sala ID={salaId} eliminato dalla UI");
                        break;
                    }
                }
            }

            salaAttuale = null;

            if (confermaPanel != null)
                confermaPanel.SetActive(false);

            if (salaSelector != null)
                salaSelector.TornaAlMenu();
        }
    }


    public void Annulla()
    {
        salaAttuale = null;
        if (confermaPanel != null)
        {
            confermaPanel.SetActive(false);
            salaviewPanel.SetActive(true);
        }
    }
}

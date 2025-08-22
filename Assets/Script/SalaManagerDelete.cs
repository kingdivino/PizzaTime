using UnityEngine;
using TMPro;

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
#if UNITY_EDITOR
        if (salaAttuale != null)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(salaAttuale);
            if (!string.IsNullOrEmpty(path))
            {
                UnityEditor.AssetDatabase.DeleteAsset(path);
                UnityEditor.AssetDatabase.Refresh();
            }
        }
#endif

        var allButtons = FindObjectsOfType<SalaButton>(true); // 👈 true = include anche disattivati
        Debug.Log(allButtons.Length + " pulsanti sala trovati nella scena.");
        foreach (var sb in allButtons)
        {
            Debug.Log("Ho trovato bottone: " + sb.gameObject.name + " legato a sala: " + (sb.sala != null ? sb.sala.name : "NULL"));

            if (sb.sala == salaAttuale)
            {
                Destroy(sb.gameObject); // elimina il bottone
                break;
            }
        }

        salaAttuale = null;

        if (confermaPanel != null)
            confermaPanel.SetActive(false);

        if (salaSelector != null)
            salaSelector.TornaAlMenu();
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

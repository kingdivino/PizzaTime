using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SalaLoader : MonoBehaviour
{
    [Header("UI")]
    public Transform menuSaleContainer;
    public Button salaButtonTemplate;

    [Header("Riferimenti")]
    public SalaSelector salaSelector;

    void Start()
    {
        CaricaSale();
    }

    private void CaricaSale()
    {
        Sala[] sale = Resources.LoadAll<Sala>("Sale");
        Debug.Log("Sale trovate: " + sale.Length);

        foreach (Sala s in sale)
        {
            var btnObj = Instantiate(salaButtonTemplate, menuSaleContainer);
            btnObj.name = $"Btn_{s.name}";

            // 🔹 Se il prefab ha già SalaButton → lo prendo, altrimenti lo aggiungo
            var salaBtn = btnObj.GetComponent<SalaButton>();
            if (salaBtn == null) salaBtn = btnObj.gameObject.AddComponent<SalaButton>();

            salaBtn.sala = s;

            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
                txt.text = string.IsNullOrEmpty(s.nome) ? s.name : s.nome;

            var salaLocal = s;
            btnObj.onClick.RemoveAllListeners();
            btnObj.onClick.AddListener(() =>
            {
                salaSelector.EntraInSala(salaLocal);
            });

            Debug.Log($"Creato bottone per sala '{salaBtn.sala.name}'");
        }
    }
}

using UnityEngine;

public class SalaSelector : MonoBehaviour
{
    public Sala salaA;
    public Sala salaB;
    public SalaView salaView;

    public GameObject menuSale;       // pannello dei pulsanti
    public GameObject salaViewPanel;  // pannello con i tavoli

    public void VaiSalaA()
    {
        salaView.MostraSala(salaA);
        menuSale.SetActive(false);
        salaViewPanel.SetActive(true);
    }

    public void VaiSalaB()
    {
        salaView.MostraSala(salaB);
        menuSale.SetActive(false);
        salaViewPanel.SetActive(true);
    }

    public void TornaAlMenu()
    {
        salaViewPanel.SetActive(false);
        menuSale.SetActive(true);
    }
}

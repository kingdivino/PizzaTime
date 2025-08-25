using UnityEngine;

public class SalaSelector : MonoBehaviour
{
    public SalaView salaView;
    public GameObject menuSale;
    public GameObject salaViewPanel;
    public GameObject aggiungi;

    public Sala salaCorrente { get; private set; }  // proprietà per la sala attiva

    public void EntraInSala(Sala sala)
    {
        salaCorrente = sala; // 👈 QUI viene impostata la sala attuale
        salaView.MostraSala(sala);

        menuSale.SetActive(false);
        aggiungi.SetActive(false);
        salaViewPanel.SetActive(true);
    }

    public void TornaAlMenu()
    {
        salaCorrente = null; // 👈 reset quando torni al menu
        salaViewPanel.SetActive(false);
        menuSale.SetActive(true);
        aggiungi.SetActive(true);
    }
    public void EntraInSalaDB(int salaId)
{
    Debug.Log($"Entrato nella sala con ID={salaId} (dal DB)");
    // TODO: chiamata API /tavoli?salaId=... e popolamento UI tavoli
}

}


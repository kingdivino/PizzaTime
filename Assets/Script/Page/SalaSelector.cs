using UnityEngine;

public class SalaSelector : MonoBehaviour
{
    public Sala salaA;
    public Sala salaB;
    public SalaView salaView; // riferimento al pannello che disegna i tavoli

    public void VaiSalaA()
    {
        salaView.MostraSala(salaA);
    }

    public void VaiSalaB()
    {
        salaView.MostraSala(salaB);
    }
}

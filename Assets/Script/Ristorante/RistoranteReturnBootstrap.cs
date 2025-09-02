using System.Collections;
using UnityEngine;
public class RistoranteReturnBootstrap : MonoBehaviour
{
    public SalaSelector salaSelector;

    IEnumerator Start()
    {
        yield return null;

        if (SalaCorrenteRegistry.salaIdAttiva > 0)
        {
            salaSelector.EntraInSalaDB(SalaCorrenteRegistry.salaIdAttiva);
            Debug.Log($"➡️ Rientro in sala ID {SalaCorrenteRegistry.salaIdAttiva}");
            SalaCorrenteRegistry.salaIdAttiva = 0; // reset
        }
        else
        {
            Debug.Log("➡️ Nessuna sala attiva da ricaricare");
        }
    }

}

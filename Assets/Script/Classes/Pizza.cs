using UnityEngine;

[CreateAssetMenu(fileName = "Pizza", menuName = "Scriptable Objects/Pizza")]
public class Pizza : ScriptableObject
{
    public Impasto impasto;
    public Ingrediente[] ingredienti;
    public float prezzoTotale;

    public void Init(Impasto impasto, Ingrediente[] ingredienti)
    {
        this.impasto = impasto;
        this.ingredienti = ingredienti;
        CalcolaPrezzo();
    }

    public void CalcolaPrezzo()
    {
        prezzoTotale = 0f;
        if (impasto != null)
            prezzoTotale += impasto.prezzo;

        foreach (var ingr in ingredienti)
        {
            prezzoTotale += ingr.prezzo;
        }
    }

    public float GetPrezzo()
    {
        return prezzoTotale;
    }
}

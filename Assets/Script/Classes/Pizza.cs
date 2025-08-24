using UnityEngine;

[CreateAssetMenu(fileName = "Pizza", menuName = "Scriptable Objects/Pizza")]
public class Pizza : ScriptableObject
{
    public Impasto impasto;
    public Ingrediente[] ingredienti;
    public float prezzoTotale;
    public string proprietario;

    public void Init(Impasto impasto, Ingrediente[] ingredienti, string proprietario)
    {
        this.impasto = impasto;
        this.ingredienti = ingredienti;
        this.proprietario = proprietario;
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

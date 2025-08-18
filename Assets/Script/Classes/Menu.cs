using UnityEngine;

[CreateAssetMenu(fileName = "Menu", menuName = "Scriptable Objects/Menu")]
public class Menu : ScriptableObject
{
    public Ingrediente[] ingredienti;
    public Impasto[] impasti;
    public Prodotto[] prodotti;

    public Menu(Ingrediente[] ingredienti, Impasto[] impasti, Prodotto[] prodotti)
    {
        this.ingredienti = ingredienti;
        this.impasti = impasti;
        this.prodotti = prodotti;
    }
}



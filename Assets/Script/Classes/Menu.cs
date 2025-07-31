using UnityEngine;

[CreateAssetMenu(fileName = "Menu", menuName = "Scriptable Objects/Menu")]
public class Menu : ScriptableObject
{
    public Ingrediente[] ingredienti;

    public Menu(Ingrediente[] ingredienti)
    {
        this.ingredienti = ingredienti;
    }
}



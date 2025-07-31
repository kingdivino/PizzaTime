using UnityEngine;

[CreateAssetMenu(fileName = "Prodotto", menuName = "Scriptable Objects/Prodotto")]
public class Prodotto : ScriptableObject
{
    public int id;
    public string nome;
    public Sprite img;
    public float prezzo;
    public Ingrediente[] ingredienti;

    public Prodotto(int id, string nome, Sprite img, float prezzo, Ingrediente[] ingredienti)
    {
        this.id = id;
        this.nome = nome;
        this.img = img;
        this.prezzo = prezzo;
        this.ingredienti = ingredienti;
    }
}


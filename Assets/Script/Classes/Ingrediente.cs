using UnityEngine;

[CreateAssetMenu(fileName = "Ingrediente", menuName = "Scriptable Objects/Ingrediente")]
public class Ingrediente : ScriptableObject
{
    public int id;
    public string nome;
    public float prezzo;
    public int quantita;

    public Ingrediente(int id, string nome, Sprite img, float prezzo, int quantita)
    {
        this.id = id;
        this.nome = nome;
        this.prezzo = prezzo;
        this.quantita = quantita;
    }


    public void sottraiQuantità(Ingrediente i)
    {
        i.quantita--;
    }
}

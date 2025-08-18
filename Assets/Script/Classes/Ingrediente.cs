using UnityEngine;

[CreateAssetMenu(fileName = "Ingrediente", menuName = "Scriptable Objects/Ingrediente")]
public class Ingrediente : ScriptableObject
{
    public int id;
    public string nome;
    public float prezzo;
    public int quantita;
    public Sprite sprite;
    public int layer;
    public bool singolo;
    public Ingrediente(int id, string nome, Sprite sprite, float prezzo, int quantita, int layer, bool singolo)
    {
        this.id = id;
        this.nome = nome;
        this.prezzo = prezzo;
        this.quantita = quantita;
        this.sprite = sprite;
        this.layer = layer;
        this.singolo = singolo;
    }

}

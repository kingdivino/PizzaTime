using UnityEngine;

[CreateAssetMenu(fileName = "Impasto", menuName = "Scriptable Objects/Impasto")]
public class Impasto : ScriptableObject
{
    public int id;
    public string nome;
    public float prezzo;
    public Sprite sprite;

    public Impasto(int id, string nome, float prezzo, Sprite sprite)
    {
        this.id = id;
        this.nome = nome;
        this.prezzo = prezzo;
        this.sprite = sprite;
    }
}

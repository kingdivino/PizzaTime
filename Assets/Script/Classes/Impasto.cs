using UnityEngine;

[CreateAssetMenu(fileName = "Impasto", menuName = "Scriptable Objects/Impasto")]
public class Impasto : ScriptableObject
{
    public int id;
    public string nome;
    public float prezzo;

    public Impasto(int id, string nome, float prezzo)
    {
        this.id = id;
        this.nome = nome;
        this.prezzo = prezzo;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "Sala", menuName = "Scriptable Objects/Sala")]
public class Sala : ScriptableObject
{
    public int id;
    public string nome;      // ⬅️ aggiunto

    public Tavolo[] tavoli;

}

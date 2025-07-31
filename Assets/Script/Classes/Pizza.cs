using UnityEngine;

[CreateAssetMenu(fileName = "Pizza", menuName = "Scriptable Objects/Pizza")]
public class Pizza : ScriptableObject
{
    public Impasto impasto;
    public Ingrediente[] ingredienti;
    public float prezzoTotale;
}

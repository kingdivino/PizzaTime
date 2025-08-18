using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Tavolo", menuName = "Scriptable Objects/Tavolo")]
public class Tavolo : ScriptableObject
{
    public int id;
    public string nominativo;
    public int numeroPosti;
    public bool disponibile;
    public int postiOccupati;
    public TimeSpan tempoOccupato;
    public DateTime dataOccupazione;
    public Time timestampOrdinazione;
    public Prodotto[] prodottiOrdinati;
    public Pizza[] pizzeOrdinate;
    public Sprite Table;
    
    public Tavolo(int id, string nominativo, int numeroPosti, bool disponibile, int postiOccupati, TimeSpan tempoOccupato, DateTime dataOccupazione, Time timestampOrdinazione, Prodotto[] prodottiOrdinati, Pizza[] pizzeOrdinate)
    {
        this.id = id;
        this.nominativo = nominativo;
        this.numeroPosti = numeroPosti;
        this.disponibile = disponibile;
        this.postiOccupati = postiOccupati;
        this.tempoOccupato = tempoOccupato;
        this.dataOccupazione = dataOccupazione;
        this.timestampOrdinazione = timestampOrdinazione;
        this.prodottiOrdinati = prodottiOrdinati;
        this.pizzeOrdinate = pizzeOrdinate;
    }

}

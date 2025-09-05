using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Tavolo", menuName = "Scriptable Objects/Tavolo")]
public class Tavolo : ScriptableObject
{
    public int id;
    public int salaId;
    public string nominativo;
    public int numeroPosti;
    public bool disponibile;
    public int postiOccupati;
    public TimeSpan tempoOccupato;
    public DateTime dataOccupazione;
    public Time timestampOrdinazione;
    public Prodotto[] prodottiOrdinati;
    public Pizza[] pizzeOrdinate;
    public List<Pizza> ListaPizzeOrdinate = new List<Pizza>();
    public Sprite Table;
    public string cognomePrenotazione;   // es. "Rossi"
    public string orarioPrenotazione;    // es. "20:30"
    public StatoTavolo stato;

    public Tavolo(int id, string nominativo, int numeroPosti, bool disponibile, int postiOccupati, TimeSpan tempoOccupato,
        DateTime dataOccupazione, Time timestampOrdinazione, Prodotto[] prodottiOrdinati, Pizza[] pizzeOrdinate,
        Sprite table, string cognomePrenotazione, string orarioPrenotazione)
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
        this.cognomePrenotazione = cognomePrenotazione;
        this.orarioPrenotazione = orarioPrenotazione;
    }

}
public enum StatoTavolo
{
    Libero,
    Prenotato,
    Aperto,
    OrdineInviato,
    RichiestaConto,
    Consegnato
}

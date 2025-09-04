const express = require("express");
const mysql = require("mysql2");
const cors = require("cors");

const app = express();
app.use(cors());
app.use(express.json());

// connessione al DB
const db = mysql.createPool({
  host: "localhost",
  user: "root",          // cambialo se hai user diverso
  password: "",          // metti la tua password se c'è
  database: "pizzeria"   // il nome dello schema
});

// API → lista sale
app.get("/sale", (req, res) => {
  db.query("SELECT id, nome FROM Sale", (err, results) => {
    if (err) {
      console.error(err);
      return res.status(500).json({ error: "Errore DB" });
    }
    res.json(results);
  });
});

// GET /tavoli?salaId=1
app.get('/tavoli', (req, res) => {
    const salaId = req.query.salaId;

    if (!salaId) {
        return res.status(400).json({ error: 'Parametro salaId mancante' });
    }

    const query = 'SELECT * FROM Tavoli WHERE sala_id = ?';
    db.query(query, [salaId], (err, results) => {
        if (err) {
            console.error('Errore nel recupero dei tavoli:', err);
            return res.status(500).json({ error: 'Errore interno al server' });
        }

        res.json(results); // restituisce array di tavoli JSON
    });
});

app.post('/sale', (req, res) => {
  const { nome } = req.body;

  if (!nome || nome.trim() === "") {
    return res.status(400).json({ error: "Nome sala mancante" });
  }

  const query = 'INSERT INTO Sale (nome) VALUES (?)';
  db.query(query, [nome], (err, result) => {
    if (err) {
      console.error('Errore nella creazione della sala:', err);
      return res.status(500).json({ error: 'Errore nel database' });
    }

    res.json({ id: result.insertId, nome });
  });
});

app.delete('/sale/:id', (req, res) => {
  const salaId = req.params.id;
  db.query("DELETE FROM Sale WHERE id = ?", [salaId], (err, result) => {
    if (err) return res.status(500).json({ error: "Errore DB" });
    res.json({ message: "Sala eliminata con successo" });
  });
});

app.post("/tavoli", (req, res) => {
  console.log("📥 POST /tavoli body:", req.body);
  const { sala_id, nominativo, numero_posti } = req.body;
  const disponibile = req.body.disponibile ?? true;
  const posti_occupati = req.body.posti_occupati ?? 0;
  const cognome_prenotazione = req.body.cognome_prenotazione ?? "";
  const orario_prenotazione = req.body.orario_prenotazione ?? "";

  const query = `
    INSERT INTO Tavoli 
    (sala_id, nominativo, numero_posti, disponibile, posti_occupati, cognome_prenotazione, orario_prenotazione)
    VALUES (?, ?, ?, ?, ?, ?, ?)
  `;

  db.query(query, [
    sala_id, nominativo, numero_posti, disponibile, posti_occupati, cognome_prenotazione, orario_prenotazione
  ], (err, result) => {
    if (err) {
      console.error("Errore DB:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    const newTavolo = {
      id: result.insertId,
      sala_id,
      nominativo,
      numero_posti,
      disponibile,
      posti_occupati,
      cognome_prenotazione,
      orario_prenotazione
    };

    res.json(newTavolo);
  });
});

app.put("/tavoli/:id", (req, res) => {
  const id = req.params.id;
  const { nominativo, numero_posti } = req.body;

  const query = `
    UPDATE Tavoli
    SET nominativo = ?, numero_posti = ?
    WHERE id = ?
  `;

  db.query(query, [nominativo, numero_posti, id], (err, result) => {
    if (err) {
      console.error("Errore DB:", err);
      return res.status(500).json({ error: "Errore durante l'aggiornamento" });
    }

    res.json({ message: "Tavolo aggiornato con successo" });
  });
});

app.delete("/tavoli/:id", (req, res) => {
  const id = req.params.id;
  db.query("DELETE FROM Tavoli WHERE id = ?", [id], (err, result) => {
    if (err) {
      console.error("Errore eliminazione:", err);
      return res.status(500).json({ error: "Errore durante l'eliminazione" });
    }
    res.json({ message: "Tavolo eliminato con successo" });
  });
});
app.put("/tavoli/:id/prenota", (req, res) => {
  const tavoloId = req.params.id;
  const {
    disponibile,
    posti_occupati,
    cognome_prenotazione,
    orario_prenotazione,
    stato
  } = req.body;

  const query = `
    UPDATE Tavoli SET
      disponibile = ?,
      posti_occupati = ?,
      cognome_prenotazione = ?,
      orario_prenotazione = ?,
      stato = ?
    WHERE id = ?
  `;

  db.query(
    query,
    [disponibile, posti_occupati, cognome_prenotazione, orario_prenotazione, stato, tavoloId],
    (err, result) => {
      if (err) {
        console.error("❌ Errore prenotazione tavolo:", err);
        return res.status(500).json({ error: "Errore DB" });
      }

      res.json({ success: true, message: "Tavolo prenotato correttamente" });
    }
  );
});



// GET /tavoli/nominativi?salaId=2
app.get("/tavoli/nominativi", (req, res) => {
    const query = "SELECT nominativo FROM Tavoli";
    db.query(query, (err, results) => {
        if (err) return res.status(500).json({ error: "Errore DB" });

        const nomi = results.map(r => r.nominativo);
        res.json(nomi);
    });
});

app.put("/tavoli/:id/libera", (req, res) => {
  const id = req.params.id;

  const query = `
    UPDATE Tavoli
    SET 
      disponibile = true,
      posti_occupati = 0,
      cognome_prenotazione = '',
      orario_prenotazione = '',
      stato = 'Libero'
    WHERE id = ?
  `;

  db.query(query, [id], (err, result) => {
    if (err) {
      console.error("Errore liberazione tavolo:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    res.json({ success: true, updated: result.affectedRows });
  });
});
app.put("/tavoli/:id/apri", (req, res) => {
  const id = req.params.id;
  const { stato, posti_occupati } = req.body;

  const query = `
    UPDATE Tavoli
    SET stato = ?, posti_occupati = ?
    WHERE id = ?
  `;

  db.query(query, [stato, posti_occupati, id], (err, result) => {
    if (err) {
      console.error("Errore apertura tavolo:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    res.json({ success: true, updated: result.affectedRows });
  });
});


// GET /tavoli/:id → dettagli tavolo
app.get("/tavoli/:id", (req, res) => {
  const tavoloId = req.params.id;

  const query = `
    SELECT id, sala_id, nominativo, numero_posti, disponibile,
           posti_occupati, cognome_prenotazione, orario_prenotazione
    FROM Tavoli
    WHERE id = ?
  `;

  db.query(query, [tavoloId], (err, results) => {
    if (err) {
      console.error("Errore DB:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    if (results.length === 0) {
      return res.status(404).json({ error: "Tavolo non trovato" });
    }

    res.json(results[0]); // 🔹 ritorna un singolo oggetto
  });
});

// POST /pizze → salva una pizza nel DB
app.post("/pizze", (req, res) => {
  const { nome, prezzo, impasto_id, ingredienti } = req.body;

  if (!nome || !prezzo || !impasto_id || !ingredienti) {
    return res.status(400).json({ error: "Dati incompleti" });
  }

  const query = `
    INSERT INTO Pizze (nome, prezzo, impasto_id, ingredienti)
    VALUES (?, ?, ?, ?)
  `;

  // ingredienti come JSON
  const ingredientiJson = JSON.stringify(ingredienti);

  db.query(query, [nome, prezzo, impasto_id, ingredientiJson], (err, result) => {
    if (err) {
      console.error("Errore inserimento pizza:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    res.json({ 
      success: true,
      pizzaId: result.insertId
    });
  });
});
// POST /ordini → salva un nuovo ordine
app.post("/ordini", (req, res) => {
  const { tavolo_id, prezzo_totale, pizze, prodotti } = req.body;

  const selectQuery = "SELECT * FROM Ordini WHERE tavolo_id = ? LIMIT 1";
  db.query(selectQuery, [tavolo_id], (err, results) => {
    if (err) {
      console.error("❌ Errore SELECT:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    if (results.length > 0) {
      const ordine = results[0];

      let pizzeEsistenti = [];
      try { pizzeEsistenti = JSON.parse(ordine.pizze || "[]"); } catch {}
      let prodottiEsistenti = [];
      try { prodottiEsistenti = JSON.parse(ordine.prodotti || "[]"); } catch {}

      const pizzeFinali = [...pizzeEsistenti, ...pizze];
      const prodottiFinali = [...prodottiEsistenti, ...prodotti];

      // 🔑 Somma numerica sicura
      const prezzoAttuale = parseFloat(ordine.prezzo_totale) || 0;
      const prezzoNuovo = parseFloat(prezzo_totale) || 0;
      const prezzoFinale = prezzoAttuale + prezzoNuovo;

      const updateQuery = `
        UPDATE Ordini 
        SET prezzo_totale = ?, pizze = ?, prodotti = ?, orario_ordine = NOW()
        WHERE tavolo_id = ?`;

      db.query(
        updateQuery,
        [prezzoFinale, JSON.stringify(pizzeFinali), JSON.stringify(prodottiFinali), tavolo_id],
        (err2) => {
          if (err2) {
            console.error("❌ Errore UPDATE:", err2);
            return res.status(500).json({ error: "Errore DB" });
          }
          res.json({ success: true, action: "updated", tavolo_id, prezzo_totale: prezzoFinale });
        }
      );
    } else {
        const insertQuery = `
          INSERT INTO Ordini (tavolo_id, prezzo_totale, pizze, prodotti, orario_ordine)
          VALUES (?, ?, ?, ?, NOW())
        `;


      db.query(
        insertQuery,
        [tavolo_id, parseFloat(prezzo_totale) || 0, JSON.stringify(pizze), JSON.stringify(prodotti)],
        (err3, result) => {
          if (err3) {
            console.error("❌ Errore INSERT:", err3);
            return res.status(500).json({ error: "Errore DB" });
          }
          res.json({ success: true, action: "inserted", id: result.insertId });
        }
      );
    }
  });
});



// GET /ordini?tavoloId=1 → ritorna ultimo ordine di un tavolo
app.get("/ordini", (req, res) => {
  console.log("📥 GET /ordini query:", req.query);
  const tavoloId = req.query.tavoloId;
  if (!tavoloId) {
    return res.status(400).json({ error: "Parametro tavoloId mancante" });
  }

  const query = `
    SELECT * FROM Ordini
    WHERE tavolo_id = ?
    LIMIT 1
  `;

  db.query(query, [tavoloId], (err, results) => {
    if (err) {
      console.error("❌ Errore DB:", err);
      return res.status(500).json({ error: "Errore DB" });
    }
    res.json(results[0] || {});
  });
});

// PUT /tavoli/:id/ordine-inviato
app.put("/tavoli/:id/ordine-inviato", (req, res) => {
  const id = req.params.id;
  const { stato } = req.body;

  if (!stato || stato !== "OrdineInviato") {
    return res.status(400).json({ error: "Valore 'stato' non valido o mancante" });
  }

  const query = `
    UPDATE Tavoli
    SET stato = ?
    WHERE id = ?
  `;

  db.query(query, [stato, id], (err, result) => {
    if (err) {
      console.error("❌ Errore update stato OrdineInviato:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    res.json({ success: true, updated: result.affectedRows });
  });
});


// avvio server
app.listen(3000, () => {
  console.log("API in ascolto su http://localhost:3000");
});

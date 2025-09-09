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
// POST /ordini → crea o aggiorna un ordine per un tavolo
app.post("/ordini", async (req, res) => {
  const { tavolo_id, prezzo_totale, pizze, prodotti } = req.body;

  try {
// 1️⃣ Controllo e decremento giacenza prodotti
for (const p of prodotti) {
  const [rows] = await db.promise().query(
    "SELECT giacenza FROM prodotti WHERE id = ?",
    [p.id]
  );

  if (rows.length === 0) {
    return res.status(404).json({ error: `Prodotto id=${p.id} non trovato` });
  }

  if (rows[0].giacenza < p.quantita) {
    return res.status(400).json({ error: `Giacenza insufficiente per prodotto id=${p.id}` });
  }

  await db.promise().query(
    "UPDATE prodotti SET giacenza = giacenza - ? WHERE id = ?",
    [p.quantita, p.id]
  );
}

// 2️⃣ Controlla prima se c'è un ordine in ATTESA
const [ordiniInAttesa] = await db.promise().query(
  "SELECT * FROM Ordini WHERE tavolo_id = ? AND stato = 'InAttesa' LIMIT 1",
  [tavolo_id]
);

if (ordiniInAttesa.length > 0) {
  // ✏️ AGGIORNA ordine esistente
  const ordine = ordiniInAttesa[0];
  const pizzeFinali = [...JSON.parse(ordine.pizze || "[]"), ...pizze];
  const prodottiFinali = [...JSON.parse(ordine.prodotti || "[]"), ...prodotti];
  const prezzoFinale = parseFloat(ordine.prezzo_totale || 0) + parseFloat(prezzo_totale || 0);

  await db.promise().query(
    "UPDATE Ordini SET prezzo_totale = ?, pizze = ?, prodotti = ?, orario_ordine = NOW() WHERE id = ?",
    [prezzoFinale, JSON.stringify(pizzeFinali), JSON.stringify(prodottiFinali), ordine.id]
  );

  return res.json({ success: true, action: "updated", id: ordine.id, prezzo_totale: prezzoFinale });
}

// 3️⃣ Se non c’è un ordine in attesa, controlla se c'è uno in preparazione
const [ordiniInPreparazione] = await db.promise().query(
  "SELECT * FROM Ordini WHERE tavolo_id = ? AND stato = 'InPreparazione' LIMIT 1",
  [tavolo_id]
);

// In entrambi i casi (inPreparazione o nessuno), CREIAMO UN NUOVO ORDINE
const [result] = await db.promise().query(
  "INSERT INTO Ordini (tavolo_id, prezzo_totale, pizze, prodotti, stato, orario_ordine) VALUES (?, ?, ?, ?, 'InAttesa', NOW())",
  [tavolo_id, parseFloat(prezzo_totale), JSON.stringify(pizze), JSON.stringify(prodotti)]
);

return res.json({ success: true, action: "inserted", id: result.insertId });


  } catch (err) {
    console.error("❌ Errore POST /ordini:", err);
    return res.status(500).json({ error: "Errore interno server" });
  }
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
  `;

  db.query(query, [tavoloId], (err, results) => {
    if (err) {
      console.error("❌ Errore DB:", err);
      return res.status(500).json({ error: "Errore DB" });
    }
    res.json(results);
  });
});

// PUT /tavoli/:id/ordine-inviato
// PUT /tavoli/:id/stato
app.put("/tavoli/:id/stato", (req, res) => {
  const id = req.params.id;
  const { stato } = req.body;

  if (!stato) {
    return res.status(400).json({ error: "Campo 'stato' mancante" });
  }

  const query = `
    UPDATE Tavoli
    SET stato = ?
    WHERE id = ?
  `;

  db.query(query, [stato, id], (err, result) => {
    if (err) {
      console.error("❌ Errore aggiornamento stato tavolo:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    res.json({ success: true, updated: result.affectedRows });
  });
});


// GET /ordini/inviati
app.get("/ordini/inviati", (req, res) => {
  const query = `
    SELECT o.*, t.nominativo AS tavolo_nome
    FROM Ordini o
    JOIN Tavoli t ON t.id = o.tavolo_id
    WHERE o.stato != 'Consegnato' and o.stato != 'Chiuso' and o.stato != 'RichiestaConto'
    ORDER BY o.orario_ordine ASC
  `;

  db.query(query, (err, results) => {
    if (err) {
      console.error("❌ Errore DB:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    res.json(results);
  });
});


app.put("/ordini/:id/stato", (req, res) => {
  const ordineId = req.params.id;
  const { stato } = req.body;

  const query = `
    UPDATE Ordini
    SET stato = ?
    WHERE id = ?
  `;

  db.query(query, [stato, ordineId], (err, result) => {
    if (err) {
      console.error("❌ Errore aggiornamento stato ordine:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    res.json({ success: true, updated: result.affectedRows });
  });
});

// DELETE /ordini/chiudi?tavoloId=3
app.delete("/ordini/chiudi", (req, res) => {
  const tavoloId = req.query.tavoloId;
  if (!tavoloId) return res.status(400).json({ error: "tavoloId mancante" });

  const query = `DELETE FROM Ordini WHERE tavolo_id = ?`;
  db.query(query, [tavoloId], (err, result) => {
    if (err) {
      console.error("❌ Errore DELETE ordini:", err);
      return res.status(500).json({ error: "Errore DB" });
    }
    res.json({ success: true, deleted: result.affectedRows });
  });
});

// POST /ingredienti → consuma 1 unità di un ingrediente
app.post("/ingredienti", (req, res) => {
  const { id, quantita } = req.body;

  if (!id || !quantita) {
    return res.status(400).json({ error: "Dati incompleti" });
  }

  // prima recupera la giacenza corrente
  const selectQuery = "SELECT giacenza, nome FROM ingredienti WHERE id = ?";
  db.query(selectQuery, [id], (err, results) => {
    if (err) {
      console.error("❌ Errore DB:", err);
      return res.status(500).json({ error: "Errore DB" });
    }

    if (results.length === 0) {
      return res.status(404).json({ error: "Ingrediente non trovato" });
    }

    const giacenzaAttuale = results[0].giacenza;
    const nomeIngrediente = results[0].nome;

    if (giacenzaAttuale < quantita) {
      // ❌ non ci sono abbastanza ingredienti
      return res.status(400).json({ error: `Giacenza insufficiente per ${nomeIngrediente}` });
    }

    // ✅ aggiorna giacenza
    const updateQuery = "UPDATE ingredienti SET giacenza = giacenza - ? WHERE id = ?";
    db.query(updateQuery, [quantita, id], (err2, result) => {
      if (err2) {
        console.error("❌ Errore DB:", err2);
        return res.status(500).json({ error: "Errore DB" });
      }

      res.json({ success: true, message: `${quantita} unità di ${nomeIngrediente} consumata` });
    });
  });
});

// Lista ingredienti
app.get('/ingredienti', (req, res) => {
  const q = 'SELECT id, nome, giacenza AS quantita FROM ingredienti ORDER BY nome';
  db.query(q, (err, rows) => {
    if (err) {
      console.error("❌ Errore select:", err);
      return res.status(500).json({ error: 'Errore DB' });
    }
    res.json(rows); // ← array JSON
  });
});

// Dettaglio singolo ingrediente (opzionale)
app.get('/ingredienti/:id', (req, res) => {
  const q = 'SELECT id, nome, giacenza AS quantita FROM ingredienti WHERE id = ?';
  db.query(q, [req.params.id], (err, rows) => {
    if (err) {
      console.error("❌ Errore select:", err);
      return res.status(500).json({ error: 'Errore DB' });
    }
    if (!rows.length) return res.status(404).json({ error: 'Non trovato' });
    res.json(rows[0]);
  });
});

// Già esistente: aggiorna giacenza
app.put('/ingredienti/:id', (req, res) => {
  const id = req.params.id;
  const quantita = req.body.quantita;           // Unity manda "quantita"
  const query = 'UPDATE ingredienti SET giacenza = ? WHERE id = ?';
  db.query(query, [quantita, id], (err) => {
    if (err) {
      console.error("❌ Errore update:", err);
      return res.status(500).json({ error: 'Errore DB' });
    }
    res.json({ success: true });
  });
});

// GET: lista di tutti i prodotti
app.get("/prodotti", (req, res) => {
  db.query("SELECT * FROM prodotti", (err, results) => {
    if (err) {
      console.error("❌ Errore query prodotti:", err);
      return res.status(500).json({ error: "Errore DB" });
    }
    res.json(results);
  });
});

// POST: consuma un prodotto (decrementa la giacenza)
app.post("/prodotti", (req, res) => {
  const { id, quantita } = req.body;

  if (!id || !quantita) {
    return res.status(400).json({ error: "Parametri mancanti" });
  }

  // 1. Controlla giacenza attuale
  db.query("SELECT giacenza FROM prodotti WHERE id = ?", [id], (err, rows) => {
    if (err) {
      console.error("❌ Errore query:", err);
      return res.status(500).json({ error: "Errore DB" });
    }
    if (rows.length === 0) {
      return res.status(404).json({ error: "Prodotto non trovato" });
    }

    const giacenzaAttuale = rows[0].giacenza;
    if (giacenzaAttuale < quantita) {
      return res.status(400).json({ error: "Giacenza insufficiente" });
    }

    // 2. Aggiorna giacenza
    db.query(
      "UPDATE prodotti SET giacenza = giacenza - ? WHERE id = ?",
      [quantita, id],
      (err2) => {
        if (err2) {
          console.error("❌ Errore update:", err2);
          return res.status(500).json({ error: "Errore aggiornamento DB" });
        }
        res.json({ message: "✅ Prodotto consumato", id, quantita });
      }
    );
  });
});

app.post('/report/giornaliero/pizze', (req, res) => {
  const { num_pizze, totale } = req.body;
  const today = new Date().toISOString().slice(0, 10); // YYYY-MM-DD

  console.log("📥 Report giornaliero pizze:", req.body);
  const sql = `
    INSERT INTO storico_giornaliero_pizze (data, num_pizze, totale)
    VALUES (?, ?, ?)
    ON DUPLICATE KEY UPDATE
      num_pizze = num_pizze + VALUES(num_pizze),
      totale = totale + VALUES(totale)
  `;

  db.query(sql, [today, num_pizze, totale], (err) => {
    if (err) {
      console.error("❌ Errore inserimento report:", err);
      return res.status(500).json({ error: "Errore DB" });
    }
    res.json({ success: true });
  });
});


// avvio server
app.listen(3000, () => {
  console.log("API in ascolto su http://localhost:3000");
});

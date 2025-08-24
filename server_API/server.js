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

// avvio server
app.listen(3000, () => {
  console.log("API in ascolto su http://localhost:3000");
});

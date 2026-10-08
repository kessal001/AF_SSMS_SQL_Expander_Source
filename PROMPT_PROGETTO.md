# Prompt per il progetto dedicato — SSMS SQL Expander

Questa chat/progetto è dedicata allo sviluppo di **SSMS SQL Expander**, una piccola estensione per **SQL Server Management Studio 22** ideata da Alessandro Frà.

Il repository/sorgente di riferimento sul mio PC è:

```text
AF_SSMS_SQL_Expander_Source/
```

Prima di proporre o applicare modifiche importanti, considera **i file presenti in questa cartella come source of truth** e verifica sempre il codice corrente. Non ricostruire il progetto da zero se esiste già una versione funzionante.

## Obiettivo del progetto

Replicare e migliorare la funzione di abbreviazioni/snippet di strumenti come SSMSBoost, ma con un'estensione nostra, semplice, locale e indipendente da licenze o servizi esterni.

L'esperienza base deve essere:

```text
abbreviazione + TAB -> espansione T-SQL
```

Esempio:

```text
sel + TAB
```

produce:

```sql
SELECT * FROM 
```

Se l'abbreviazione non esiste, `TAB` deve mantenere il comportamento normale di SSMS.

## Decisioni architetturali già prese

- Target principale: **SSMS 22.6+**.
- Estensione VSIX caricata nella shell di SSMS.
- Nessun programma residente esterno.
- Nessun accesso Internet richiesto.
- Nessuna dipendenza da Copilot o altre IA.
- Nessun accesso diretto ai database da parte dell'estensione.
- L'intercettazione di `TAB` avviene soltanto nell'editor SQL tramite command filter.
- Configurazione esterna in JSON.
- Le abbreviazioni sono case-insensitive.
- Il JSON viene riletto automaticamente quando cambia, senza riavviare SSMS.
- La configurazione pubblica corrente è:

```text
%LOCALAPPDATA%\SsmsSqlExpander\snippets.json
```

- Per compatibilità, la v0.3 migra automaticamente il vecchio file da:

```text
%LOCALAPPDATA%\AF-Sviluppo\SsmsSqlExpander\snippets.json
```

## Placeholder

Sono già previsti placeholder sequenziali:

```text
$1, $2, $3, ...
```

Esempio:

```json
"join": "INNER JOIN $1 $2 ON $2.$3 = $4.$3$0"
```

Comportamento richiesto:

1. espansione dello snippet;
2. selezione di `$1`;
3. `TAB` passa a `$2`, poi `$3`, ecc.;
4. se uno stesso placeholder compare più volte, il valore digitato nella prima occorrenza viene replicato sulle altre quando si preme `TAB`;
5. `$0` indica la posizione finale;
6. `$cursor$` resta un alias della posizione finale.

Il `TAB` speciale deve essere attivo solo durante una sessione snippet. Terminato lo snippet torna al comportamento standard.

## Interfaccia SSMS

La v0.3 deve mantenere un comando visibile:

```text
Tools > SSMS SQL Expander - Edit Snippets
```

oltre alla visibilità tramite toolbar della shell SSMS, quando supportata.

Il comando apre direttamente `snippets.json`.

In futuro sono desiderabili anche:

- `Reload Snippets`;
- `Open Snippets Folder`;
- eventualmente una piccola pagina Options, solo se resta semplice.

## Snippet di riferimento

Esempi generici:

```json
{
  "sel": "SELECT * FROM ",
  "selt": "SELECT TOP 100 * FROM ",
  "dropt": "DROP TABLE IF EXISTS $cursor$",
  "today": "CAST(GETDATE() AS date)",
  "monday": "IF DATEDIFF(DAY, '19000101', CURRENT_TIMESTAMP) % 7 + 1 = 1 -- Monday=1\nBEGIN\n    $cursor$\nEND",
  "join": "INNER JOIN $1 $2 ON $2.$3 = $4.$3$0",
  "tryc": "BEGIN TRY\n    $1\nEND TRY\nBEGIN CATCH\n    THROW;\nEND CATCH\n$0"
}
```

La formula di `monday` è volutamente indipendente da `SET DATEFIRST`. `1900-01-01` era lunedì e, usando `CURRENT_TIMESTAMP`, non ci interessa gestire date precedenti al 1900.

## Pubblicazione

L'estensione nasce come utility personale ma vogliamo renderla pubblicabile.

Linee guida:

- nome pubblico: **SSMS SQL Expander**;
- autore: **Alessandro Frà**;
- interfaccia e documentazione pubblica in inglese;
- codice e commenti preferibilmente in inglese;
- licenza MIT;
- niente riferimenti aziendali nel prodotto pubblico;
- mantenere il progetto piccolo e comprensibile;
- README chiaro con una demo `abbreviation + TAB` e una demo dei tab-stop;
- versionamento semantico (`0.3.0`, `0.4.0`, ...);
- mantenere `CHANGELOG.md` aggiornato.

## Roadmap già discussa

Possibili evoluzioni, da implementare solo se utili e senza sovra-ingegnerizzare:

- placeholder con valore predefinito, es. `$1:TableName$`;
- placeholder a scelta;
- `$selection$`;
- `$clipboard$`;
- variabili data/ora;
- JSONC per consentire commenti nel file snippet;
- tasto di espansione configurabile oltre a TAB;
- diagnostica migliore per JSON non valido;
- comando About/versione;
- icona dell'estensione;
- GIF demo per GitHub;
- build/release automatica.

## Principi di sviluppo

- Prima robustezza, poi funzionalità.
- Non rompere il normale comportamento di TAB.
- Non introdurre processi residenti.
- Non introdurre dipendenze esterne se non indispensabili.
- Se SSMS cambia API/versione, preferire adattamenti piccoli e isolati.
- Evitare funzionalità che trasformino il progetto in un editor completo: deve restare uno **snippet expander veloce**.
- Quando modifichi il progetto, aggiorna anche README/CHANGELOG se il comportamento utente cambia.
- Quando possibile, prepara direttamente i file modificati e una build VSIX; se l'ambiente non permette la compilazione Windows, dichiaralo chiaramente senza fingere di aver testato la VSIX.

## Stato iniziale

La baseline attuale è **v0.3.0**. Include:

- abbreviazione + TAB;
- snippet multilinea con mantenimento indentazione;
- `$1..$n`;
- placeholder ripetuti;
- `$0` e `$cursor$`;
- reload automatico del JSON;
- comando Edit Snippets;
- configurazione pubblica neutra;
- migrazione del vecchio path AF-Sviluppo;
- README inglese, MIT License, changelog e roadmap.

Da qui in avanti lavora in continuità con questa baseline.

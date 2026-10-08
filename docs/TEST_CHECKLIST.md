# Manual test checklist

Use this list after every VSIX build on Windows/SSMS 22.6 or later. Record the exact host version and architecture; build/package checks do not cover these runtime checks.

## Installation

- [ ] VSIX installs with SSMS closed.
- [ ] SSMS starts normally after installation.
- [ ] `Tools > SSMS SQL Expander - Edit Snippets` is visible.
- [ ] Toolbar command is visible where supported by the current SSMS shell.

## Configuration

- [ ] First run creates `%LOCALAPPDATA%\SsmsSqlExpander\snippets.json`.
- [ ] Legacy config is copied from `%LOCALAPPDATA%\AF-Sviluppo\SsmsSqlExpander\snippets.json` when applicable.
- [ ] Editing the JSON changes expansions without restarting SSMS.
- [ ] Invalid JSON does not crash or block the SQL editor.

## Expansion

- [ ] With `isna` present in the active `snippets.json`, `isna + TAB` expands to `ISNULL(annullato,'')<>'S'`.
- [ ] `sel + TAB` expands to `SELECT * FROM `.
- [ ] Unknown abbreviation + TAB behaves like normal TAB.
- [ ] Abbreviations are case-insensitive.
- [ ] Expansion works in an indented SQL line.
- [ ] Multi-line expansion preserves the current line indentation.
- [ ] `$cursor$` is removed and places the caret correctly.
- [ ] `$0` is removed and places the caret correctly.

## Tab stops

- [ ] `$1` is selected after expansion.
- [ ] TAB advances `$1 -> $2 -> $3`.
- [ ] Repeated placeholders synchronize when leaving the primary placeholder.
- [ ] Final TAB moves to `$0`/`$cursor$`.
- [ ] After the snippet session ends, TAB returns to normal editor behavior.
- [ ] Moving the caret outside the active placeholder cancels the special TAB behavior safely.

## Regression

- [ ] Normal SSMS IntelliSense continues to work.
- [ ] Copilot inline suggestions still work.
- [ ] Result grids and Object Explorer are unaffected.
- [ ] Multiple SQL query tabs can each expand snippets.

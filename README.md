# SSMS SQL Expander

**SSMS SQL Expander** is a lightweight productivity extension for SQL Server Management Studio 22.

Type a short abbreviation, press `TAB`, and expand it into deterministic T-SQL. It is designed for the small pieces of SQL you type repeatedly and want to expand the same way every time.

No background process, no AI service, no network access and no database access are required by the extension itself.

## Example

With this configuration:

```json
{
  "sel": "SELECT * FROM ",
  "dropt": "DROP TABLE IF EXISTS $cursor$"
}
```

Typing:

```text
sel<TAB>
```

produces:

```sql
SELECT * FROM 
```

## Sequential tab stops

Use `$1`, `$2`, `$3`, ... for editable positions and `$0` for the final cursor position.

```json
{
  "join": "INNER JOIN $1 $2 ON $2.$3 = $4.$3$0"
}
```

After `join + TAB`, the editor selects `$1`. Each following `TAB` moves to the next placeholder.

Repeated placeholders are synchronized when you leave that placeholder with `TAB`. For example, entering `a` in `$2` replaces every `$2` occurrence with `a`.

`$cursor$` is supported as an alias for the final cursor position.

## Multi-line snippets and indentation

Multi-line expansions inherit the indentation of the current SQL line.

Example:

```json
{
  "tryc": "BEGIN TRY\n    $1\nEND TRY\nBEGIN CATCH\n    THROW;\nEND CATCH\n$0"
}
```

## Configuration

The active configuration is stored at:

```text
%LOCALAPPDATA%\SsmsSqlExpander\snippets.json
```

If version 0.1/0.2 created the old configuration under:

```text
%LOCALAPPDATA%\AF-Sviluppo\SsmsSqlExpander\snippets.json
```

version 0.3 automatically copies it to the new public path the first time it runs.

The configuration is reloaded automatically when the JSON file changes. Restarting SSMS is not required.

## Edit Snippets command

The extension registers:

```text
Tools > SSMS SQL Expander - Edit Snippets
```

and also exposes the same command through the SSMS/Visual Studio shell toolbar command placement.

The command opens the active `snippets.json` file with the Windows-associated JSON editor.

## Sample snippets

See:

- `snippets.example.json` for generic public examples;
- `snippets.personal.example.json` for a practical starter set.

Example day-of-week snippet that does not depend on `SET DATEFIRST`:

```json
{
  "monday": "IF DATEDIFF(DAY, '19000101', CURRENT_TIMESTAMP) % 7 + 1 = 1 -- Monday=1\nBEGIN\n    $cursor$\nEND"
}
```

## Build

Requirements on Windows:

- Visual Studio / Build Tools with MSBuild;
- **Visual Studio extension development** workload;
- .NET Framework 4.8 targeting support.

Build from PowerShell:

```powershell
.\Build.ps1
```

or open `SSMS_SQL_Expander.sln` and build `Release`.

> The source package can be prepared and statically checked outside Windows, but the VSIX must be built and tested on Windows with the Visual Studio extension toolchain.

## Install

Close SSMS, then run:

```powershell
.\Install.ps1
```

The script expects the SSMS 22 VSIX installer at:

```text
C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\VSIXInstaller.exe
```

## Uninstall

Close SSMS and run:

```powershell
.\Uninstall.ps1
```

## Error log

Only extension errors are logged, at:

```text
%LOCALAPPDATA%\SsmsSqlExpander\errors.log
```

## Design goals

- deterministic expansion rather than probabilistic completion;
- zero resident process outside SSMS;
- local, human-editable configuration;
- minimal interference with normal `TAB` behavior;
- small codebase that can be adapted if future SSMS releases change the extension shell.

## Compatibility note

SSMS does not guarantee compatibility for third-party extensions across all releases. This project deliberately keeps the integration surface small: an editor command filter plus a lightweight package/menu command.

## License

MIT License. Copyright (c) 2026 Alessandro Frà.

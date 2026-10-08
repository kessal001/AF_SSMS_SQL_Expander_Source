# SSMS SQL Expander

[![Build VSIX](https://github.com/kessal001/AF_SSMS_SQL_Expander_Source/actions/workflows/build.yml/badge.svg)](https://github.com/kessal001/AF_SSMS_SQL_Expander_Source/actions/workflows/build.yml)

A lightweight extension that expands abbreviations into deterministic T-SQL with `TAB`.
It supports numbered placeholders, repeated values and multi-line indentation.
No background process, AI service, network connection or database access is required.

## Quick example

```json
{
  "sel": "SELECT * FROM ",
  "join": "INNER JOIN $1 $2 ON $2.$3 = $4.$3$0"
}
```

Type `sel` and press `TAB` to insert `SELECT * FROM `. With `join`, replace the
selected `$1`, then press `TAB` to visit `$2`, `$3` and `$4`. Repeated values are
synchronized when leaving their primary placeholder. `$0` is the final caret
position; `$cursor$` is an alias. Use one final-position marker per snippet.

## Requirements and compatibility

- Windows and .NET Framework 4.8.
- The current manifest targets **SSMS 22.6 or later**, x64 and ARM64.
- For building: Visual Studio / Build Tools with MSBuild, Visual Studio extension
  development tooling and the .NET Framework 4.8 targeting pack.

The manifest expresses install targets; runtime compatibility must be tested on
individual SSMS versions and architectures. Microsoft states that
[third-party SSMS extensions are not supported](https://learn.microsoft.com/en-us/ssms/faq).
Use the [manual checklist](docs/TEST_CHECKLIST.md) before distributing a tested release.

## Build

From the repository root in PowerShell:

```powershell
.\Build.ps1
```

The script finds MSBuild (including a fallback to Visual Studio Insiders), validates
the repository, builds the active solution and validates the VSIX package. The
ready-to-install output is:

```text
artifacts/SsmsSqlExpander.vsix
```

Alternatively open `SSMS_SQL_Expander.sln` in Visual Studio. Advanced builds can use
`-Configuration Debug` or `-MsBuildPath 'C:\path\to\MSBuild.exe'`.
GitHub Actions builds the same solution and provides a downloadable VSIX artifact
under [Actions](https://github.com/kessal001/AF_SSMS_SQL_Expander_Source/actions).

## Install or uninstall

Close all SSMS instances, then run:

```powershell
.\Install.ps1
# Later, to remove the extension:
.\Uninstall.ps1
```

Both scripts discover the installed compatible SSMS through `vswhere` and wait for
the VSIX installer result. Use `-InstallerPath 'C:\path\to\VSIXInstaller.exe'` for
custom installations, or `-WhatIf` to preview the operation. Installation defaults
to the validated package in `artifacts/`; `-VsixPath` accepts another package with
the current extension identity and version.

## Configuration

The active JSON file is stored at:

```text
%LOCALAPPDATA%\SsmsSqlExpander\snippets.json
```

Open it using **Tools > SSMS SQL Expander - Edit Snippets**. The command uses the
Windows-associated JSON editor. Changes reload on the next snippet lookup without
restarting SSMS; invalid JSON retains the last valid configuration.

On first use, an existing configuration from
`%LOCALAPPDATA%\AF-Sviluppo\SsmsSqlExpander\snippets.json` is copied if the new file
does not exist. Migration preserves existing abbreviation names. Default and
sample configurations use `isna`; older files with `insa` must be edited if you want
the new spelling.

Start from [generic examples](examples/snippets.example.json) or the
[practical starter set](examples/snippets.personal.example.json). Multi-line snippets
use JSON `\n` escapes and inherit the current line's indentation. Errors are logged
locally at `%LOCALAPPDATA%\SsmsSqlExpander\errors.log`.

## Repository layout

```text
src/SsmsSqlExpander/       Active VSIX project
  Editor/                 SQL view integration and placeholder sessions
  Configuration/          JSON storage, migration and diagnostics
  Commands/               Edit Snippets command
docs/                     Architecture, host test checklist and roadmap
examples/                 Generic snippet configurations
scripts/                  Validation and SSMS installer discovery
archive/v0.1/             Previous source, excluded from the active build
.github/                  Windows build workflow and issue template
artifacts/                Local generated VSIX (ignored by Git)
```

Read the [architecture and analysis](docs/ARCHITECTURE.md),
[contributing guide](CONTRIBUTING.md), [changelog](CHANGELOG.md) and
[roadmap](docs/ROADMAP.md). The historical project shares the current VSIX identity;
build and install only the active project.

## License

[MIT](LICENSE). Copyright (c) 2026 Alessandro Frà.

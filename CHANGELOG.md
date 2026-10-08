# Changelog

## Unreleased

- Organized current sources under src/, documentation under docs/, and snippets under examples/.
- Preserved the previous project in archive/v0.1/, outside the active solution.
- Added repository/package validation, Windows CI, contribution guidelines and editor conventions.
- Fixed MSBuild discovery for Visual Studio Insiders, the missing System.Design reference, and the COM error constant reference.
- Added UI-thread guards before forwarding COM editor commands.
- Corrected VSIX metadata element order to match the package schema.
- Unified installation discovery with the current manifest requirement and added installer exit-code checks and -WhatIf.
- Standardized local VSIX delivery at artifacts/SsmsSqlExpander.vsix.
- Documented the missing legacy insa alias and runtime validation limits.

## 0.3.0 - 2026-09-30

- Renamed the public-facing extension to **SSMS SQL Expander**.
- Switched UI, manifest and documentation to English.
- Added author/publisher metadata for Alessandro Frà.
- Added MIT license and repository-ready files.
- Moved the public configuration path to `%LOCALAPPDATA%\SsmsSqlExpander\snippets.json`.
- Added automatic one-time migration from the legacy `%LOCALAPPDATA%\AF-Sviluppo\SsmsSqlExpander\snippets.json` path.
- Added a `monday` sample based on `DATEDIFF(DAY, '19000101', CURRENT_TIMESTAMP) % 7 + 1`.
- Kept sequential placeholders (`$1`, `$2`, ...), repeated-placeholder synchronization and `$0` / `$cursor$` final position.
- Kept quick access through the Tools menu and toolbar command placement.
- Added a dedicated-project prompt and repository-ready documentation.
- Restored the proven VSSDK build settings used by the previous working project for more deterministic command-line VSIX builds.
- Restricted `Install.ps1` to the current `SsmsSqlExpander\bin\Release` output so a legacy VSIX cannot be installed by mistake.
- Added a compatibility alias for the v0.1 default typo `insa`, so migrated configurations also respond to the intended `isna` abbreviation.
- Added the `isna` smoke-test snippet to the default and example configuration so a clean installation can verify TAB expansion immediately.

## 0.2.0

- Added sequential tab stops.
- Added repeated placeholders.
- Added an Edit Snippets command in SSMS.

## 0.1.0

- Initial deterministic `abbreviation + TAB` expansion.

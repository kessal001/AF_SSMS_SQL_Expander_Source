# Architecture

## Active project

The repository has one maintained .NET Framework 4.8 VSIX project in
`src/SsmsSqlExpander/`. All types retain the `SsmsSqlExpander` namespace, assembly
name, package GUID and extension identity so moving files does not change registration.
The SDK project includes C# files recursively; no extra project layers are needed
for this small codebase.

| Location | Responsibility |
| --- | --- |
| `Editor/SqlTextViewCreationListener.cs` | Attach a command filter to editable SQL views through MEF. |
| `Editor/TabExpansionCommandFilter.cs` | Intercept TAB, find the abbreviation, apply indentation, and delegate unhandled commands. |
| `Editor/SnippetSession.cs` | Track editable positions, advance numbered placeholders and synchronize repeats. |
| `Configuration/SnippetStore.cs` | Create/migrate local JSON, reload changed files, retain the last valid configuration and log failures. |
| `Commands/OpenSnippetsCommand.cs` | Open the configuration with the associated Windows editor. |
| `SqlExpanderPackage.cs` and `SqlExpanderCommands.vsct` | Register the package and Tools/menu command. |
| `source.extension.vsixmanifest` | Define extension identity, version, host requirements and packaged assets. |

## Expansion flow

1. SSMS creates an editable SQL view and the MEF listener attaches the filter.
2. TAB first advances an active snippet session, if one exists.
3. Otherwise the filter reads the word immediately preceding the caret and asks
   `SnippetStore.TryGet` for its expansion.
4. A match is inserted with the line's indentation. `SnippetSession` tracks
   `$1`, `$2`, ... and the first `$0` or `$cursor$` final position.
5. Commands not handled by the extension are forwarded to SSMS.

Configuration is shared across views; session state belongs to each view's filter.
No SQL is executed by the extension, and no database or network connection is made.
COM command forwarding and editor operations run on the SSMS UI thread.

## Build and distribution

`Build.ps1` validates source references, PowerShell syntax, example JSON and version
consistency, then rebuilds only the active solution. It validates the ZIP contents
and manifest identity of the generated VSIX before copying it to
`artifacts/SsmsSqlExpander.vsix`. Generated artifacts remain outside Git.

`Install.ps1` and `Uninstall.ps1` share installer discovery in
`scripts/SsmsInstallation.ps1`. Automatic discovery follows the minimum SSMS
version in the current manifest. An explicit `-InstallerPath` is available for
custom installations; the VSIX installer still enforces host compatibility.

CI runs the same build on a Windows GitHub runner and uploads the validated VSIX
as an artifact. It does not install SSMS or publish a Marketplace extension/release.

## Validation limits

A successful build verifies compilation and packaging. It does not prove that
MEF composition, command placement, placeholder tracking or IntelliSense behave
correctly in SSMS. Follow [the manual checklist](TEST_CHECKLIST.md) on every
supported host/version/architecture before claiming runtime compatibility.

## Follow-up findings

These are code-review findings, not verified runtime failures:

- Invalid JSON is retried and logged on subsequent lookups because the last valid
  timestamp is retained. Consider throttling failures without losing reload behavior.
- The changelog mentions an `insa` compatibility alias, but the current store does
  not implement it. Migration copies existing keys unchanged; use `isna` explicitly.
- Only the first final-position marker is tracked. Document a single final marker
  per snippet; validate/reject additional markers in a future parser change.
- Snippet edits are not explicitly grouped into one undo transaction. Verify undo,
  caret cancellation and multi-view behavior before changing this integration.

Introduce a separate pure snippet parser and focused automated behavioral tests
when extending marker syntax. Avoid adding abstraction layers that merely forward
the existing VS editor interfaces.

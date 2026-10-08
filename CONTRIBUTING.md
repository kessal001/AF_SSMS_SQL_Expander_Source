# Contributing

Use the current solution `SSMS_SQL_Expander.sln` and sources in `src/`.
`archive/` is historical reference and is not part of the active build.

1. Install Visual Studio or Build Tools with MSBuild, Visual Studio extension
   development tooling and the .NET Framework 4.8 targeting pack.
2. Run `./scripts/Test-Repository.ps1` for repository checks.
3. Run `./Build.ps1` for compilation and package validation.
4. Test host behavior using [docs/TEST_CHECKLIST.md](docs/TEST_CHECKLIST.md).

Keep extension IDs/GUIDs stable. When changing the release version, update both
the project and VSIX manifest, and record changes in `CHANGELOG.md`.
Follow `.editorconfig`, preserve the MIT attribution, and use generic SQL examples.
Never commit credentials, connection strings, customer snippets or generated binaries.

Pull requests should describe the resulting behavior and list actual validation,
including the SSMS version and architecture used for manual testing. Do not treat
a successful package build as proof of runtime compatibility.

# Roadmap

## Near term

- Validate and harden VSIX loading on current SSMS 22 releases.
- Confirm toolbar placement across SSMS 22 minor versions.
- Add an optional `Reload Snippets` command even though file changes are already auto-detected.
- Add `Open Snippets Folder`.
- Improve diagnostics when `snippets.json` is invalid.
- Add a small version/about command.

## Snippet engine ideas

- Default placeholder text, for example `$1:TableName$`.
- Choice placeholders, for example `${1|INNER,LEFT,RIGHT|}`.
- Optional selection variable such as `$selection$`.
- Clipboard variable such as `$clipboard$`.
- Date/time variables.
- JSONC support for comments.
- Configurable expansion key in addition to TAB.
- Scope rules so individual snippets can be limited to T-SQL contexts.

## Distribution

- [x] Create a GitHub repository.
- Add a simple extension icon.
- Add an animated GIF showing `join + TAB` and placeholder navigation.
- [x] Automate VSIX builds and artifact upload with GitHub Actions.
- Add a tested release workflow after manual SSMS compatibility validation.
- Investigate publication/distribution options for SSMS extensions.

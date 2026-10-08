using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace AfSsmsSqlExpander
{
    internal sealed class TabExpansionCommandFilter : IOleCommandTarget
    {
        private readonly IWpfTextView _view;
        private IOleCommandTarget _next;

        public TabExpansionCommandFilter(IWpfTextView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void SetNext(IOleCommandTarget next)
        {
            _next = next;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _next?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText)
                   ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (pguidCmdGroup == VSConstants.VSStd2K &&
                nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB &&
                TryExpandSnippet())
            {
                return VSConstants.S_OK;
            }

            return _next?.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
                   ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        private bool TryExpandSnippet()
        {
            try
            {
                if (!_view.Selection.IsEmpty || _view.Caret.Position.VirtualBufferPosition.IsInVirtualSpace)
                    return false;

                SnapshotPoint caret = _view.Caret.Position.BufferPosition;
                ITextSnapshot snapshot = caret.Snapshot;
                ITextSnapshotLine line = caret.GetContainingLine();

                int lineStart = line.Start.Position;
                int caretPos = caret.Position;
                int tokenStart = caretPos;

                while (tokenStart > lineStart)
                {
                    char ch = snapshot[tokenStart - 1];
                    if (!IsTokenChar(ch))
                        break;
                    tokenStart--;
                }

                int tokenLength = caretPos - tokenStart;
                if (tokenLength == 0)
                    return false;

                string token = snapshot.GetText(tokenStart, tokenLength);
                if (!SnippetStore.TryGet(token, out string expansion))
                    return false;

                string indentation = GetLineIndentation(line.GetText());
                expansion = PrepareExpansion(expansion, indentation, out int cursorOffset);

                using (ITextEdit edit = snapshot.TextBuffer.CreateEdit())
                {
                    if (!edit.Replace(new Span(tokenStart, tokenLength), expansion))
                        return false;
                    ITextSnapshot newSnapshot = edit.Apply();
                    if (edit.Canceled)
                        return false;

                    int newCaretPos = Math.Min(tokenStart + cursorOffset, newSnapshot.Length);
                    _view.Caret.MoveTo(new SnapshotPoint(newSnapshot, newCaretPos));
                    _view.Caret.EnsureVisible();
                }

                return true;
            }
            catch (Exception ex)
            {
                SnippetStore.LogError("Errore durante l'espansione dello snippet", ex);
                return false; // TAB normale in caso di qualsiasi problema.
            }
        }

        private static bool IsTokenChar(char ch)
        {
            return char.IsLetterOrDigit(ch) || ch == '_';
        }

        private static string GetLineIndentation(string lineText)
        {
            int count = 0;
            while (count < lineText.Length && (lineText[count] == ' ' || lineText[count] == '\t'))
                count++;
            return lineText.Substring(0, count);
        }

        private static string PrepareExpansion(string expansion, string indentation, out int cursorOffset)
        {
            const string CursorMarker = "$cursor$";
            string normalized = (expansion ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            if (normalized.IndexOf('\n') >= 0)
                normalized = normalized.Replace("\n", Environment.NewLine + indentation);

            int markerIndex = normalized.IndexOf(CursorMarker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                normalized = normalized.Remove(markerIndex, CursorMarker.Length);
                cursorOffset = markerIndex;
            }
            else
            {
                cursorOffset = normalized.Length;
            }

            return normalized;
        }
    }
}

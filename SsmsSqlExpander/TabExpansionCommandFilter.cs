using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SsmsSqlExpander
{
    internal sealed class TabExpansionCommandFilter : IOleCommandTarget
    {
        private readonly IWpfTextView _view;
        private IOleCommandTarget _next;
        private SnippetSession _session;

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
            return _next?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText)
                   ?? VSConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K &&
                nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
            {
                try
                {
                    if (_session != null && _session.TryHandleTab())
                        return VSConstants.S_OK;

                    _session = null;

                    if (TryExpandSnippet())
                        return VSConstants.S_OK;
                }
                catch (Exception ex)
                {
                    _session = null;
                    SnippetStore.LogError("Error while handling TAB", ex);
                }
            }

            return _next?.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
                   ?? VSConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        private bool TryExpandSnippet()
        {
            if (!_view.Selection.IsEmpty)
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
            expansion = PrepareExpansion(expansion, indentation);

            using (ITextEdit edit = snapshot.TextBuffer.CreateEdit())
            {
                edit.Replace(new Span(tokenStart, tokenLength), expansion);
                ITextSnapshot newSnapshot = edit.Apply();
                if (edit.Canceled)
                    return false;

                _session = SnippetSession.TryCreate(
                    _view,
                    newSnapshot,
                    tokenStart,
                    expansion.Length);

                if (_session == null)
                {
                    int cursorMarker = expansion.IndexOf("$cursor$", StringComparison.OrdinalIgnoreCase);
                    int zeroMarker = expansion.IndexOf("$0", StringComparison.Ordinal);
                    int markerIndex = cursorMarker >= 0 ? cursorMarker : zeroMarker;

                    if (markerIndex >= 0)
                    {
                        string marker = cursorMarker >= 0 ? "$cursor$" : "$0";
                        using (ITextEdit markerEdit = newSnapshot.TextBuffer.CreateEdit())
                        {
                            markerEdit.Delete(new Span(tokenStart + markerIndex, marker.Length));
                            ITextSnapshot finalSnapshot = markerEdit.Apply();
                            _view.Caret.MoveTo(new SnapshotPoint(finalSnapshot, tokenStart + markerIndex));
                        }
                    }
                    else
                    {
                        int newCaretPos = Math.Min(tokenStart + expansion.Length, newSnapshot.Length);
                        _view.Caret.MoveTo(new SnapshotPoint(newSnapshot, newCaretPos));
                    }

                    _view.Caret.EnsureVisible();
                }

                return true;
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

        private static string PrepareExpansion(string expansion, string indentation)
        {
            string normalized = (expansion ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            if (normalized.IndexOf('\n') >= 0)
                normalized = normalized.Replace("\n", Environment.NewLine + indentation);

            return normalized;
        }
    }
}

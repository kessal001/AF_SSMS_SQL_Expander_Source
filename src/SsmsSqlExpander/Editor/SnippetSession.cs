using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SsmsSqlExpander
{
    internal sealed class SnippetSession
    {
        private sealed class PlaceholderGroup
        {
            public int Number { get; set; }
            public ITrackingSpan Primary { get; set; }
            public List<ITrackingSpan> Repeats { get; } = new List<ITrackingSpan>();
            public string Marker => "$" + Number;
        }

        private static readonly Regex MarkerRegex = new Regex(
            @"\$(?<n>\d+)|\$cursor\$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly IWpfTextView _view;
        private readonly List<PlaceholderGroup> _groups;
        private readonly ITrackingSpan _finalMarker;
        private readonly ITrackingPoint _snippetEnd;
        private int _groupIndex;
        private bool _finished;

        private SnippetSession(
            IWpfTextView view,
            List<PlaceholderGroup> groups,
            ITrackingSpan finalMarker,
            ITrackingPoint snippetEnd)
        {
            _view = view;
            _groups = groups;
            _finalMarker = finalMarker;
            _snippetEnd = snippetEnd;
            _groupIndex = 0;
        }

        public static SnippetSession TryCreate(
            IWpfTextView view,
            ITextSnapshot snapshot,
            int insertionStart,
            int insertionLength)
        {
            if (view == null || snapshot == null || insertionLength <= 0)
                return null;

            string insertedText = snapshot.GetText(insertionStart, insertionLength);
            MatchCollection matches = MarkerRegex.Matches(insertedText);
            if (matches.Count == 0)
                return null;

            var byNumber = new SortedDictionary<int, List<ITrackingSpan>>();
            ITrackingSpan finalMarker = null;

            foreach (Match match in matches)
            {
                int absoluteStart = insertionStart + match.Index;
                ITrackingSpan tracking = snapshot.CreateTrackingSpan(
                    new Span(absoluteStart, match.Length),
                    SpanTrackingMode.EdgeInclusive);

                string raw = match.Value;
                if (raw.Equals("$cursor$", StringComparison.OrdinalIgnoreCase))
                {
                    if (finalMarker == null)
                        finalMarker = tracking;
                    continue;
                }

                if (!int.TryParse(match.Groups["n"].Value, out int number))
                    continue;

                if (number == 0)
                {
                    if (finalMarker == null)
                        finalMarker = tracking;
                    continue;
                }

                if (!byNumber.TryGetValue(number, out List<ITrackingSpan> spans))
                {
                    spans = new List<ITrackingSpan>();
                    byNumber[number] = spans;
                }

                spans.Add(tracking);
            }

            var groups = new List<PlaceholderGroup>();
            foreach (var pair in byNumber)
            {
                if (pair.Value.Count == 0)
                    continue;

                var group = new PlaceholderGroup
                {
                    Number = pair.Key,
                    Primary = pair.Value[0]
                };

                for (int i = 1; i < pair.Value.Count; i++)
                    group.Repeats.Add(pair.Value[i]);

                groups.Add(group);
            }

            ITrackingPoint end = snapshot.CreateTrackingPoint(
                Math.Min(insertionStart + insertionLength, snapshot.Length),
                PointTrackingMode.Positive);

            if (groups.Count == 0 && finalMarker == null)
                return null;

            var session = new SnippetSession(view, groups, finalMarker, end);
            session.MoveToCurrentOrFinish();
            return session;
        }

        public bool TryHandleTab()
        {
            if (_finished)
                return false;

            if (_groups.Count == 0)
            {
                Finish();
                return true;
            }

            if (_groupIndex < 0 || _groupIndex >= _groups.Count)
            {
                Finish();
                return true;
            }

            PlaceholderGroup current = _groups[_groupIndex];
            SnapshotSpan primarySpan = current.Primary.GetSpan(_view.TextSnapshot);

            if (!CaretOrSelectionBelongsTo(primarySpan))
            {
                _finished = true;
                return false;
            }

            ResolveCurrentGroup(current);
            _groupIndex++;
            MoveToCurrentOrFinish();
            return true;
        }

        private bool CaretOrSelectionBelongsTo(SnapshotSpan primarySpan)
        {
            if (!_view.Selection.IsEmpty)
            {
                SnapshotSpan selected = _view.Selection.StreamSelectionSpan.SnapshotSpan;
                return selected.IntersectsWith(primarySpan) || selected == primarySpan;
            }

            int caret = _view.Caret.Position.BufferPosition.Position;
            return caret >= primarySpan.Start.Position && caret <= primarySpan.End.Position;
        }

        private void ResolveCurrentGroup(PlaceholderGroup group)
        {
            ITextSnapshot snapshot = _view.TextSnapshot;
            SnapshotSpan primary = group.Primary.GetSpan(snapshot);
            string value = primary.GetText();

            if (value.Equals(group.Marker, StringComparison.OrdinalIgnoreCase))
                value = string.Empty;

            var replacements = new List<Tuple<SnapshotSpan, string>>();

            if (primary.GetText().Equals(group.Marker, StringComparison.OrdinalIgnoreCase))
                replacements.Add(Tuple.Create(primary, value));

            foreach (ITrackingSpan repeat in group.Repeats)
            {
                SnapshotSpan span = repeat.GetSpan(snapshot);
                replacements.Add(Tuple.Create(span, value));
            }

            if (replacements.Count == 0)
                return;

            using (ITextEdit edit = snapshot.TextBuffer.CreateEdit())
            {
                foreach (var replacement in replacements.OrderByDescending(x => x.Item1.Start.Position))
                    edit.Replace(replacement.Item1.Span, replacement.Item2);

                edit.Apply();
            }
        }

        private void MoveToCurrentOrFinish()
        {
            if (_groupIndex < _groups.Count)
            {
                SnapshotSpan span = _groups[_groupIndex].Primary.GetSpan(_view.TextSnapshot);
                _view.Selection.Select(span, false);
                _view.Caret.MoveTo(span.End);
                _view.Caret.EnsureVisible();
                return;
            }

            Finish();
        }

        private void Finish()
        {
            if (_finished)
                return;

            _finished = true;
            ITextSnapshot snapshot = _view.TextSnapshot;
            int destination;

            if (_finalMarker != null)
            {
                SnapshotSpan final = _finalMarker.GetSpan(snapshot);
                destination = final.Start.Position;

                using (ITextEdit edit = snapshot.TextBuffer.CreateEdit())
                {
                    edit.Delete(final.Span);
                    ITextSnapshot newSnapshot = edit.Apply();
                    destination = Math.Min(destination, newSnapshot.Length);
                    _view.Selection.Clear();
                    _view.Caret.MoveTo(new SnapshotPoint(newSnapshot, destination));
                }
            }
            else
            {
                SnapshotPoint end = _snippetEnd.GetPoint(snapshot);
                destination = end.Position;
                _view.Selection.Clear();
                _view.Caret.MoveTo(end);
            }

            _view.Caret.EnsureVisible();
        }
    }
}

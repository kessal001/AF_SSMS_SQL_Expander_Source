using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace AfSsmsSqlExpander
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("sql")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class SqlTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var wpfTextView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (wpfTextView == null)
                return;

            var filter = new TabExpansionCommandFilter(wpfTextView);
            int hr = textViewAdapter.AddCommandFilter(filter, out var next);
            if (hr == 0)
                filter.SetNext(next);
        }
    }
}

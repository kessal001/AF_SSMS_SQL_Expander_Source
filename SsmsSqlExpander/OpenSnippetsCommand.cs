using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace SsmsSqlExpander
{
    internal sealed class OpenSnippetsCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("F8109F3C-12B3-4694-A2CC-3B90E8AE705D");

        private OpenSnippetsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService))
                as OleMenuCommandService;

            if (commandService != null)
                new OpenSnippetsCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                SnippetStore.EnsureConfigExists();
                Process.Start(new ProcessStartInfo
                {
                    FileName = SnippetStore.ConfigPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                SnippetStore.LogError("Unable to open snippets.json.", ex);
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace SsmsSqlExpander
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("SSMS SQL Expander", "T-SQL abbreviation and snippet expansion", "0.3")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    public sealed class SqlExpanderPackage : AsyncPackage
    {
        public const string PackageGuidString = "7B0A2D38-450E-4E50-AF9B-9D944F36CBE2";

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await OpenSnippetsCommand.InitializeAsync(this);
        }
    }
}

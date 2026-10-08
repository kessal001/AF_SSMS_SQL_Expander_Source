[CmdletBinding(SupportsShouldProcess = $true)]
param([string]$InstallerPath)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'scripts\SsmsInstallation.ps1')
if (Get-Process ssms -ErrorAction SilentlyContinue) { throw 'Close every SSMS instance before uninstalling the extension.' }
[xml]$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'src\SsmsSqlExpander\source.extension.vsixmanifest') -Raw
$extensionId = $manifest.PackageManifest.Metadata.Identity.Id
$installer = Get-SsmsVsixInstaller -InstallerPath $InstallerPath
if ($PSCmdlet.ShouldProcess($installer, "Uninstall $extensionId")) {
    $process = Start-Process -FilePath $installer -ArgumentList "/uninstall:$extensionId" -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "VSIX uninstall failed with exit code $($process.ExitCode)." }
}

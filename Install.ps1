[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$InstallerPath,
    [string]$VsixPath = (Join-Path $PSScriptRoot 'artifacts\SsmsSqlExpander.vsix')
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'scripts\SsmsInstallation.ps1')
if (Get-Process ssms -ErrorAction SilentlyContinue) { throw 'Close every SSMS instance before installing the extension.' }
& (Join-Path $PSScriptRoot 'scripts\Test-Repository.ps1') -VsixPath $VsixPath
$installer = Get-SsmsVsixInstaller -InstallerPath $InstallerPath
$package = (Resolve-Path -LiteralPath $VsixPath).Path
if ($PSCmdlet.ShouldProcess($installer, "Install $package")) {
    $process = Start-Process -FilePath $installer -ArgumentList ('"{0}"' -f $package) -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "VSIX installation failed with exit code $($process.ExitCode)." }
}

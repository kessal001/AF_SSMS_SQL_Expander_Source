$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$releaseFolder = Join-Path $root 'SsmsSqlExpander\bin\Release'
$vsix = Get-ChildItem $releaseFolder -Filter *.vsix -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $vsix) {
    throw 'No VSIX found. Run .\Build.ps1 first.'
}

$installer = 'C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\VSIXInstaller.exe'
if (-not (Test-Path $installer)) {
    throw "SSMS 22 VSIXInstaller not found at: $installer"
}

if (Get-Process ssms -ErrorAction SilentlyContinue) {
    throw 'Close every SSMS instance before installing the extension.'
}

Write-Host "Installing $($vsix.FullName)"
& $installer $vsix.FullName

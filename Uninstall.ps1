$ErrorActionPreference = 'Stop'
$installer = 'C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\VSIXInstaller.exe'
$extensionId = 'AF.SsmsSqlExpander.0d7522b8-5317-4edf-839a-7d33c633f1f8'

if (-not (Test-Path $installer)) {
    throw "SSMS 22 VSIXInstaller not found at: $installer"
}

if (Get-Process ssms -ErrorAction SilentlyContinue) {
    throw 'Close every SSMS instance before uninstalling the extension.'
}

& $installer "/uninstall:$extensionId"

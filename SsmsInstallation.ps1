function Get-SsmsVsixInstaller {
    param([string]$InstallerPath)

    if ($InstallerPath) {
        if (-not (Test-Path -LiteralPath $InstallerPath -PathType Leaf)) {
            throw "VSIXInstaller non trovato: $InstallerPath"
        }
        return (Resolve-Path -LiteralPath $InstallerPath).Path
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $instances = & $vswhere -all -products Microsoft.VisualStudio.Product.Ssms -format json | ConvertFrom-Json
        $instances = $instances | Where-Object { [version]$_.installationVersion -ge [version]'21.6' } |
            Sort-Object { [version]$_.installationVersion } -Descending
        foreach ($instance in $instances) {
            $candidate = Join-Path $instance.installationPath 'Common7\IDE\VSIXInstaller.exe'
            if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
        }
    }

    throw 'SSMS 21.6 o successivo non trovato. Specifica -InstallerPath con il percorso del suo VSIXInstaller.exe.'
}

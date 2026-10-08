function Get-SsmsVsixInstaller {
    param([string]$InstallerPath)

    [xml]$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot '..\src\SsmsSqlExpander\source.extension.vsixmanifest') -Raw
    $targetRange = @($manifest.PackageManifest.Installation.InstallationTarget)[0].Version
    $minimumVersion = [version]($targetRange.TrimStart('[').Split(',')[0])

    if ($InstallerPath) {
        if (-not (Test-Path -LiteralPath $InstallerPath -PathType Leaf)) {
            throw "VSIXInstaller non trovato: $InstallerPath"
        }
        return (Resolve-Path -LiteralPath $InstallerPath).Path
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $instances = & $vswhere -all -products Microsoft.VisualStudio.Product.Ssms -format json | ConvertFrom-Json
        $instances = $instances | Where-Object { [version]$_.installationVersion -ge $minimumVersion } |
            Sort-Object { [version]$_.installationVersion } -Descending
        foreach ($instance in $instances) {
            $candidate = Join-Path $instance.installationPath 'Common7\IDE\VSIXInstaller.exe'
            if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
        }
    }

    throw "SSMS $minimumVersion or later not found. Specify -InstallerPath with its VSIXInstaller.exe path."
}

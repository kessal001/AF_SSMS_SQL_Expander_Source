[CmdletBinding()]
param([string]$VsixPath)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root 'src\SsmsSqlExpander\SsmsSqlExpander.csproj'
[xml]$project = Get-Content -LiteralPath $projectPath -Raw
[xml]$manifest = Get-Content -LiteralPath (Join-Path $root 'src\SsmsSqlExpander\source.extension.vsixmanifest') -Raw
$identity = $manifest.PackageManifest.Metadata.Identity
if ($project.Project.PropertyGroup.Version -ne $identity.Version) { throw 'Project and VSIX manifest versions differ.' }

$solution = Get-Content -LiteralPath (Join-Path $root 'SSMS_SQL_Expander.sln') -Raw
foreach ($match in [regex]::Matches($solution, '"([^"\r\n]+\.csproj)"')) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $match.Groups[1].Value))) { throw "Missing solution project: $($match.Groups[1].Value)" }
}
foreach ($content in $project.Project.ItemGroup.Content) {
    if ($content -and -not (Test-Path -LiteralPath (Join-Path (Split-Path $projectPath) $content.Include))) { throw "Missing project content: $($content.Include)" }
}
foreach ($example in Get-ChildItem -LiteralPath (Join-Path $root 'examples') -Filter '*.json') {
    $snippets = Get-Content -LiteralPath $example.FullName -Raw | ConvertFrom-Json
    foreach ($entry in $snippets.PSObject.Properties) {
        if ([string]::IsNullOrWhiteSpace($entry.Name) -or $entry.Value -isnot [string]) { throw "Invalid snippet in $($example.Name): $($entry.Name)" }
    }
}
foreach ($script in Get-ChildItem -LiteralPath $root -Recurse -Filter '*.ps1' | Where-Object { $_.FullName -notmatch '[\\/](archive|bin|obj|\.git)[\\/]' }) {
    $parseErrors = $null
    $tokens = $null
    $null = [System.Management.Automation.Language.Parser]::ParseFile($script.FullName, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors.Count) { throw "PowerShell syntax error in $($script.FullName): $parseErrors" }
}

if ($VsixPath) {
    $packagePath = (Resolve-Path -LiteralPath $VsixPath).Path
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        foreach ($required in @('extension.vsixmanifest', 'SsmsSqlExpander.dll', 'SsmsSqlExpander.pkgdef', 'LICENSE')) {
            if (-not $zip.GetEntry($required)) { throw "Missing VSIX entry: $required" }
        }
        $reader = [System.IO.StreamReader]::new($zip.GetEntry('extension.vsixmanifest').Open())
        try { [xml]$packagedManifest = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $packagedIdentity = $packagedManifest.PackageManifest.Metadata.Identity
        if ($packagedIdentity.Id -ne $identity.Id -or $packagedIdentity.Version -ne $identity.Version) { throw 'VSIX identity/version does not match the current source manifest.' }
    } finally { $zip.Dispose() }
    Write-Host "VSIX package checks passed: $packagePath"
}
Write-Host 'Repository checks passed.'

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$MsBuildPath
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$solution = Join-Path $root 'SSMS_SQL_Expander.sln'
if ($MsBuildPath) {
    if (-not (Test-Path -LiteralPath $MsBuildPath -PathType Leaf)) { throw "MSBuild not found: $MsBuildPath" }
    $msbuild = (Resolve-Path -LiteralPath $MsBuildPath).Path
} else {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) { throw 'Install Visual Studio or Build Tools with MSBuild.' }
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) {
        $msbuild = & $vswhere -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
    if (-not $msbuild) { throw 'MSBuild not found. Install the Visual Studio extension development workload.' }
}
& (Join-Path $root 'scripts\Test-Repository.ps1')
Write-Host "MSBuild: $msbuild"
& $msbuild $solution /restore /t:Rebuild "/p:Configuration=$Configuration" '/p:Platform=Any CPU' /m /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "MSBuild failed with exit code $LASTEXITCODE." }
$vsix = Join-Path $root "src\SsmsSqlExpander\bin\$Configuration\net48\SsmsSqlExpander.vsix"
& (Join-Path $root 'scripts\Test-Repository.ps1') -VsixPath $vsix
$artifactFolder = Join-Path $root 'artifacts'
New-Item -ItemType Directory -Path $artifactFolder -Force | Out-Null
$artifactPath = Join-Path $artifactFolder 'SsmsSqlExpander.vsix'
Copy-Item -LiteralPath $vsix -Destination $artifactPath -Force
Write-Host "VSIX ready: $artifactPath" -ForegroundColor Green

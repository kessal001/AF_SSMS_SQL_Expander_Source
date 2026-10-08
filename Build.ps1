$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root 'SSMS_SQL_Expander.sln'

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) {
    throw 'vswhere.exe not found. Install Visual Studio or Build Tools with MSBuild.'
}

$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) {
    throw 'MSBuild not found. Add the "Visual Studio extension development" workload in Visual Studio Installer.'
}

Write-Host "MSBuild: $msbuild"
& $msbuild $solution /restore /t:Rebuild /p:Configuration=Release /p:Platform='Any CPU' /m
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$vsix = Get-ChildItem (Join-Path $root 'SsmsSqlExpander\bin\Release') -Filter *.vsix -Recurse | Select-Object -First 1
if (-not $vsix) {
    throw 'Build completed but no VSIX was found under bin\Release.'
}

Write-Host ''
Write-Host 'VSIX created:' -ForegroundColor Green
Write-Host $vsix.FullName -ForegroundColor Green

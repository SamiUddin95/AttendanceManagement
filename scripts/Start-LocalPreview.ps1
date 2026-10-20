param([int]$Port = 5088)
$ErrorActionPreference = 'Stop'
if ($Port -lt 1024 -or $Port -gt 65535) { throw 'Choose a port between 1024 and 65535.' }
$projectRoot = Split-Path $PSScriptRoot -Parent
$preview = Join-Path $projectRoot ('artifacts/preview-' + [guid]::NewGuid().ToString('N'))
& dotnet publish (Join-Path $projectRoot 'src/AttendanceManagement.Web') -c Release -o $preview -v q
if ($LASTEXITCODE) { throw 'Publishing the preview failed.' }
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    Write-Host "Preview: http://127.0.0.1:$Port (Ctrl+C to stop). Uses the configured development database."
    # Run published copies, leaving project bin/Debug and bin/Release files unlocked.
    & dotnet (Join-Path $preview 'AttendanceManagement.Web.dll') --contentRoot $preview --urls "http://127.0.0.1:$Port"
} finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}

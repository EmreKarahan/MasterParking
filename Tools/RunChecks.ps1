# Runs the checks that are possible without a Unity Editor installation.
# Usage: powershell -ExecutionPolicy Bypass -File Tools/RunChecks.ps1
$ErrorActionPreference = "Stop"

$repo = Split-Path -Parent $PSScriptRoot
Push-Location $repo
try {
    Write-Host "== Runtime compile check =="
    dotnet build "Tools\UnityCompileCheck\MasterParking.UnityCompileCheck.csproj"
    if ($LASTEXITCODE -ne 0) { throw "Runtime compile check failed." }

    Write-Host "== Editor + EditMode test compile check =="
    dotnet build "Tools\EditorCompileCheck\MasterParking.EditorCompileCheck.csproj"
    if ($LASTEXITCODE -ne 0) { throw "Editor compile check failed." }

    Write-Host "== Core behaviour checks =="
    dotnet build "Tools\CoreTests\MasterParking.Core.Tests.csproj"
    if ($LASTEXITCODE -ne 0) { throw "Core checks failed to build." }

    & "Tools\CoreTests\bin\Debug\net10.0\MasterParking.CoreChecks.exe"
    if ($LASTEXITCODE -ne 0) { throw "Core behaviour checks failed." }

    Write-Host "All available checks passed."
}
finally {
    Pop-Location
}

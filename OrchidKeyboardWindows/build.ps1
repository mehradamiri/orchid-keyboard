# Orchid Keyboard Windows - Build Script
# Usage: .\build.ps1 [-Release]

param(
    [switch]$Release
)

$ErrorActionPreference = "Stop"
$ProjectDir = Join-Path $PSScriptRoot "OrchidKeyboard"

Write-Host "=== Orchid Keyboard Windows Build ===" -ForegroundColor Cyan

if ($Release) {
    Write-Host "Building Release (single-file, self-contained)..." -ForegroundColor Yellow
    dotnet publish $ProjectDir `
        -c Release `
        -r win-x64 `
        --self-contained `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=true `
        -o "$PSScriptRoot\publish"

    if ($LASTEXITCODE -eq 0) {
        $exe = Get-Item "$PSScriptRoot\publish\OrchidKeyboard.exe"
        Write-Host "`nBuild successful!" -ForegroundColor Green
        Write-Host "Output: $($exe.FullName)" -ForegroundColor Green
        Write-Host "Size: $([math]::Round($exe.Length / 1MB, 1)) MB" -ForegroundColor Green
    } else {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Building Debug..." -ForegroundColor Yellow
    dotnet build $ProjectDir -c Debug

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nBuild successful!" -ForegroundColor Green
    } else {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
}

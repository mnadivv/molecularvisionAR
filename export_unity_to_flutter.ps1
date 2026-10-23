# Script Otomatis Export Unity ke Flutter Embed (Compressed LZ4HC + Shader Stripped)
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   MVAR: Export Unity ke Flutter Embed (Super Ringan)   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Cari Unity 6 Editor
$unityPaths = @(
    "C:\Program Files\Unity\Hub\Editor\6000.5.3f1\Editor\Unity.exe",
    "C:\Program Files\Unity\Hub\Editor\6000.0.*\Editor\Unity.exe"
)

$unityExe = $null
foreach ($pathPattern in $unityPaths) {
    $found = Get-Item $pathPattern -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $unityExe = $found.FullName
        break
    }
}

if (-not $unityExe) {
    # Coba cari di folder Unity Hub
    $hubEditors = Get-ChildItem -Path "C:\Program Files\Unity\Hub\Editor" -Recurse -Filter "Unity.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($hubEditors) {
        $unityExe = $hubEditors.FullName
    }
}

if (-not $unityExe) {
    Write-Host "[ERROR] Unity.exe tidak ditemukan di C:\Program Files\Unity\Hub\Editor!" -ForegroundColor Red
    Write-Host "Silakan buka Unity Hub dan pastikan Unity 6 terpasang." -ForegroundColor Yellow
    exit 1
}

Write-Host "[OK] Unity ditemukan: $unityExe" -ForegroundColor Green

$projectPath = Join-Path $PSScriptRoot "unity\MVAR"
$outputPath = Join-Path $PSScriptRoot "android\unityLibrary"
$logFile = Join-Path $PSScriptRoot "unity_export.log"

Write-Host "[INFO] Project Path : $projectPath" -ForegroundColor White
Write-Host "[INFO] Output Path  : $outputPath" -ForegroundColor White
Write-Host "[INFO] Kompresi     : LZ4HC (High Compression)" -ForegroundColor Yellow
Write-Host "[INFO] Shader Strip : Aktif (Sentis stripped, Lightmaps/Fog stripped, GLES3 only)" -ForegroundColor Yellow
Write-Host "[INFO] IL2CPP Opt   : OptimizeSize + ManagedStrippingLevel.High" -ForegroundColor Yellow

# Pastikan folder target bersih atau siap
if (Test-Path $outputPath) {
    Write-Host "[INFO] Membersihkan folder unityLibrary lama..." -ForegroundColor Yellow
    Remove-Item -Path $outputPath -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "`n[START] Menjalankan build Unity batchmode (mohon tunggu)..." -ForegroundColor Cyan
$startTime = Get-Date

$proc = Start-Process -FilePath $unityExe -ArgumentList `
    "-projectPath `"$projectPath`"", `
    "-batchmode", `
    "-buildTarget Android", `
    "-executeMethod ProjectExporterBatchmode.ExportProjectAndroid", `
    "-exportPath `"$outputPath`"", `
    "-quit", `
    "-logFile `"$logFile`"" `
    -NoNewWindow -PassThru -Wait

$duration = (Get-Date) - $startTime
Write-Host "[SELESAI] Proses build selesai dalam $($duration.Minutes)m $($duration.Seconds)s." -ForegroundColor Green

if ($proc.ExitCode -ne 0) {
    Write-Host "[ERROR] Unity build gagal dengan exit code $($proc.ExitCode)!" -ForegroundColor Red
    Write-Host "Periksa log di: $logFile" -ForegroundColor Yellow
    if (Test-Path $logFile) {
        Get-Content $logFile -Tail 30
    }
    exit $proc.ExitCode
}

# Periksa hasil export
if (Test-Path (Join-Path $outputPath "build.gradle")) {
    Write-Host "`n[BERHASIL] unityLibrary berhasil diexport ke Flutter Embed!" -ForegroundColor Green
    
    # Ringkasan ukuran file penting
    $dataPath = Join-Path $outputPath "src\main\assets\bin\Data\data.unity3d"
    if (Test-Path $dataPath) {
        $dataSize = [math]::Round((Get-Item $dataPath).Length / 1MB, 2)
        Write-Host " - data.unity3d (LZ4HC compressed) : $dataSize MB" -ForegroundColor White
    }

    $soList = Get-ChildItem -Path (Join-Path $outputPath "src\main\jniLibs") -Recurse -Filter "*.so" -ErrorAction SilentlyContinue
    if ($soList) {
        Write-Host " - Native Libraries (.so):" -ForegroundColor White
        foreach ($so in $soList) {
            $soSize = [math]::Round($so.Length / 1MB, 2)
            Write-Host "     $($so.Name) : $soSize MB" -ForegroundColor Gray
        }
    }
    Write-Host "`nSekarang Anda dapat menjalankan: flutter run" -ForegroundColor Cyan
} else {
    Write-Host "[WARNING] File build.gradle tidak ditemukan di $outputPath. Periksa $logFile" -ForegroundColor Yellow
}

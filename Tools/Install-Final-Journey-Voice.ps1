$ErrorActionPreference = 'Stop'

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$Downloads = Join-Path $env:USERPROFILE 'Downloads'
$TargetDir = Join-Path $ProjectRoot 'Assets\LearningWithJourney\Resources\JourneyVoice\ABC'
$TargetFile = Join-Path $TargetDir '01.mp3'
$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'

$candidates = @(
    (Join-Path $Downloads 'Journey_ABC_01_FINAL_GAME.mp3'),
    (Join-Path $Downloads 'Journey_Inspired_Voice_FINAL_MASTER.mp3'),
    (Join-Path $Downloads 'Journey_Inspired_Voice_FINAL_MASTER (1).mp3')
)

$SourceFile = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $SourceFile) {
    Write-Host ''
    Write-Host 'FINAL JOURNEY VOICE NOT FOUND' -ForegroundColor Yellow
    Write-Host 'Download Journey_ABC_01_FINAL_GAME.mp3 from ChatGPT into your Downloads folder, then run this script again.'
    exit 1
}

New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
Copy-Item $SourceFile $TargetFile -Force
Write-Host "Installed final Journey voice to: $TargetFile" -ForegroundColor Green

if (-not (Test-Path $UnityExe)) {
    Write-Host "Unity was not found at $UnityExe. The audio file is installed, but run 'Learning with Journey > Apply Final Journey Voice to Book Reader' manually in Unity." -ForegroundColor Yellow
    exit 0
}

Write-Host 'Importing and connecting the final voice in Unity...'
& $UnityExe -batchmode -quit -projectPath $ProjectRoot -executeMethod LearningWithJourney.EditorTools.LWJInstallFinalJourneyVoiceV1.Apply -logFile '-'
if ($LASTEXITCODE -ne 0) {
    throw "Unity returned exit code $LASTEXITCODE while applying the final Journey voice."
}

Write-Host ''
Write-Host 'FINAL JOURNEY VOICE APPLIED TO THE GAME.' -ForegroundColor Green
Write-Host 'ABC page 1 now uses the mastered clip. Book Reader V2 loads it automatically from Resources, and the current V1 scene is also wired by the editor installer.'

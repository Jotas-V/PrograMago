param(
    [ValidateSet("EditMode", "PlayMode", "All")]
    [string]$Platform = "All"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectVersionPath = Join-Path $repoRoot "ProjectSettings\ProjectVersion.txt"
$versionLine = Get-Content -LiteralPath $projectVersionPath |
    Where-Object { $_ -match '^m_EditorVersion:\s*(.+)$' } |
    Select-Object -First 1

if (-not $versionLine) {
    throw "Não foi possível identificar a versão do Unity em $projectVersionPath."
}

$unityVersion = ($versionLine -replace '^m_EditorVersion:\s*', '').Trim()
$unityPath = Join-Path $env:ProgramFiles "Unity\Hub\Editor\$unityVersion\Editor\Unity.exe"

if (-not (Test-Path -LiteralPath $unityPath)) {
    throw "Unity $unityVersion não encontrado em $unityPath."
}

$resultDirectory = Join-Path $repoRoot "TestResults"
New-Item -ItemType Directory -Path $resultDirectory -Force | Out-Null

$platforms = if ($Platform -eq "All") {
    @("EditMode", "PlayMode")
} else {
    @($Platform)
}

foreach ($testPlatform in $platforms) {
    $resultPath = Join-Path $resultDirectory "$testPlatform.xml"
    $logPath = Join-Path $resultDirectory "$testPlatform.log"
    $unityArguments = @(
        "-batchmode",
        "-nographics",
        "-projectPath", $repoRoot,
        "-runTests",
        "-testPlatform", $testPlatform,
        "-testResults", $resultPath,
        "-logFile", $logPath
    )

    Write-Host "Executando testes $testPlatform no Unity $unityVersion..."
    $unityProcess = Start-Process `
        -FilePath $unityPath `
        -ArgumentList $unityArguments `
        -Wait `
        -PassThru `
        -WindowStyle Hidden

    if (-not (Test-Path -LiteralPath $resultPath)) {
        if (Test-Path -LiteralPath $logPath) {
            Get-Content -Tail 80 -LiteralPath $logPath | Write-Host
        }

        throw "Unity não gerou o resultado de $testPlatform. Feche o editor caso o projeto esteja aberto."
    }

    [xml]$results = Get-Content -Raw -LiteralPath $resultPath
    $testRun = $results.'test-run'
    Write-Host (
        "{0}: {1} aprovados, {2} falhos, {3} ignorados, {4} no total." -f
        $testPlatform,
        $testRun.passed,
        $testRun.failed,
        $testRun.skipped,
        $testRun.total)

    if ($unityProcess.ExitCode -ne 0 -or [int]$testRun.failed -gt 0) {
        exit 1
    }
}

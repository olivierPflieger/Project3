[CmdletBinding()]
param(
  [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$fixturesDirectory = Join-Path $repoRoot 'perf\fixtures'
$reportsRootDirectory = Join-Path $repoRoot 'perf\reports'
$campaignTimestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$campaignReportsDirectory = Join-Path $reportsRootDirectory "campaign-$campaignTimestamp"

function Ensure-FixtureFile {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Path,
    [Parameter(Mandatory = $true)]
    [long]$SizeBytes
  )

  if (Test-Path -LiteralPath $Path) {
    return
  }

  $parentDirectory = Split-Path -Parent $Path
  New-Item -ItemType Directory -Path $parentDirectory -Force | Out-Null

  $fileStream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
  try {
    $fileStream.SetLength($SizeBytes)
  }
  finally {
    $fileStream.Dispose()
  }
}

function Invoke-K6Run {
  param(
    [Parameter(Mandatory = $true)]
    [hashtable]$RunConfig
  )

  $reportPath = Join-Path $campaignReportsDirectory $RunConfig.ReportName
  $summaryPath = [System.IO.Path]::ChangeExtension($reportPath, '.summary.md')
  $summaryJsonPath = [System.IO.Path]::ChangeExtension($reportPath, '.summary.json')
  $displayTitle = $RunConfig.Name

  Write-Host ''
  Write-Host "=== $displayTitle ==="
  Write-Host "Report: $reportPath"

  $env:K6_VUS = ''
  $env:K6_DURATION = ''
  $env:K6_SCENARIO_VUS = [string]$RunConfig.Vus
  $env:K6_SCENARIO_DURATION = $RunConfig.Duration
  $env:K6_UPLOAD_FILE = $RunConfig.UploadFile
  $env:K6_REPORT_FILE = $reportPath
  $env:K6_SUMMARY_FILE = $summaryPath
  $env:K6_SUMMARY_JSON_FILE = $summaryJsonPath
  $env:K6_UPLOAD_RANDOM_RANGE = if ($RunConfig.ContainsKey('UploadRandomRange') -and $RunConfig.UploadRandomRange) { 'true' } else { 'false' }
  $env:K6_UPLOAD_MIN_MB = if ($RunConfig.ContainsKey('UploadMinMb')) { [string]$RunConfig.UploadMinMb } else { '' }
  $env:K6_UPLOAD_MAX_MB = if ($RunConfig.ContainsKey('UploadMaxMb')) { [string]$RunConfig.UploadMaxMb } else { '' }
  $env:K6_UPLOAD_SOURCE_MAX_MB = if ($RunConfig.ContainsKey('UploadSourceMaxMb')) { [string]$RunConfig.UploadSourceMaxMb } else { '' }
  $env:K6_UPLOAD_RESPONSE_P95 = if ($RunConfig.ContainsKey('UploadResponseP95')) { [string]$RunConfig.UploadResponseP95 } else { '' }
  $env:K6_DOWNLOAD_RESPONSE_P95 = if ($RunConfig.ContainsKey('DownloadResponseP95')) { [string]$RunConfig.DownloadResponseP95 } else { '' }

  if ($DryRun) {
    Write-Host "[DryRun] npm run perf:load"
    return $reportPath
  }

  & npm run perf:load

  if ($LASTEXITCODE -ne 0) {
    throw "K6 run failed for '$displayTitle' with exit code $LASTEXITCODE."
  }

  if (-not (Test-Path -LiteralPath $reportPath)) {
    throw "Expected report was not generated: $reportPath"
  }

  if (-not (Test-Path -LiteralPath $summaryPath)) {
    throw "Expected summary was not generated: $summaryPath"
  }

  return $reportPath
}

$smallFixture = Join-Path $fixturesDirectory 'upload-test.txt'
$mediumFixture = Join-Path $fixturesDirectory 'upload-test-5mb.bin'
$largeFixture = Join-Path $fixturesDirectory 'upload-test-10mb.bin'
$xlFixture = Join-Path $fixturesDirectory 'upload-test-50mb.bin'
$xxlFixture = Join-Path $fixturesDirectory 'upload-test-100mb.bin'

Ensure-FixtureFile -Path $mediumFixture -SizeBytes 5MB
Ensure-FixtureFile -Path $largeFixture -SizeBytes 10MB
Ensure-FixtureFile -Path $xlFixture -SizeBytes 50MB
Ensure-FixtureFile -Path $xxlFixture -SizeBytes 100MB

New-Item -ItemType Directory -Path $campaignReportsDirectory -Force | Out-Null

$runs = @(
  @{
    Name = 'Random upload size 1-100MiB x50'
    Vus = 20
    Duration = '3m'
    UploadFile = $xxlFixture
    UploadRandomRange = $true
    UploadMinMb = 1
    UploadMaxMb = 100
    UploadSourceMaxMb = 100
    UploadResponseP95 = 20000
    DownloadResponseP95 = 20000
    ReportName = 'k6-report.html'
  }
)

$originalEnv = @{
  K6_VUS = $env:K6_VUS
  K6_DURATION = $env:K6_DURATION
  K6_SCENARIO_VUS = $env:K6_SCENARIO_VUS
  K6_SCENARIO_DURATION = $env:K6_SCENARIO_DURATION
  K6_UPLOAD_FILE = $env:K6_UPLOAD_FILE
  K6_REPORT_FILE = $env:K6_REPORT_FILE
  K6_SUMMARY_FILE = $env:K6_SUMMARY_FILE
  K6_SUMMARY_JSON_FILE = $env:K6_SUMMARY_JSON_FILE
  K6_UPLOAD_RANDOM_RANGE = $env:K6_UPLOAD_RANDOM_RANGE
  K6_UPLOAD_MIN_MB = $env:K6_UPLOAD_MIN_MB
  K6_UPLOAD_MAX_MB = $env:K6_UPLOAD_MAX_MB
  K6_UPLOAD_SOURCE_MAX_MB = $env:K6_UPLOAD_SOURCE_MAX_MB
  K6_UPLOAD_RESPONSE_P95 = $env:K6_UPLOAD_RESPONSE_P95
  K6_DOWNLOAD_RESPONSE_P95 = $env:K6_DOWNLOAD_RESPONSE_P95
}

$generatedReports = @()

try {
  Push-Location $repoRoot

  foreach ($run in $runs) {
    $generatedReports += Invoke-K6Run -RunConfig $run
  }
}
finally {
  Pop-Location

  foreach ($envName in $originalEnv.Keys) {
    if ($null -eq $originalEnv[$envName]) {
      Remove-Item "Env:$envName" -ErrorAction SilentlyContinue
    }
    else {
      Set-Item "Env:$envName" -Value $originalEnv[$envName]
    }
  }
}

Write-Host ''
Write-Host 'Generated reports:'

foreach ($report in $generatedReports) {
  Write-Host "- $report"
}

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
  $displayTitle = $RunConfig.Name

  Write-Host ''
  Write-Host "=== $displayTitle ==="
  Write-Host "Report: $reportPath"

  $env:K6_VUS = [string]$RunConfig.Vus
  $env:K6_DURATION = $RunConfig.Duration
  $env:K6_UPLOAD_FILE = $RunConfig.UploadFile
  $env:K6_REPORT_FILE = $reportPath

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

  return $reportPath
}

$smallFixture = Join-Path $fixturesDirectory 'upload-test.txt'
$mediumFixture = Join-Path $fixturesDirectory 'upload-test-5mb.bin'
$largeFixture = Join-Path $fixturesDirectory 'upload-test-10mb.bin'

Ensure-FixtureFile -Path $mediumFixture -SizeBytes 5MB
Ensure-FixtureFile -Path $largeFixture -SizeBytes 10MB
New-Item -ItemType Directory -Path $campaignReportsDirectory -Force | Out-Null

$runs = @(
  @{
    Name = 'Baseline load'
    Vus = 5
    Duration = '1m'
    UploadFile = $smallFixture
    ReportName = '01-baseline-load.html'
  },
  @{
    Name = 'Concurrency x10'
    Vus = 10
    Duration = '1m'
    UploadFile = $smallFixture
    ReportName = '02-concurrency-10vu.html'
  },
  @{
    Name = 'Concurrency x20'
    Vus = 20
    Duration = '2m'
    UploadFile = $smallFixture
    ReportName = '03-concurrency-20vu.html'
  },
  @{
    Name = 'Medium file 5MB'
    Vus = 5
    Duration = '3m'
    UploadFile = $mediumFixture
    ReportName = '04-medium-file-5mb.html'
  }
)

$originalEnv = @{
  K6_VUS = $env:K6_VUS
  K6_DURATION = $env:K6_DURATION
  K6_UPLOAD_FILE = $env:K6_UPLOAD_FILE
  K6_REPORT_FILE = $env:K6_REPORT_FILE
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

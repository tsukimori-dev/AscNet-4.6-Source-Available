[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RuntimeDataRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [ValidateSet('win-x64', 'linux-x64')]
    [string]$RuntimeIdentifier = 'win-x64',

    [string]$DotNetPath = 'dotnet',

    [switch]$AcknowledgeNoRedistribution
)

$ErrorActionPreference = 'Stop'
if (-not $AcknowledgeNoRedistribution) {
    throw 'Pass -AcknowledgeNoRedistribution after reading RIGHTS-NOTICE.md and DEPLOYMENT.md.'
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path.TrimEnd('\', '/')
$runtimeRoot = (Resolve-Path -LiteralPath $RuntimeDataRoot).Path.TrimEnd('\', '/')
$output = [IO.Path]::GetFullPath($OutputDirectory).TrimEnd('\', '/')

if ($runtimeRoot.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Runtime data must remain outside the Git worktree.'
}
if ($output.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Deployment output must be outside the Git worktree.'
}
if (Test-Path -LiteralPath $output) {
    throw 'OutputDirectory must not already exist. Use a new path so publishing cannot overwrite an existing deployment.'
}

& (Join-Path $PSScriptRoot 'verify-source-snapshot.ps1')
$runtimeCheck = & (Join-Path $PSScriptRoot 'verify-runtime-data.ps1') -RuntimeDataRoot $runtimeRoot

$parent = Split-Path -Parent $output
if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
    [IO.Directory]::CreateDirectory($parent) | Out-Null
}

Push-Location $repoRoot
try {
    & $DotNetPath publish '.\AscNet\AscNet.csproj' `
        -c Release `
        -r $RuntimeIdentifier `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $output
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet publish failed.'
    }
}
finally {
    Pop-Location
}

$outputResources = Join-Path $output 'Resources'
[IO.Directory]::CreateDirectory($outputResources) | Out-Null
foreach ($directory in @('Configs', 'Data', 'table', 'table-4.6.0')) {
    Copy-Item -LiteralPath (Join-Path $runtimeRoot $directory) -Destination $outputResources -Recurse -Force
}

$outputVersionPath = Join-Path $outputResources 'Configs\version_config.json'
$allVersions = Get-Content -LiteralPath $outputVersionPath -Raw | ConvertFrom-Json
$version46 = $allVersions.PSObject.Properties['4.6.0'].Value
[ordered]@{ '4.6.0' = $version46 } |
    ConvertTo-Json -Depth 30 |
    Set-Content -LiteralPath $outputVersionPath -Encoding UTF8

$resourcesRootFull = [IO.Path]::GetFullPath($outputResources).TrimEnd('\', '/')
$noticesRoot = Join-Path $outputResources 'Configs\Notices'
if (Test-Path -LiteralPath $noticesRoot -PathType Container) {
    foreach ($directory in Get-ChildItem -LiteralPath $noticesRoot -Directory) {
        if ($directory.Name -eq '4.6.0') {
            continue
        }

        $legacyNoticeFull = [IO.Path]::GetFullPath($directory.FullName)
        if (-not $legacyNoticeFull.StartsWith($resourcesRootFull + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Legacy notice cleanup path escaped the generated Resources directory.'
        }
        [IO.Directory]::Delete($legacyNoticeFull, $true)
    }
}

$serverName = if ($RuntimeIdentifier -eq 'win-x64') { 'AscNet.exe' } else { 'AscNet' }
$serverPath = Join-Path $output $serverName
if (-not (Test-Path -LiteralPath $serverPath -PathType Leaf)) {
    throw "Published server executable is missing: $serverName"
}

$sourceCommitOutput = @(& git -C $repoRoot rev-parse --verify --quiet HEAD)
$sourceCommit = if ($sourceCommitOutput.Count -gt 0) { [string]$sourceCommitOutput[0] } else { '' }
$sourceCommit = $sourceCommit.Trim()
if ([string]::IsNullOrWhiteSpace($sourceCommit)) {
    $sourceCommit = 'uncommitted'
}
$manifest = [ordered]@{
    ClientVersion = '4.6.0'
    RuntimeIdentifier = $RuntimeIdentifier
    SourceCommit = $sourceCommit
    ServerSha256 = (Get-FileHash -LiteralPath $serverPath -Algorithm SHA256).Hash
    BaseTables = $runtimeCheck.BaseTables
    VersionedTables = $runtimeCheck.VersionedTables
    ResourceFiles = @(Get-ChildItem -LiteralPath $outputResources -File -Recurse).Count
    CreatedAtUtc = [DateTime]::UtcNow.ToString('o')
    RedistributionAuthorized = $false
}
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $output 'DEPLOYMENT-MANIFEST.json') -Encoding UTF8

@'
PRIVATE DEPLOYMENT OUTPUT

This directory contains operator-supplied runtime material and is not authorized for redistribution or resale.
Keep it private, keep all listeners local by default, and review RIGHTS-NOTICE.md in the source repository.
'@ | Set-Content -LiteralPath (Join-Path $output 'DO-NOT-REDISTRIBUTE.txt') -Encoding UTF8

Write-Host "Private 4.6 deployment published to: $output"
Write-Host "Server SHA-256: $($manifest.ServerSha256)"

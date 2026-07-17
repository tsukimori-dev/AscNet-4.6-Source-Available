[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RuntimeDataRoot
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $RuntimeDataRoot).Path.TrimEnd('\', '/')

$requiredFiles = @(
    'Configs\config.json',
    'Configs\version_config.json',
    'Configs\study_compatibility_4.6.0.json',
    'Configs\table_compatibility_4.6.0.json'
)
$requiredDirectories = @('Data', 'table', 'table-4.6.0')

foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf)) {
        throw "Required 4.6 runtime file is missing: $relative"
    }
}
foreach ($relative in $requiredDirectories) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Container)) {
        throw "Required 4.6 runtime directory is missing: $relative"
    }
}

$forbiddenExtensions = @('.exe', '.dll', '.pdb', '.dmp', '.pcap', '.db', '.sqlite', '.sqlite3', '.pem', '.key', '.p12', '.pfx', '.kdbx')
$forbiddenFiles = @(Get-ChildItem -LiteralPath $root -File -Recurse | Where-Object {
    $forbiddenExtensions -contains $_.Extension.ToLowerInvariant()
})
if ($forbiddenFiles.Count -gt 0) {
    throw "Runtime data input contains forbidden package/private artifacts: $($forbiddenFiles.FullName -join ', ')"
}

$config = Get-Content -LiteralPath (Join-Path $root 'Configs\config.json') -Raw | ConvertFrom-Json
if ($config.Client.ApplicationVersion -ne '4.6.0' -or $config.Client.StudyCompatibilityVersion -ne '4.6.0') {
    throw 'Configs/config.json must select ApplicationVersion=4.6.0 and StudyCompatibilityVersion=4.6.0.'
}

$versions = Get-Content -LiteralPath (Join-Path $root 'Configs\version_config.json') -Raw | ConvertFrom-Json
$versionProperty = $versions.PSObject.Properties['4.6.0']
if ($null -eq $versionProperty) {
    throw 'Configs/version_config.json does not contain a 4.6.0 entry.'
}
$version = $versionProperty.Value
if ($version.Ready -ne $true) {
    throw 'The 4.6.0 version response is not release-ready.'
}
if ($version.IndexSha1 -notmatch '^[0-9a-fA-F]{40}$' -or $version.LaunchIndexSha1 -notmatch '^[0-9a-fA-F]{40}$') {
    throw 'The 4.6.0 document/launch SHA-1 contract is invalid.'
}
if ([long]$version.IndexSize -le 0 -or [long]$version.LaunchIndexSize -le 0) {
    throw 'The 4.6.0 document/launch size contract is invalid.'
}

$studyPath = Join-Path $root 'Configs\study_compatibility_4.6.0.json'
$studyText = [IO.File]::ReadAllText($studyPath)
if ($studyText -notmatch '"ClientVersion"\s*:\s*"4\.6\.0"') {
    throw 'The Study compatibility fixture is not for client 4.6.0.'
}

$manifestPath = Join-Path $root 'Configs\table_compatibility_4.6.0.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.ClientVersion -ne '4.6.0' -or [int]$manifest.GeneratedFileCount -ne 47) {
    throw 'The versioned-table manifest is not the expected 47-file 4.6.0 contract.'
}
$entries = @($manifest.Files)
if ($entries.Count -ne [int]$manifest.GeneratedFileCount) {
    throw 'The versioned-table manifest file count is inconsistent.'
}

$versionedRoot = [IO.Path]::GetFullPath((Join-Path $root 'table-4.6.0')).TrimEnd('\', '/')
foreach ($entry in $entries) {
    $relative = [string]$entry.Table
    if ([string]::IsNullOrWhiteSpace($relative) -or [IO.Path]::IsPathRooted($relative) -or $relative -match '(^|[\\/])\.\.([\\/]|$)') {
        throw "Unsafe versioned-table path in manifest: $relative"
    }

    $path = [IO.Path]::GetFullPath((Join-Path $versionedRoot ($relative -replace '/', [IO.Path]::DirectorySeparatorChar)))
    if (-not $path.StartsWith($versionedRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Versioned-table path escapes its root: $relative"
    }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Versioned table is missing: $relative"
    }

    $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if (-not [string]::Equals($actual, [string]$entry.OutputSha256, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Versioned table hash mismatch: $relative"
    }
}

$baseTableCount = @(Get-ChildItem -LiteralPath (Join-Path $root 'table') -Filter '*.tsv' -File -Recurse).Count
if ($baseTableCount -lt 70) {
    throw "The base table set is incomplete: found $baseTableCount TSV files."
}

$result = [pscustomobject]@{
    ClientVersion = '4.6.0'
    BaseTables = $baseTableCount
    VersionedTables = $entries.Count
    RuntimeDataFiles = @(Get-ChildItem -LiteralPath $root -File -Recurse).Count
}
Write-Host "4.6 runtime data verification passed: $($result.BaseTables) base tables, $($result.VersionedTables) versioned tables."
$result

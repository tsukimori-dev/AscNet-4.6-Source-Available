[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

Push-Location $repoRoot
try {
    $gitRoot = (& git rev-parse --show-toplevel).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($gitRoot)) {
        throw 'The verifier must run inside a Git worktree.'
    }

    if (-not [string]::Equals(
        [IO.Path]::GetFullPath($gitRoot).TrimEnd('\', '/'),
        [IO.Path]::GetFullPath($repoRoot).TrimEnd('\', '/'),
        [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The verifier resolved a different Git worktree.'
    }

    $trackedFiles = @(& git ls-files)
    if ($LASTEXITCODE -ne 0 -or $trackedFiles.Count -eq 0) {
        throw 'Unable to enumerate the public source-available snapshot.'
    }

    $forbiddenPathPatterns = [ordered]@{
        LocalState = '(^|/)(\.idea|\.codegraph|docs/reports|proxylog|Packaging|Scripts|VersionEntrypoints)/'
        RuntimeResources = '^Resources/(Configs|Data|table(?:-[^/]+)?)/'
        LocalHelpers = '(^|/)(proxy\.py|run_steam\.py|launch-pgr-ascnet\.sh|AGENTS\.md)$'
        PacketCaptureUtility = '^AscNet\.PcapParser/'
        RetiredPrivatePublicationFiles = '(?i)(^|/)(private-safety\.yml|verify-private-source\.ps1|publish-private-runtime\.ps1)$'
        LicenseGrant = '(?i)(^|/)(LICENSE|COPYING)(\.[^/]*)?$'
        RuntimeArtifacts = '(?i)\.(exe|dll|pdb|dmp|zip|7z|tar|gz|pcap|db|sqlite|sqlite3|pem|key|p12|pfx|kdbx|msgpack|b64)$'
    }

    foreach ($rule in $forbiddenPathPatterns.GetEnumerator()) {
        $matches = @($trackedFiles | Where-Object { $_ -match $rule.Value })
        if ($matches.Count -gt 0) {
            throw "Forbidden tracked paths ($($rule.Key)): $($matches -join ', ')"
        }
    }

    $cacheFileMarker = 'KR' + 'SDK' + 'User' + 'Cache'
    $loginIdentityMarker = 'last' + '_login_' + 'cuid'
    $oldBrandName = ([char]0x5929).ToString() + ([char]0x673A) + ([char]0x9601)
    $oldBrandDomain = 'tian' + 'jg\.com'
    $forbiddenContentPatterns = [ordered]@{
        PrivateKey = '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----'
        GitHubToken = '\b(gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})\b'
        OpenAiKey = '\bsk-[A-Za-z0-9]{20,}\b'
        AwsAccessKey = '\b(AKIA|ASIA)[A-Z0-9]{16}\b'
        JsonWebToken = '\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\b'
        CredentialInUri = '(https?|mongodb(\+srv)?):\/\/[^\/\s:@]+:[^\/\s@]+@'
        UserProfilePath = '(?i)[A-Z]:[\\/]Users[\\/]'
        LocalGamePath = '(?i)[A-Z]:[\\/][^\r\n]{0,120}Punishing Gray Raven'
        RetiredVersion = '(?i)\b4\.5\.0\b'
        OldBrand = "(?i)($([regex]::Escape($oldBrandName))|$oldBrandDomain)"
        ThirdPartyProductionCdn = '(?i)kurogame\.(com|net)'
        LoginCacheFile = [regex]::Escape($cacheFileMarker)
        LoginIdentityKey = [regex]::Escape($loginIdentityMarker)
    }

    foreach ($path in $trackedFiles) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Tracked path is missing from the working tree: $path"
        }

        $fileInfo = Get-Item -LiteralPath $path
        if ($fileInfo.Length -gt 8MB) {
            throw "Tracked file exceeds the source-snapshot size limit: $path"
        }

        try {
            $content = [IO.File]::ReadAllText($fileInfo.FullName)
        }
        catch {
            throw "Tracked file is not readable as text: $path"
        }

        foreach ($rule in $forbiddenContentPatterns.GetEnumerator()) {
            if ($content -match $rule.Value) {
                throw "Forbidden content category '$($rule.Key)' found in $path"
            }
        }
    }

    $requiredFiles = @(
        'README.md',
        'DEPLOYMENT.md',
        'RIGHTS-NOTICE.md',
        'SECURITY.md',
        '.github/workflows/public-source-safety.yml',
        'AscNet.Common/ClientCompatibility.cs',
        'AscNet.GameServer/Game/DrawManager.Client46.cs',
        'tools/verify-source-snapshot.ps1',
        'tools/verify-runtime-data.ps1',
        'tools/publish-runtime.ps1'
    )
    foreach ($path in $requiredFiles) {
        if ($trackedFiles -notcontains $path) {
            throw "Required source-snapshot file is not tracked: $path"
        }
    }

    $readme = [IO.File]::ReadAllText((Resolve-Path -LiteralPath 'README.md'))
    if ($readme -notmatch '(?i)public, source-available' -or $readme -notmatch '(?i)not open-source software') {
        throw 'README.md must identify this as public source-available material that is not open-source software.'
    }
    $rightsNotice = [IO.File]::ReadAllText((Resolve-Path -LiteralPath 'RIGHTS-NOTICE.md'))
    if ($rightsNotice -notmatch '(?i)No open-source, redistribution, or commercial-use license is granted') {
        throw 'RIGHTS-NOTICE.md must retain the explicit no-license boundary.'
    }

    $schemaFiles = @($trackedFiles | Where-Object { $_ -match '^Schemas/table/.+\.tsv$' })
    if ($schemaFiles.Count -lt 70) {
        throw "Expected the compile-time schema set; found only $($schemaFiles.Count) TSV files."
    }

    foreach ($path in $schemaFiles) {
        $lines = @([IO.File]::ReadAllLines((Resolve-Path -LiteralPath $path)))
        if ($lines.Count -ne 3) {
            throw "Schema TSV must contain one header and two synthetic rows: $path"
        }

        foreach ($line in $lines | Select-Object -Skip 1) {
            foreach ($cell in $line.Split([char]9)) {
                if ($cell.Length -gt 0 -and $cell -ne '0' -and $cell -ne 'sample') {
                    throw "Non-synthetic schema value found in $path"
                }
            }
        }
    }

    $diffWhitespace = 'core.whitespace=-blank-at-eol,-blank-at-eof,-space-before-tab'
    & git -c $diffWhitespace diff --check
    if ($LASTEXITCODE -ne 0) {
        throw 'git diff --check failed.'
    }
    & git -c $diffWhitespace diff --cached --check
    if ($LASTEXITCODE -ne 0) {
        throw 'git diff --cached --check failed.'
    }

    & git rev-parse --verify --quiet HEAD | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $commitCount = [int]((& git rev-list --count HEAD).Trim())
        $headLine = (& git rev-list --parents -n 1 HEAD).Trim().Split(' ', [StringSplitOptions]::RemoveEmptyEntries)
        if ($commitCount -ne 1 -or $headLine.Count -ne 1) {
            throw 'The public source-available snapshot must remain a single root commit with no inherited history.'
        }
    }

    Write-Host "Public source-available verification passed: $($trackedFiles.Count) tracked text files, $($schemaFiles.Count) synthetic schemas."
}
finally {
    Pop-Location
}

[CmdletBinding()]
param(
    [string] $Version,
    [switch] $List,
    [switch] $Push
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$source = "https://api.nuget.org/v3/index.json"
$manifestPath = Join-Path $PSScriptRoot "database-comparison.release.json"
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))

function Invoke-DotNet {
    param(
        [Parameter(Mandatory)]
        [string[]] $Arguments,

        [Parameter(Mandatory)]
        [string] $FailureMessage
    )

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw $FailureMessage
    }
}

function Resolve-RepositoryPath {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    return [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path))
}

function Get-RegistryState {
    param(
        [Parameter(Mandatory)]
        [string] $PackageId,

        [Parameter(Mandatory)]
        [string] $PackageVersion
    )

    $normalizedId = $PackageId.ToLowerInvariant()
    $indexUrl = "https://api.nuget.org/v3-flatcontainer/$normalizedId/index.json"
    try {
        $index = Invoke-RestMethod -Method Get -Uri $indexUrl
        $versions = @($index.versions)
        return [pscustomobject]@{
            PackageId = $PackageId
            Latest = if ($versions.Count -eq 0) { "<none>" } else { $versions[-1] }
            ExactVersionExists = $versions -contains $PackageVersion.ToLowerInvariant()
        }
    }
    catch {
        $response = $_.Exception.Response
        $statusCode = if ($null -eq $response) { 0 } else { $response.StatusCode.value__ }
        if ($statusCode -eq 404) {
            return [pscustomobject]@{
                PackageId = $PackageId
                Latest = "<none>"
                ExactVersionExists = $false
            }
        }

        throw "NuGet.org preflight failed for $PackageId`: $($_.Exception.Message)"
    }
}

function Assert-VersionIsUnpublished {
    param(
        [Parameter(Mandatory)]
        $PackageFamily,

        [Parameter(Mandatory)]
        [string] $PackageVersion
    )

    $states = foreach ($package in @($PackageFamily.packages)) {
        Get-RegistryState -PackageId $package.id -PackageVersion $PackageVersion
    }

    $states | Format-Table PackageId, Latest, ExactVersionExists -AutoSize
    $published = @($states | Where-Object ExactVersionExists)
    if ($published.Count -gt 0) {
        throw "$PackageVersion is already immutable on NuGet.org for: $($published.PackageId -join ', ')"
    }
}

function Get-NuspecNode {
    param(
        [Parameter(Mandatory)]
        [xml] $Nuspec,

        [Parameter(Mandatory)]
        [string] $LocalName
    )

    return $Nuspec.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='$LocalName']")
}

function Assert-PackageArtifact {
    param(
        [Parameter(Mandatory)]
        $Package,

        [Parameter(Mandatory)]
        [string] $PackageVersion,

        [Parameter(Mandatory)]
        [string] $OutputFeed
    )

    $nupkgPath = Join-Path $OutputFeed "$($Package.id).$PackageVersion.nupkg"
    $snupkgPath = Join-Path $OutputFeed "$($Package.id).$PackageVersion.snupkg"
    if (-not (Test-Path -LiteralPath $nupkgPath -PathType Leaf)) {
        throw "Package not found: $nupkgPath"
    }
    if (-not (Test-Path -LiteralPath $snupkgPath -PathType Leaf)) {
        throw "Symbol package not found: $snupkgPath"
    }

    $archive = [IO.Compression.ZipFile]::OpenRead($nupkgPath)
    try {
        $nuspecEntry = $archive.Entries | Where-Object FullName -Like "*.nuspec" |
            Select-Object -First 1
        if ($null -eq $nuspecEntry) {
            throw "Nuspec not found: $nupkgPath"
        }

        $reader = [IO.StreamReader]::new($nuspecEntry.Open())
        try {
            [xml] $nuspec = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        $actualId = (Get-NuspecNode -Nuspec $nuspec -LocalName "id").InnerText
        $actualVersion = (Get-NuspecNode -Nuspec $nuspec -LocalName "version").InnerText
        $repository = Get-NuspecNode -Nuspec $nuspec -LocalName "repository"
        $readme = Get-NuspecNode -Nuspec $nuspec -LocalName "readme"
        if ($actualId -ne $Package.id -or $actualVersion -ne $PackageVersion) {
            throw "Nuspec id/version mismatch: $nupkgPath"
        }
        if ($null -eq $repository -or $repository.GetAttribute("type") -ne "git" -or
            [string]::IsNullOrWhiteSpace($repository.GetAttribute("url"))) {
            throw "Repository metadata missing: $nupkgPath"
        }
        if ($null -eq $readme -or [string]::IsNullOrWhiteSpace($readme.InnerText)) {
            throw "README metadata missing: $nupkgPath"
        }

        $readmeEntry = $archive.Entries | Where-Object {
            $_.FullName -eq $readme.InnerText
        } | Select-Object -First 1
        if ($null -eq $readmeEntry) {
            throw "README content missing: $nupkgPath"
        }

        $forbiddenEntry = $archive.Entries | Where-Object {
            $entryPath = $_.FullName.Replace('\', '/')
            $entryPath -match '(^|/)(host|test|tests)/' -or
                $entryPath -match '(^|/)appsettings(?:\.[^/]+)?\.json$'
        } | Select-Object -First 1
        if ($null -ne $forbiddenEntry) {
            throw "Forbidden content in $($Package.id): $($forbiddenEntry.FullName)"
        }

        foreach ($requiredDependency in @($Package.requiredDependencies)) {
            $dependency = $nuspec.SelectSingleNode(
                "//*[local-name()='dependency' and @id='$requiredDependency']")
            if ($null -eq $dependency -or $dependency.version -notlike "*$PackageVersion*") {
                throw "Dependency mismatch: $($Package.id) -> $requiredDependency $PackageVersion"
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}

function Invoke-ReleaseGate {
    param(
        [Parameter(Mandatory)]
        $PackageFamily,

        [Parameter(Mandatory)]
        [string] $PackageVersion,

        [Parameter(Mandatory)]
        [string] $OutputFeed
    )

    $solutionPath = Resolve-RepositoryPath -Path $PackageFamily.solution
    $nugetConfig = Resolve-RepositoryPath -Path "NuGet.Config"
    if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
        throw "Solution not found: $solutionPath"
    }
    if (-not (Test-Path -LiteralPath $nugetConfig -PathType Leaf)) {
        throw "NuGet.Config not found: $nugetConfig"
    }

    Write-Host "[$($PackageFamily.displayName)] restore"
    Invoke-DotNet -Arguments @(
        "restore", $solutionPath, "--configfile", $nugetConfig
    ) -FailureMessage "Restore failed."

    Write-Host "[$($PackageFamily.displayName)] Release build"
    Invoke-DotNet -Arguments @(
        "build", $solutionPath, "-c", "Release", "--no-restore", "-m:1",
        "-p:Version=$PackageVersion"
    ) -FailureMessage "Release build failed."

    Write-Host "[$($PackageFamily.displayName)] tests"
    Invoke-DotNet -Arguments @(
        "test", $solutionPath, "-c", "Release", "--no-build", "--no-restore", "-m:1",
        "-p:Version=$PackageVersion"
    ) -FailureMessage "Tests failed."

    foreach ($package in @($PackageFamily.packages)) {
        $projectPath = Resolve-RepositoryPath -Path $package.project
        if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
            throw "Package project not found: $projectPath"
        }

        foreach ($extension in @("nupkg", "snupkg")) {
            $artifactPath = Join-Path $OutputFeed "$($package.id).$PackageVersion.$extension"
            if (Test-Path -LiteralPath $artifactPath -PathType Leaf) {
                Remove-Item -LiteralPath $artifactPath -Force
            }
        }

        Write-Host "[$($PackageFamily.displayName)] pack $($package.id) $PackageVersion"
        Invoke-DotNet -Arguments @(
            "pack", $projectPath, "-c", "Release", "--no-build", "--no-restore", "-m:1",
            "-p:Version=$PackageVersion", "-o", $OutputFeed
        ) -FailureMessage "Pack failed: $($package.id)"
        Assert-PackageArtifact -Package $package -PackageVersion $PackageVersion -OutputFeed $OutputFeed
    }

    $packageCount = @(Get-ChildItem -LiteralPath $OutputFeed -Filter "*.$PackageVersion.nupkg").Count
    $symbolCount = @(Get-ChildItem -LiteralPath $OutputFeed -Filter "*.$PackageVersion.snupkg").Count
    $expectedCount = @($PackageFamily.packages).Count
    if ($packageCount -ne $expectedCount -or $symbolCount -ne $expectedCount) {
        throw "Artifact count mismatch: nupkg=$packageCount snupkg=$symbolCount expected=$expectedCount"
    }

    Write-Host "Gate complete: $packageCount packages and $symbolCount symbol packages verified."
    Write-Host "Local feed: $OutputFeed"
}

function Publish-PackageFamily {
    param(
        [Parameter(Mandatory)]
        $PackageFamily,

        [Parameter(Mandatory)]
        [string] $PackageVersion,

        [Parameter(Mandatory)]
        [string] $OutputFeed
    )

    Assert-VersionIsUnpublished -PackageFamily $PackageFamily -PackageVersion $PackageVersion
    Write-Host ""
    Write-Host "Family       : $($PackageFamily.displayName)"
    Write-Host "Key name     : $($PackageFamily.keyName)"
    Write-Host "NuGet glob   : $($PackageFamily.packageGlob)"
    Write-Host "Owner        : $($PackageFamily.owner)"
    Write-Host "Version      : $PackageVersion"
    Write-Host "Key scope    : Push only new package versions"

    $confirmation = Read-Host "NuGet.org publication is immutable. Type 'evet' to continue"
    if ($confirmation.Trim().ToLowerInvariant() -ne "evet") {
        throw "Publication cancelled."
    }

    $secureKey = Read-Host "$($PackageFamily.displayName) [$($PackageFamily.packageGlob)] PUSH API key" -AsSecureString
    if ($secureKey.Length -eq 0) {
        throw "API key cannot be empty."
    }

    $keyPointer = [IntPtr]::Zero
    $apiKey = $null
    try {
        $keyPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureKey)
        $apiKey = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($keyPointer)
        foreach ($package in @($PackageFamily.packages)) {
            $nupkgPath = Join-Path $OutputFeed "$($package.id).$PackageVersion.nupkg"
            Write-Host "Publishing $($package.id) $PackageVersion"
            Invoke-DotNet -Arguments @(
                "nuget", "push", $nupkgPath,
                "--source", $source,
                "--api-key", $apiKey,
                "--symbol-source", $source,
                "--symbol-api-key", $apiKey
            ) -FailureMessage "Push failed: $($package.id). Stop and inspect partial publication state."
        }
    }
    finally {
        if ($keyPointer -ne [IntPtr]::Zero) {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($keyPointer)
        }
        $apiKey = $null
        if ($null -ne $secureKey) {
            $secureKey.Dispose()
        }
        $secureKey = $null
    }
}

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Release manifest not found: $manifestPath"
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.schemaVersion -ne 1 -or @($manifest.families).Count -ne 1) {
    throw "Unsupported release manifest."
}

$family = @($manifest.families)[0]
$packageVersion = if ([string]::IsNullOrWhiteSpace($Version)) {
    [string] $family.version
}
else {
    $Version
}

if ($packageVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "Invalid SemVer: $packageVersion"
}
if (@($family.immutableVersions) -contains $packageVersion) {
    throw "$packageVersion is recorded as immutable; choose a new version."
}

if ($List) {
    [pscustomobject]@{
        Family = $family.displayName
        Version = $packageVersion
        PackageGlob = $family.packageGlob
        Packages = @($family.packages).Count
        Push = [bool] $Push
    } | Format-List
    exit 0
}

$outputFeed = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot $manifest.feed))
$repositoryPrefix = $repositoryRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
if (-not $outputFeed.StartsWith($repositoryPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Release feed must remain inside repository: $outputFeed"
}

New-Item -ItemType Directory -Path $outputFeed -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem

Assert-VersionIsUnpublished -PackageFamily $family -PackageVersion $packageVersion
Invoke-ReleaseGate -PackageFamily $family -PackageVersion $packageVersion -OutputFeed $outputFeed

if (-not $Push) {
    Write-Host "NuGet.org push was not requested. Re-run with -Push after reviewing this gate."
    exit 0
}

Publish-PackageFamily -PackageFamily $family -PackageVersion $packageVersion -OutputFeed $outputFeed
Write-Host "NuGet.org publication completed for all selected packages."

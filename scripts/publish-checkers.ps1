[CmdletBinding()]
param(
    [ValidateSet("All", "ApiContracts", "DatabaseComparison")]
    [string] $Family = "All",

    [ValidatePattern('^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$')]
    [string] $Version = "0.2.0-alpha.1",

    [string] $Feed,

    [string] $Source = "https://api.nuget.org/v3/index.json",

    [switch] $Push,

    [switch] $ResumePartial
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$workspaceRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($Feed)) {
    $Feed = Join-Path $workspaceRoot "artifacts\local-feed"
}

$feedPath = [IO.Path]::GetFullPath($Feed)
$immutableVersions = @(
    "0.1.0-alpha.5",
    "0.2.0-alpha.1",
    "0.2.0-alpha.2",
    "0.2.0-alpha.4",
    "0.2.0-alpha.5",
    "0.2.0-alpha.6",
    "0.2.0-alpha.7",
    "0.2.0-alpha.8"
)

# Herhangi bir PowerShell dizininden guvenli cagri:
# powershell -NoProfile -ExecutionPolicy Bypass `
#   -File "C:\Users\mertb\RiderProjects\ptn-assurance-platform\scripts\publish-checkers.ps1" `
#   -Version <YENI_SURUM>
# Ilk kosu -Push olmadan yapilir. Relative -File yolu yalniz repository kokunde kullanilir.

if ($immutableVersions -contains $Version) {
    throw "$Version NuGet.org'da immutable yayindir; ayni surum tekrar paketlenemez."
}

$families = @(
    [pscustomobject]@{
        Selector = "ApiContracts"
        DisplayName = "API Contracts"
        Glob = "CheckNexus.ApiContracts*"
        Root = Join-Path $workspaceRoot "checkers\api-contract"
        Solution = "Ptn.ApiContractChecker.slnx"
        Packages = @(
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts.Domain.Shared"; Project = "src\Ptn.ApiContractChecker.Domain.Shared\Ptn.ApiContractChecker.Domain.Shared.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts.Domain"; Project = "src\Ptn.ApiContractChecker.Domain\Ptn.ApiContractChecker.Domain.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts.Application.Contracts"; Project = "src\Ptn.ApiContractChecker.Application.Contracts\Ptn.ApiContractChecker.Application.Contracts.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts.Application"; Project = "src\Ptn.ApiContractChecker.Application\Ptn.ApiContractChecker.Application.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts.EntityFrameworkCore"; Project = "src\Ptn.ApiContractChecker.EntityFrameworkCore\Ptn.ApiContractChecker.EntityFrameworkCore.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts.HttpApi"; Project = "src\Ptn.ApiContractChecker.HttpApi\Ptn.ApiContractChecker.HttpApi.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts.HttpApi.Client"; Project = "src\Ptn.ApiContractChecker.HttpApi.Client\Ptn.ApiContractChecker.HttpApi.Client.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.ApiContracts"; Project = "src\CheckNexus.ApiContracts\CheckNexus.ApiContracts.csproj" }
        )
    }
    [pscustomobject]@{
        Selector = "DatabaseComparison"
        DisplayName = "Database Comparison"
        Glob = "CheckNexus.DatabaseComparison*"
        Root = Join-Path $workspaceRoot "checkers\database-comparison"
        Solution = "Ptn.DatabaseChecker.slnx"
        Packages = @(
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison.Domain.Shared"; Project = "src\Ptn.DatabaseChecker.Domain.Shared\Ptn.DatabaseChecker.Domain.Shared.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison.Domain"; Project = "src\Ptn.DatabaseChecker.Domain\Ptn.DatabaseChecker.Domain.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison.Application.Contracts"; Project = "src\Ptn.DatabaseChecker.Application.Contracts\Ptn.DatabaseChecker.Application.Contracts.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison.Application"; Project = "src\Ptn.DatabaseChecker.Application\Ptn.DatabaseChecker.Application.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison.EntityFrameworkCore"; Project = "src\Ptn.DatabaseChecker.EntityFrameworkCore\Ptn.DatabaseChecker.EntityFrameworkCore.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison.HttpApi"; Project = "src\Ptn.DatabaseChecker.HttpApi\Ptn.DatabaseChecker.HttpApi.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison.HttpApi.Client"; Project = "src\Ptn.DatabaseChecker.HttpApi.Client\Ptn.DatabaseChecker.HttpApi.Client.csproj" }
            [pscustomobject]@{ Id = "CheckNexus.DatabaseComparison"; Project = "src\CheckNexus.DatabaseComparison\CheckNexus.DatabaseComparison.csproj" }
        )
    }
)

if ($Family -ne "All") {
    $families = @($families | Where-Object Selector -eq $Family)
}

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

function Invoke-FamilyGate {
    param([Parameter(Mandatory)] $PackageFamily)

    Write-Host "[$($PackageFamily.DisplayName)] restore"
    Invoke-DotNet `
        -Arguments @("restore", $PackageFamily.Solution, "--source", $Source) `
        -FailureMessage "$($PackageFamily.DisplayName) restore basarisiz."

    Write-Host "[$($PackageFamily.DisplayName)] Release build"
    Invoke-DotNet `
        -Arguments @("build", $PackageFamily.Solution, "-c", "Release", "--no-restore", "-p:Version=$Version") `
        -FailureMessage "$($PackageFamily.DisplayName) build basarisiz."

    Write-Host "[$($PackageFamily.DisplayName)] test"
    Invoke-DotNet `
        -Arguments @("test", $PackageFamily.Solution, "-c", "Release", "--no-build", "--no-restore", "-p:Version=$Version") `
        -FailureMessage "$($PackageFamily.DisplayName) test basarisiz."

    foreach ($package in $PackageFamily.Packages) {
        $nupkgPath = Join-Path $feedPath "$($package.Id).$Version.nupkg"
        $snupkgPath = Join-Path $feedPath "$($package.Id).$Version.snupkg"

        foreach ($packagePath in @($nupkgPath, $snupkgPath)) {
            if (Test-Path -LiteralPath $packagePath) {
                Remove-Item -LiteralPath $packagePath -Force
            }
        }

        Write-Host "[$($PackageFamily.DisplayName)] pack $($package.Id) $Version"
        Invoke-DotNet `
            -Arguments @("pack", $package.Project, "-c", "Release", "--no-build", "--no-restore", "-p:Version=$Version", "-o", $feedPath) `
            -FailureMessage "Pack basarisiz: $($package.Id)"
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

function Test-PublishedVersion {
    param(
        [Parameter(Mandatory)]
        [string] $PackageId
    )

    $normalizedId = $PackageId.ToLowerInvariant()
    $indexUrl = "https://api.nuget.org/v3-flatcontainer/$normalizedId/index.json"

    try {
        $index = Invoke-RestMethod -Method Get -Uri $indexUrl
        return @($index.versions) -contains $Version.ToLowerInvariant()
    }
    catch {
        $response = $_.Exception.Response
        $statusCode = if ($null -ne $response) { $response.StatusCode.value__ } else { 0 }
        if ($statusCode -eq 404) {
            return $false
        }

        throw "NuGet.org preflight basarisiz: $PackageId - $($_.Exception.Message)"
    }
}

function Assert-Package {
    param([Parameter(Mandatory)] $Package)

    $nupkgPath = Join-Path $feedPath "$($Package.Id).$Version.nupkg"
    $snupkgPath = Join-Path $feedPath "$($Package.Id).$Version.snupkg"

    foreach ($packagePath in @($nupkgPath, $snupkgPath)) {
        if (-not (Test-Path -LiteralPath $packagePath)) {
            throw "Paket bulunamadi: $packagePath"
        }
    }

    $archive = [IO.Compression.ZipFile]::OpenRead($nupkgPath)
    try {
        $nuspecEntry = $archive.Entries | Where-Object FullName -Like "*.nuspec" | Select-Object -First 1
        if ($null -eq $nuspecEntry) {
            throw "Nuspec bulunamadi: $nupkgPath"
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

        if ($actualId -ne $Package.Id -or $actualVersion -ne $Version) {
            throw "Paket kimligi/surumu uyusmuyor: $nupkgPath"
        }

        if ($null -eq $repository -or $repository.type -ne "git" -or [string]::IsNullOrWhiteSpace($repository.url)) {
            throw "Repository metadata eksik: $nupkgPath"
        }

        if ($null -eq $readme -or $readme.InnerText -ne "README.md") {
            throw "Paket README metadata eksik: $nupkgPath"
        }

        $forbiddenEntry = $archive.Entries | Where-Object {
            $entryPath = $_.FullName.Replace('\', '/')
            $entryPath -match '(^|/)(host|test|tests)/' -or $entryPath -match '(^|/)appsettings(?:\.[^/]+)?\.json$'
        } | Select-Object -First 1

        if ($null -ne $forbiddenEntry) {
            throw "Host/test/config icerigi pakete sizdi: $($Package.Id) -> $($forbiddenEntry.FullName)"
        }

        if ($Package.Id -eq "CheckNexus.ApiContracts.Domain") {
            $dependency = $nuspec.SelectSingleNode("//*[local-name()='dependency' and @id='NJsonSchema']")
            if ($null -eq $dependency -or $dependency.version -notmatch '11\.6\.1') {
                throw "NJsonSchema 11.6.1 transitive dependency metadata eksik: $nupkgPath"
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}

function Publish-Family {
    param([Parameter(Mandatory)] $PackageFamily)

    $publishedIds = @(
        $PackageFamily.Packages | Where-Object { Test-PublishedVersion -PackageId $_.Id } |
            ForEach-Object Id
    )
    if ($publishedIds.Count -gt 0 -and -not $ResumePartial) {
        throw "Registry'de $Version zaten var: $($publishedIds -join ', '). Yeni surum secin; yarim kalmis ayni-byte yayin icin acikca -ResumePartial kullanin."
    }

    Write-Host ""
    Write-Host "Yayin ailesi : $($PackageFamily.DisplayName)"
    Write-Host "NuGet glob    : $($PackageFamily.Glob)"
    Write-Host "Owner         : mertbyd"
    Write-Host "Anahtar scope : Push only new package versions"
    Write-Host "Paket sayisi  : $($PackageFamily.Packages.Count) nupkg + $($PackageFamily.Packages.Count) snupkg"

    $confirmation = Read-Host "$($PackageFamily.DisplayName) $Version nuget.org'a kalici olarak yayinlanacak. Devam icin 'evet' yazin"
    if ($confirmation.Trim().ToLowerInvariant() -ne "evet") {
        throw "$($PackageFamily.DisplayName) yayini kullanici tarafindan iptal edildi."
    }

    $secureKey = Read-Host "$($PackageFamily.DisplayName) [$($PackageFamily.Glob)] icin nuget.org PUSH API key" -AsSecureString
    if ($secureKey.Length -eq 0) {
        throw "$($PackageFamily.DisplayName) API key bos olamaz."
    }

    $keyPointer = [IntPtr]::Zero
    $apiKey = $null

    try {
        $keyPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureKey)
        $apiKey = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($keyPointer)

        foreach ($package in $PackageFamily.Packages) {
            $packagePath = Join-Path $feedPath "$($package.Id).$Version.nupkg"
            Write-Host "Publishing $($package.Id) $Version (nupkg + sibling snupkg)"
            $pushArguments = @(
                "nuget", "push", $packagePath,
                "--source", $Source,
                "--api-key", $apiKey,
                "--symbol-source", $Source,
                "--symbol-api-key", $apiKey
            )
            if ($ResumePartial) {
                $pushArguments += "--skip-duplicate"
            }

            Invoke-DotNet `
                -Arguments $pushArguments `
                -FailureMessage "Push basarisiz: $($package.Id)"
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

Add-Type -AssemblyName System.IO.Compression.FileSystem
New-Item -ItemType Directory -Path $feedPath -Force | Out-Null

foreach ($packageFamily in $families) {
    Push-Location $packageFamily.Root
    try {
        Invoke-FamilyGate -PackageFamily $packageFamily
    }
    finally {
        Pop-Location
    }
}

foreach ($packageFamily in $families) {
    foreach ($package in $packageFamily.Packages) {
        Assert-Package -Package $package
    }
}

$packageCount = ($families | ForEach-Object { $_.Packages.Count } | Measure-Object -Sum).Sum
Write-Host "Gate tamam: $packageCount nupkg + $packageCount snupkg dogrulandi."
Write-Host "Local feed: $feedPath"

if (-not $Push) {
    Write-Host "NuGet.org push yapilmadi. Yayin icin scripti -Push ile yeniden calistirin."
    exit 0
}

foreach ($packageFamily in $families) {
    Publish-Family -PackageFamily $packageFamily
}

Write-Host "Secilen paket ailelerinin nuget.org push islemi tamamlandi."

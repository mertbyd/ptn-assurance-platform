<#
.SYNOPSIS
Starts the local persistent Vault and wires its limited application token into Test Module user-secrets.

.DESCRIPTION
The script never stores the unseal key or bootstrap root token. On first initialization it prints the
unseal key once; save it in a personal password manager. Only synthetic/local credentials may be used.

.PARAMETER UnsealKey
Required after a restart when the existing local Vault volume is sealed.

.PARAMETER TestModulePath
Path to the Test Orchestration repository whose Web user-secrets receive the limited local token.
#>
[CmdletBinding()]
param(
    [string]$UnsealKey,
    [string]$TestModulePath = (Join-Path $PSScriptRoot '..\..\ptn-test-orchestration-platform')
)

$ErrorActionPreference = 'Stop'
$composeFile = Join-Path $PSScriptRoot 'docker-compose.local.yml'
$webProject = Join-Path $TestModulePath 'src\Ptn.TestOrchestration.Web\Ptn.TestOrchestration.Web.csproj'
$localVaultAddress = 'http://127.0.0.1:8200'

if (-not (Test-Path $webProject)) {
    throw "Test Module Web project was not found: $webProject"
}

# islevi: Vault CLI komutunu container icinde calistirip stdout'u dondurur.
# sistemdeki gorevi: Sealed status exit code'u ve bootstrap token injection ayrintisini tek sinirda tutar.
function Invoke-LocalVault {
    param([string[]]$Arguments, [string]$Token)

    $command = @(
        'compose', '-f', $composeFile, 'exec', '-T',
        '-e', "VAULT_ADDR=$localVaultAddress"
    )
    if ($Token) {
        $command += @('-e', "VAULT_TOKEN=$Token")
    }
    $command += @('vault', 'vault') + $Arguments

    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        return (& docker @command 2>&1 | Out-String)
    }
    finally {
        $ErrorActionPreference = $previousPreference
    }
}

docker compose -f $composeFile up -d
if ($LASTEXITCODE -ne 0) {
    throw 'Local Vault container could not be started.'
}

$status = $null
foreach ($attempt in 1..30) {
    $statusOutput = Invoke-LocalVault -Arguments @('status', '-format=json')
    if ($statusOutput -match '"initialized"') {
        $status = $statusOutput | ConvertFrom-Json
        break
    }

    Start-Sleep -Seconds 1
}

if ($null -eq $status) {
    throw 'Local Vault did not become ready. Inspect docker compose logs.'
}

$rootToken = $null
if (-not $status.initialized) {
    $init = (Invoke-LocalVault -Arguments @(
        'operator', 'init', '-key-shares=1', '-key-threshold=1', '-format=json'
    )) | ConvertFrom-Json
    $UnsealKey = $init.unseal_keys_b64[0]
    $rootToken = $init.root_token

    Write-Host ''
    Write-Host 'LOCAL VAULT UNSEAL KEY (save once in your password manager):' -ForegroundColor Yellow
    Write-Host $UnsealKey -ForegroundColor Yellow
    Write-Host 'The bootstrap root token is intentionally not printed and will be revoked.' -ForegroundColor Green
}

if ($status.sealed -or -not $status.initialized) {
    if ([string]::IsNullOrWhiteSpace($UnsealKey)) {
        throw 'Vault is sealed. Run again with -UnsealKey from your password manager.'
    }

    Invoke-LocalVault -Arguments @('operator', 'unseal', $UnsealKey) | Out-Null
}

if ([string]::IsNullOrWhiteSpace($rootToken)) {
    Write-Host 'Vault is unsealed. Existing Test Module user-secret token is preserved.' -ForegroundColor Green
    return
}

Invoke-LocalVault -Arguments @('secrets', 'enable', '-path=pintern-dev', '-version=2', 'kv') -Token $rootToken | Out-Null
Invoke-LocalVault -Arguments @(
    'policy', 'write', 'pintern-quality-platform-local', '/vault/policies/quality-platform-local.hcl'
) -Token $rootToken | Out-Null

$applicationToken = (Invoke-LocalVault -Arguments @(
    'token', 'create',
    '-policy=pintern-quality-platform-local',
    '-no-default-policy',
    '-period=720h',
    '-orphan',
    '-field=token'
) -Token $rootToken).Trim()

if ([string]::IsNullOrWhiteSpace($applicationToken)) {
    throw 'The limited local Vault application token could not be created.'
}

dotnet user-secrets set 'Vault:Address' $localVaultAddress --project $webProject | Out-Null
dotnet user-secrets set 'Vault:Mount' 'pintern-dev' --project $webProject | Out-Null
dotnet user-secrets set 'Vault:AuthenticationMode' 'Token' --project $webProject | Out-Null
dotnet user-secrets set 'Vault:Token' $applicationToken --project $webProject | Out-Null

Invoke-LocalVault -Arguments @('token', 'revoke', '-self') -Token $rootToken | Out-Null

Write-Host 'Local Vault is ready and the limited token was written to Test Module user-secrets.' -ForegroundColor Green
Write-Host 'Do not place production customer credentials in this local Vault.' -ForegroundColor Yellow

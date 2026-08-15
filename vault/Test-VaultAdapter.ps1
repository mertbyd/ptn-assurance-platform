[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$pinternVaultContainer = 'pintern-vault-adapter-smoke'
$pinternVaultLabel = 'pintern.task=vault-adapter-smoke'
$pinternVaultRootToken = 'pintern-smoke-bootstrap'
$pinternVaultAddress = 'http://127.0.0.1:18200'

# islevi: Sinirli smoke token'inin yasak bir Vault endpointine erisemedigini dogrular.
# sistemdeki gorevi: Default policy, sys yetkisi veya genis KV wildcard'i kazara eklendiginde smoke'u fail-closed durdurur.
function Assert-CheckNexusVaultAccessDenied {
    param([string]$Uri, [hashtable]$Headers)

    try {
        Invoke-WebRequest `
            -Method Get `
            -Uri $Uri `
            -Headers $Headers `
            -UseBasicParsing `
            -ErrorAction Stop | Out-Null
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 403) {
            return
        }

        throw 'Limited Vault token denial check failed with an unexpected status.'
    }

    throw 'Limited Vault token unexpectedly accessed a forbidden endpoint.'
}

$pinternExisting = docker ps -a --filter "name=^/$pinternVaultContainer$" --format '{{.Names}}'
if ($pinternExisting) {
    throw "Smoke container already exists: $pinternVaultContainer"
}

try {
    docker run `
        --name $pinternVaultContainer `
        --label $pinternVaultLabel `
        --cap-add=IPC_LOCK `
        -e VAULT_DEV_ROOT_TOKEN_ID=$pinternVaultRootToken `
        -e VAULT_DEV_LISTEN_ADDRESS=0.0.0.0:8200 `
        -p 127.0.0.1:18200:8200 `
        -d hashicorp/vault:2.0.3 server -dev | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'Vault smoke container could not start.'
    }

    $pinternVaultReady = $false
    for ($pinternAttempt = 0; $pinternAttempt -lt 30; $pinternAttempt++) {
        docker exec `
            -e VAULT_ADDR=http://127.0.0.1:8200 `
            -e VAULT_TOKEN=$pinternVaultRootToken `
            $pinternVaultContainer vault status | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $pinternVaultReady = $true
            break
        }

        Start-Sleep -Seconds 1
    }

    if (-not $pinternVaultReady) {
        throw 'Vault smoke container did not become ready.'
    }

    $pinternBootstrapHeaders = @{ 'X-Vault-Token' = $pinternVaultRootToken }
    $pinternMountBody = @{
        type = 'kv'
        options = @{ version = '2' }
    } | ConvertTo-Json
    Invoke-RestMethod `
        -Method Post `
        -Uri "$pinternVaultAddress/v1/sys/mounts/pintern-dev" `
        -Headers $pinternBootstrapHeaders `
        -ContentType 'application/json' `
        -Body $pinternMountBody | Out-Null

    $pinternPolicy = @'
path "pintern-dev/data/+/sources/+" {
  capabilities = ["create", "read", "update", "delete"]
}

path "pintern-dev/data/+/connections/+" {
  capabilities = ["create", "read", "update", "delete"]
}
'@
    $pinternPolicyBody = @{ policy = $pinternPolicy } | ConvertTo-Json
    Invoke-RestMethod `
        -Method Put `
        -Uri "$pinternVaultAddress/v1/sys/policies/acl/pintern-smoke" `
        -Headers $pinternBootstrapHeaders `
        -ContentType 'application/json' `
        -Body $pinternPolicyBody | Out-Null

    $pinternTokenBody = @{
        policies = @('pintern-smoke')
        no_default_policy = $true
        ttl = '15m'
    } | ConvertTo-Json
    $pinternTokenResponse = Invoke-RestMethod `
        -Method Post `
        -Uri "$pinternVaultAddress/v1/auth/token/create" `
        -Headers $pinternBootstrapHeaders `
        -ContentType 'application/json' `
        -Body $pinternTokenBody
    $pinternVaultToken = $pinternTokenResponse.auth.client_token
    if ([string]::IsNullOrWhiteSpace($pinternVaultToken)) {
        throw 'Limited smoke token creation failed.'
    }

    $pinternAssignedPolicies = @($pinternTokenResponse.auth.policies)
    if ($pinternAssignedPolicies.Count -ne 1 -or
        $pinternAssignedPolicies[0] -ne 'pintern-smoke') {
        throw 'Limited smoke token received an unexpected policy set.'
    }

    $pinternLimitedHeaders = @{ 'X-Vault-Token' = $pinternVaultToken }
    Assert-CheckNexusVaultAccessDenied `
        -Uri "$pinternVaultAddress/v1/sys/mounts" `
        -Headers $pinternLimitedHeaders
    Assert-CheckNexusVaultAccessDenied `
        -Uri "$pinternVaultAddress/v1/pintern-dev/data/smoke/unrelated/policy-denial" `
        -Headers $pinternLimitedHeaders

    [Environment]::SetEnvironmentVariable('PINTERN_VAULT_SMOKE_ENABLED', 'true', 'Process')
    [Environment]::SetEnvironmentVariable('PINTERN_VAULT_SMOKE_ADDRESS', $pinternVaultAddress, 'Process')
    [Environment]::SetEnvironmentVariable('PINTERN_VAULT_SMOKE_TOKEN', $pinternVaultToken, 'Process')

    dotnet test `
        (Join-Path $PSScriptRoot 'test\CheckNexus.Vault.Tests\CheckNexus.Vault.Tests.csproj') `
        -c Release `
        --no-build `
        --no-restore `
        --nologo `
        --filter 'Category=LiveVault'
    if ($LASTEXITCODE -ne 0) {
        throw 'Live Vault adapter smoke test failed.'
    }

    Write-Output 'Vault 2.0.3 KV v2 round-trip passed with a limited policy token.'
}
finally {
    [Environment]::SetEnvironmentVariable('PINTERN_VAULT_SMOKE_ENABLED', $null, 'Process')
    [Environment]::SetEnvironmentVariable('PINTERN_VAULT_SMOKE_ADDRESS', $null, 'Process')
    [Environment]::SetEnvironmentVariable('PINTERN_VAULT_SMOKE_TOKEN', $null, 'Process')

    $pinternVaultTarget = docker ps -a `
        --filter "name=^/$pinternVaultContainer$" `
        --filter "label=$pinternVaultLabel" `
        --format '{{.Names}}'
    if ($pinternVaultTarget -eq $pinternVaultContainer) {
        docker rm -f $pinternVaultContainer | Out-Null
    }
}

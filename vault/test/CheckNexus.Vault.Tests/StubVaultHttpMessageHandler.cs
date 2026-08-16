namespace CheckNexus.Vault.Tests;

// islevi: Her Vault HTTP istegini test tarafindan verilen callback'e yonlendirir.
// sistemdeki gorevi: Endpoint, auth header ve secret payload assertion'larini deterministik request sinirinda tutar.
internal sealed class StubVaultHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

    public StubVaultHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    // Provider request'ini dis ag yerine test callback'ine iletir.
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        => _handler(request);
}

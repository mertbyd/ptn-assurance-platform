namespace CheckNexus.Vault.Tests;

// islevi: Vault provider testlerine kontrol edilen HttpClient instance'ini verir.
// sistemdeki gorevi: Gercek aga cikmadan provider request/payload/header davranisinin test edilmesini saglar.
internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public StubHttpClientFactory(HttpMessageHandler handler)
    {
        _client = new HttpClient(handler);
    }

    // Provider'in named client talebine test client'ini dondurur.
    public HttpClient CreateClient(string name) => _client;
}

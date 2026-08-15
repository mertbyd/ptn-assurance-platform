using System;
using System.Net.Http;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IdentityModel;

namespace Ptn.TestModule;

// islevi: Test Module HTTP API'sini disaridan, gercek token ile cagiran elle dogrulama aracidir.
// sistemdeki gorevi: Authenticator'dan alinan access token'in bu hostta kabul edildigini kanitlar.
// Dinamik proxy ornegi, yayimlanmis bir AppService sozlesmesi olusana kadar burada yoktur.
public class ClientDemoService : ITransientDependency
{
    private readonly IIdentityModelAuthenticationService _authenticationService;
    private readonly IConfiguration _configuration;

    public ClientDemoService(
        IIdentityModelAuthenticationService authenticationService,
        IConfiguration configuration)
    {
        _authenticationService = authenticationService;
        _configuration = configuration;
    }

    public async Task RunAsync()
    {
        await TestWithHttpClientAndIdentityModelAuthenticationServiceAsync();
        await TestAllManuallyAsync();
    }

    /* ABP'nin IIdentityModelAuthenticationService'i ile access token alip HTTP istegi yapar. */
    private async Task TestWithHttpClientAndIdentityModelAuthenticationServiceAsync()
    {
        Console.WriteLine();
        Console.WriteLine($"***** {nameof(TestWithHttpClientAndIdentityModelAuthenticationServiceAsync)} *****");

        var accessToken = await _authenticationService.GetAccessTokenAsync(
            new IdentityClientConfiguration(
                _configuration["IdentityClients:Default:Authority"]!,
                _configuration["IdentityClients:Default:Scope"]!,
                _configuration["IdentityClients:Default:ClientId"]!,
                _configuration["IdentityClients:Default:ClientSecret"]!,
                _configuration["IdentityClients:Default:GrantType"]!,
                _configuration["IdentityClients:Default:UserName"],
                _configuration["IdentityClients:Default:UserPassword"]
            )
        );

        using var httpClient = new HttpClient();
        httpClient.SetBearerToken(accessToken);

        await WriteResponseAsync(httpClient);
    }

    /* Token'i dogrudan discovery + password grant ile alir; ABP soyutlamasi olmadan ayni akisi gosterir. */
    private async Task TestAllManuallyAsync()
    {
        Console.WriteLine();
        Console.WriteLine($"***** {nameof(TestAllManuallyAsync)} *****");

        var client = new HttpClient();
        var discovery = await client.GetDiscoveryDocumentAsync(_configuration["IdentityClients:Default:Authority"]);
        if (discovery.IsError)
        {
            Console.WriteLine(discovery.Error);
            return;
        }

        var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = discovery.TokenEndpoint,
            ClientId = _configuration["IdentityClients:Default:ClientId"]!,
            ClientSecret = _configuration["IdentityClients:Default:ClientSecret"],
            UserName = _configuration["IdentityClients:Default:UserName"]!,
            Password = _configuration["IdentityClients:Default:UserPassword"]!,
            Scope = _configuration["IdentityClients:Default:Scope"]
        });

        if (tokenResponse.IsError)
        {
            Console.WriteLine(tokenResponse.Error);
            return;
        }

        Console.WriteLine(tokenResponse.Json);

        using var httpClient = new HttpClient();
        httpClient.SetBearerToken(tokenResponse.AccessToken!);

        await WriteResponseAsync(httpClient);
    }

    private async Task WriteResponseAsync(HttpClient httpClient)
    {
        var url = _configuration["RemoteServices:TestModule:BaseUrl"] + "health";

        var responseMessage = await httpClient.GetAsync(url);
        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new Exception("Remote server returns error code: " + responseMessage.StatusCode);
        }

        Console.WriteLine("Result: " + await responseMessage.Content.ReadAsStringAsync());
    }
}

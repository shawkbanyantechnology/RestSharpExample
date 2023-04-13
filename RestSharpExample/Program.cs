using System.Text.Json.Serialization;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;

var client_id = "your client id";
var client_secret = "your client secret";

using var banyanClient = new BanyanApiClient(client_id, client_secret);
var accessorials = await banyanClient.GetAccessorials();
var shipments = await banyanClient.GetShipments();
var singleShipment = await banyanClient.GetShipment(50777684);
 
foreach (var accessorial in accessorials.Take(4))
{
    await Console.Out.WriteLineAsync(accessorial.ToString());
}

await Console.Out.WriteLineAsync(string.Empty);

foreach (var shipment in shipments)
{
    await Console.Out.WriteLineAsync(shipment.ToString());
}

await Console.Out.WriteLineAsync(string.Empty);

await Console.Out.WriteLineAsync(singleShipment.ToString());



await Console.In.ReadLineAsync();

public class BanyanApiClient : IBanyanApiClient, IDisposable
{
    private readonly RestClient _client;

    public BanyanApiClient(string apiKey, string apiKeySecret)
    {
        var options = new RestClientOptions("https://ws.integration.banyantechnology.com/")
        {
            Authenticator = new BanyanAuthenticator("https://ws.integration.banyantechnology.com/", apiKey, apiKeySecret)
        };

        _client = new RestClient(options);
    }

    public async Task<IEnumerable<BanyanAccessorial>> GetAccessorials()
    {
        var response = await _client.GetJsonAsync<IEnumerable<BanyanAccessorial>>("/api/StaticData/Accessorials");

        return response ?? new List<BanyanAccessorial>();
    }

    public async Task<IEnumerable<BanyanShipment>> GetShipments()
    {
        var response = await _client.GetJsonAsync<IEnumerable<BanyanShipment>>("api/v3/shipments/");

        return response ?? new List<BanyanShipment>();
    }

    public async Task<BanyanShipment> GetShipment(int id)
    {
        var response = await _client.GetJsonAsync<BanyanShipment>($"api/v3/shipments/{id}");

        return response ?? new BanyanShipment();
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public interface IBanyanApiClient
{
    Task<IEnumerable<BanyanAccessorial>> GetAccessorials();
    Task<IEnumerable<BanyanShipment>> GetShipments();
    Task<BanyanShipment> GetShipment(int id);
}


public class BanyanAuthenticator : AuthenticatorBase
{
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public BanyanAuthenticator(string baseUrl, string clientId, string clientSecret) : base("")
    {
        _baseUrl = baseUrl;
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
    {
        Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
        return new HeaderParameter(KnownHeaders.Authorization, Token);
    }

    private async Task<string> GetToken()
    {
        var options = new RestClientOptions(_baseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(_clientId, HttpUtility.UrlEncode(_clientSecret))
        };

        using var client = new RestClient(options);


        var request = new RestRequest("auth/connect/token")
            .AddParameter("grant_type", "client_credentials");

        var response = await client.PostAsync<TokenResponse>(request);
        return $"{response!.TokenType} {response!.AccessToken}";
    }
}


public record BanyanAccessorial()
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Type: {Type}, Abbreviation: {Abbreviation}, Description: {Description} ";
    }
}

public record BanyanShipment
{
    public int LoadId { get; set; }
    public string Status { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Load Id: {LoadId}, Status: {Status}";
    }
}

internal record TokenResponse()
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;

    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;
}


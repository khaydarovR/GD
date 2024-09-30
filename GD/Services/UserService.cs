using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GD.Services;

public class UserService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Response> GetInfo(string jwt)
    {
        using var client = _httpClientFactory.CreateClient("api");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/info");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<Response>())!;
    }
}

public class Response
{
    public Guid Id { get; set; }
    public double Balance { get; set; }
    public double PosLati { get; set; }
    public double PosLong { get; set; }
}

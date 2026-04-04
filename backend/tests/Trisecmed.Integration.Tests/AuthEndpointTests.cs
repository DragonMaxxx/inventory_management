using System.Net;
using System.Net.Http.Json;

namespace Trisecmed.Integration.Tests;

public class AuthEndpointTests : IClassFixture<TrisecmedApiFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(TrisecmedApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "wrong@test.com", password = "wrongpassword" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Users_WithoutToken_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Devices_ShouldBeAccessible()
    {
        var response = await _client.GetAsync("/api/v1/devices");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

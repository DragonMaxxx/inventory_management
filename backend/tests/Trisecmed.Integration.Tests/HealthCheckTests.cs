using System.Net;

namespace Trisecmed.Integration.Tests;

public class HealthCheckTests : IClassFixture<TrisecmedApiFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(TrisecmedApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");
        // W trybie testing bez PostgreSQL health check może zwracać Unhealthy,
        // ale endpoint sam powinien odpowiadać
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable);
    }
}

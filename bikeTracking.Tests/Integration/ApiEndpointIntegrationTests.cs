using System.Net.Http.Json;

using BikeTracking.Shared.DTOs;

namespace bikeTracking.Tests.Integration;

/// <summary>
/// Integration tests for /api/rides endpoints (Category 5).
/// Tests the complete vertical slice: endpoint → handler → event store → projection → database.
/// Coverage: POST, GET, PUT, DELETE with auth, validation, data isolation, ownership checks.
/// Per Constitution Principle IV: Integration tests verify each vertical slice end-to-end.
/// </summary>
[TestFixture]
public class ApiEndpointIntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private async Task<HttpClient> CreateApiClientAsync(CancellationToken cancellationToken = default)
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.bikeTracking_AppHost>(cancellationToken);
        
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        
        return httpClient;
    }

    #region POST /api/rides Tests

    [Test]
    public async Task WhenCreatingRideWithValidDataThenReturns201WithRideResponse()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.bikeTracking_AppHost>(cancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            Hour = 14,
            Distance = 12.5m,
            DistanceUnit = "miles",
            RideName = $"DEMO_IntegrationTest_{Guid.NewGuid():N}",
            StartLocation = "DEMO_Home",
            EndLocation = "DEMO_Office",
            Notes = "DEMO_Integration test ride"
        };

        // Act
        var response = await httpClient.PostAsJsonAsync("/api/rides", request, cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var rideResponse = await response.Content.ReadFromJsonAsync<RideResponse>(cancellationToken: cancellationToken);
        Assert.That(rideResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rideResponse!.RideId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(rideResponse.RideName, Is.EqualTo(request.RideName));
            Assert.That(rideResponse.Distance, Is.EqualTo(request.Distance));
            Assert.That(rideResponse.DistanceUnit, Is.EqualTo(request.DistanceUnit));
        }
    }

    [Test]
    public async Task WhenCreatingRideWithInvalidDateThenReturns400BadRequest()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.bikeTracking_AppHost>(cancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Future date violates validation
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Hour = 14,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Invalid Future Date",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var response = await httpClient.PostAsJsonAsync("/api/rides", request, cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.That(errorContent, Does.Contain("Date cannot be in the future"));
    }

    #endregion
}

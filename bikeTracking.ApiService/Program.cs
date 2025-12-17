using bikeTracking.ApiService.Endpoints;

using BikeTracking.Domain.Commands;
using BikeTracking.Domain.Services;
using BikeTracking.Infrastructure.Data;
using BikeTracking.Infrastructure.Repositories;
using BikeTracking.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// ===== Database Configuration =====
var connectionString = builder.Configuration.GetConnectionString("BikeTrackingDb")
    ?? throw new InvalidOperationException("BikeTrackingDb connection string not found");

builder.Services.AddDbContext<BikeTrackingContext>(options =>
    options.UseSqlServer(connectionString));


// ===== Authentication & Authorization =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/login";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.ClientId = builder.Configuration["Authentication:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

builder.Services.AddAuthorization();

// ===== Domain Services =====
builder.Services.AddScoped<IWeatherService, NoaaWeatherService>();
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IRideProjectionRepository, RideProjectionRepository>();
builder.Services.AddScoped<CreateRideCommandHandler>();
builder.Services.AddScoped<EditRideCommandHandler>();

builder.Services.AddHttpClient<NoaaWeatherService>();

// ===== Razor Components & Routing =====
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===== API Configuration =====
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== Middleware Pipeline =====
app.UseExceptionHandler();
app.UseCors();
// ===== Authentication & Authorization Middleware =====
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}

//// ===== Razor Components Endpoints =====
//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();

// ===== API Endpoints =====
app.MapGroup("/api")
    .WithName("API");

// Map Rides endpoints
RidesEndpoints.MapRidesEndpoints(app);

app.MapDefaultEndpoints();

app.Run();

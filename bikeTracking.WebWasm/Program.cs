using bikeTracking.WebWasm;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Use fake authentication for development
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, TestAuthStateProvider>();

// Real authentication - Use MSAL for Entra ID
//builder.Services.AddMsalAuthentication(options =>
//{
//    builder.Configuration.Bind("Local", options.ProviderOptions.Authentication);
//    options.ProviderOptions.DefaultAccessTokenScopes.Add($"api://{builder.Configuration["Authentication:ClientId"]}/access_as_user");
//});

await builder.Build().RunAsync();

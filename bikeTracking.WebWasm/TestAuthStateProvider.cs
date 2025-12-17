using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace bikeTracking.WebWasm;

public class TestAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "Kevin"),
            new Claim(ClaimTypes.Email, "kevin@testing.com"),
            new Claim(ClaimTypes.Role, "User")
        ], "Test");

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}

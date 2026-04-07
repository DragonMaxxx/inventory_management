using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Trisecmed.Web.Services;

public class TrisecmedAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;

    public TrisecmedAuthStateProvider(AuthService authService)
    {
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await _authService.GetAuthStateAsync();
        return new AuthenticationState(user);
    }

    public void NotifyAuthChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

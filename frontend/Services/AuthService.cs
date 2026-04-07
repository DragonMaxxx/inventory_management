using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.JSInterop;
using Trisecmed.Web.Models;

namespace Trisecmed.Web.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private const string TokenKey = "trisecmed_token";

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/login", new LoginRequest(email, password));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return (false, error?.GetValueOrDefault("error") ?? "Błąd logowania");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result?.AccessToken is not null)
            {
                await _js.InvokeVoidAsync("localStorageHelper.setItem", TokenKey, result.AccessToken);
                return (true, null);
            }
            return (false, "Brak tokenu w odpowiedzi");
        }
        catch (Exception ex)
        {
            return (false, $"Błąd połączenia: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorageHelper.removeItem", TokenKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string?>("localStorageHelper.getItem", TokenKey);
    }

    public async Task<string?> GetUserRoleAsync()
    {
        var principal = await GetAuthStateAsync();
        return principal.FindFirst(ClaimTypes.Role)?.Value;
    }

    public async Task<string?> GetUserEmailAsync()
    {
        var principal = await GetAuthStateAsync();
        return principal.FindFirst("email")?.Value;
    }

    public async Task<ClaimsPrincipal> GetAuthStateAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new ClaimsPrincipal(new ClaimsIdentity());

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await LogoutAsync();
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var claims = jwt.Claims.ToList();
            // Map role claim for Blazor auth
            var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaim is not null)
                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));

            var identity = new ClaimsIdentity(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            await LogoutAsync();
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}

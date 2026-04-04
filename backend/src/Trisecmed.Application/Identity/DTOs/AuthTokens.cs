namespace Trisecmed.Application.Identity.DTOs;

public record AuthTokens(string AccessToken, string RefreshToken, DateTime AccessTokenExpires);

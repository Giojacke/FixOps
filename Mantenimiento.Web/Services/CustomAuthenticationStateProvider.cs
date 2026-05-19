using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Mantenimiento.Web.Services;

public class CustomAuthenticationStateProvider(
    ILocalStorageService localStorageService,
    HttpClient httpClient) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await localStorageService.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }
        catch
        {
            await localStorageService.RemoveItemAsync("authToken");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    // JwtSecurityTokenHandler serializes ClaimTypes to short JWT names — map them back
    private static readonly Dictionary<string, string> JwtToClaimType = new()
    {
        ["nameid"]       = ClaimTypes.NameIdentifier,
        ["unique_name"]  = ClaimTypes.Name,
        ["email"]        = ClaimTypes.Email,
        ["role"]         = ClaimTypes.Role,
    };

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;

        var jsonBytes = ParseBase64WithoutPadding(parts[1]);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        if (keyValuePairs == null) return claims;

        // Handle roles (can be array or single string)
        if (keyValuePairs.TryGetValue("role", out var rolesObj))
        {
            var rolesString = rolesObj.ToString() ?? string.Empty;
            if (rolesString.Trim().StartsWith("["))
            {
                foreach (var r in JsonSerializer.Deserialize<string[]>(rolesString) ?? [])
                    claims.Add(new Claim(ClaimTypes.Role, r));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, rolesString));
            }
            keyValuePairs.Remove("role");
        }

        foreach (var kvp in keyValuePairs)
        {
            var claimType = JwtToClaimType.TryGetValue(kvp.Key, out var mapped) ? mapped : kvp.Key;
            claims.Add(new Claim(claimType, kvp.Value?.ToString() ?? string.Empty));
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

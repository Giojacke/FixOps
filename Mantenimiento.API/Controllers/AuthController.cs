using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mantenimiento.Application.Common;
using Mantenimiento.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(
    UserManager<Usuario> userManager,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(ApiResponse<object>.Fail("Credenciales inválidas."));

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return Ok(ApiResponse<LoginResponse>.Ok(
            new LoginResponse { Token = token, Nombre = user.Nombre, Rol = user.Rol.ToString() },
            "Sesión iniciada correctamente."));
    }

    private string GenerateJwtToken(Usuario user, IList<string> roles)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nombre),
            new(ClaimTypes.Email, user.Email!),
            new("RolEnum", user.Rol.ToString())
        };
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(claims),
            Expires            = DateTime.UtcNow.AddDays(7),
            SigningCredentials  = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer             = jwtSettings["Issuer"],
            Audience           = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}

public class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token  { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol    { get; set; } = string.Empty;
}

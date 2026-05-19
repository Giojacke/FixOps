using System.Security.Claims;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Mantenimiento.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId => Guid.TryParse(
        httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
        ? id : null;

    public string UserName => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value
                              ?? "Desconocido";

    public string Email => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
                           ?? string.Empty;
}

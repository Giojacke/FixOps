namespace Mantenimiento.Application.Interfaces;

public interface ICurrentUserService
{
    Guid?  UserId   { get; }
    string UserName { get; }
    string Email    { get; }
}

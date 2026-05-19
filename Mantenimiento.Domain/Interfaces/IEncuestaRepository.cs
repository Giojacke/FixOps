using Mantenimiento.Domain.Entities;

namespace Mantenimiento.Domain.Interfaces;

public interface IEncuestaRepository : IRepository<EncuestaSatisfaccion>
{
    Task<IEnumerable<EncuestaSatisfaccion>> GetAllWithOrdenAsync();
}

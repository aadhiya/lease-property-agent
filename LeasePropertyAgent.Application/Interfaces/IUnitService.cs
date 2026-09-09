using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IUnitService
{
    Task<List<UnitListResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
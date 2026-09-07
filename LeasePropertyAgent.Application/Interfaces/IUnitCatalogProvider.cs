using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IUnitCatalogProvider
{
    Task<UnitCatalog> GetCatalogAsync(
        CancellationToken cancellationToken = default);

    Task<UnitCatalogItem?> FindUnitAsync(
        string unitId,
        CancellationToken cancellationToken = default);
}
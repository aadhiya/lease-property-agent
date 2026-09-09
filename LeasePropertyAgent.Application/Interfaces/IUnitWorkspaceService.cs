using LeasePropertyAgent.Application.Models.Workspace;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IUnitWorkspaceService
{
    Task<UnitWorkspaceResponse?> GetWorkspaceAsync(
        Guid unitId,
        CancellationToken cancellationToken = default);
}
using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IReviewRepository
{
    Task<LeaseField?> GetLeaseFieldByIdAsync(
        Guid fieldId,
        CancellationToken cancellationToken = default);

    Task<LeaseFlag?> GetLeaseFlagByIdAsync(
        Guid flagId,
        CancellationToken cancellationToken = default);

    Task<WorkOrder?> GetWorkOrderByIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default);

    Task AddReviewActionAsync(
        ReviewAction reviewAction,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
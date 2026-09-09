using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IReviewService
{
    Task ReviewLeaseFieldAsync(
        Guid fieldId,
        ReviewLeaseFieldRequest request,
        CancellationToken cancellationToken = default);

    Task ReviewLeaseFlagAsync(
        Guid flagId,
        ReviewLeaseFlagRequest request,
        CancellationToken cancellationToken = default);

    Task ReviewWorkOrderAsync(
        Guid workOrderId,
        ReviewWorkOrderRequest request,
        CancellationToken cancellationToken = default);
}
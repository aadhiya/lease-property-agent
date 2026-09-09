using System.Text.Json;
using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task ReviewLeaseFieldAsync(
        Guid fieldId,
        ReviewLeaseFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (fieldId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid lease field ID is required.",
                nameof(fieldId));
        }

        var field = await _reviewRepository.GetLeaseFieldByIdAsync(
            fieldId,
            cancellationToken);

        if (field is null)
        {
            throw new KeyNotFoundException(
                $"Lease field '{fieldId}' was not found.");
        }

        var action = NormalizeAction(request.Action);

        switch (action)
        {
            case "accept":
                field.ReviewStatus = LeaseFieldReviewStatus.Accepted;
                break;

            case "reject":
                field.ReviewStatus = LeaseFieldReviewStatus.Rejected;
                break;

            case "edit":
                if (string.IsNullOrWhiteSpace(request.ReviewedValue))
                {
                    throw new ArgumentException(
                        "ReviewedValue is required when editing a lease field.",
                        nameof(request));
                }

                field.ReviewedValue = request.ReviewedValue;
                field.ReviewStatus = LeaseFieldReviewStatus.Edited;
                break;

            default:
                throw new ArgumentException(
                    "Lease field action must be accept, reject, or edit.",
                    nameof(request));
        }

        field.ReviewedAt = DateTime.UtcNow;

        await AddReviewActionAsync(
            entityType: "LeaseField",
            entityId: field.Id,
            action: action,
            previousValue: field.ExtractedValue,
            newValue: field.ReviewedValue,
            reason: request.Reason,
            cancellationToken);

        await _reviewRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReviewLeaseFlagAsync(
        Guid flagId,
        ReviewLeaseFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (flagId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid lease flag ID is required.",
                nameof(flagId));
        }

        var flag = await _reviewRepository.GetLeaseFlagByIdAsync(
            flagId,
            cancellationToken);

        if (flag is null)
        {
            throw new KeyNotFoundException(
                $"Lease flag '{flagId}' was not found.");
        }

        var action = NormalizeAction(request.Action);

        switch (action)
        {
            case "accept":
                flag.Status = ReviewStatus.Accepted;
                break;

            case "reject":
                flag.Status = ReviewStatus.Rejected;
                break;

            case "dismiss":
                flag.Status = ReviewStatus.Dismissed;
                break;

            default:
                throw new ArgumentException(
                    "Lease flag action must be accept, reject, or dismiss.",
                    nameof(request));
        }

        await AddReviewActionAsync(
            entityType: "LeaseFlag",
            entityId: flag.Id,
            action: action,
            previousValue: flag.Message,
            newValue: null,
            reason: request.Reason,
            cancellationToken);

        await _reviewRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReviewWorkOrderAsync(
        Guid workOrderId,
        ReviewWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (workOrderId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid work order ID is required.",
                nameof(workOrderId));
        }

        var workOrder = await _reviewRepository.GetWorkOrderByIdAsync(
            workOrderId,
            cancellationToken);

        if (workOrder is null)
        {
            throw new KeyNotFoundException(
                $"Work order '{workOrderId}' was not found.");
        }

        var action = NormalizeAction(request.Action);

        var previousValue = JsonSerializer.Serialize(new
        {
            workOrder.Title,
            workOrder.Description
        });

        switch (action)
        {
            case "accept":
                workOrder.Status = WorkOrderStatus.Accepted;
                break;

            case "reject":
                workOrder.Status = WorkOrderStatus.Rejected;
                break;

            case "edit":
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    throw new ArgumentException(
                        "Title is required when editing a work order.",
                        nameof(request));
                }

                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    throw new ArgumentException(
                        "Description is required when editing a work order.",
                        nameof(request));
                }

                workOrder.Title = request.Title;
                workOrder.Description = request.Description;
                workOrder.Status = WorkOrderStatus.Edited;
                break;

            default:
                throw new ArgumentException(
                    "Work order action must be accept, reject, or edit.",
                    nameof(request));
        }

        workOrder.ReviewedAt = DateTime.UtcNow;

        var newValue = JsonSerializer.Serialize(new
        {
            workOrder.Title,
            workOrder.Description,
            Status = workOrder.Status.ToString()
        });

        await AddReviewActionAsync(
            entityType: "WorkOrder",
            entityId: workOrder.Id,
            action: action,
            previousValue: previousValue,
            newValue: newValue,
            reason: request.Reason,
            cancellationToken);

        await _reviewRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task AddReviewActionAsync(
        string entityType,
        Guid entityId,
        string action,
        string? previousValue,
        string? newValue,
        string? reason,
        CancellationToken cancellationToken)
    {
        var reviewAction = new ReviewAction
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PreviousValue = previousValue,
            NewValue = newValue,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddReviewActionAsync(
            reviewAction,
            cancellationToken);
    }

    private static string NormalizeAction(string? action)
    {
        return action?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
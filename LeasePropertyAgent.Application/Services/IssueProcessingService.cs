using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Application.Services;

/// <summary>
/// Coordinates issue ingestion from property images into a reviewable
/// issue and draft work order. Unit identity is supplied by the caller;
/// the agent is responsible only for interpreting the image evidence.
/// </summary>
public class IssueProcessingService : IIssueProcessingService
{
    private readonly IIssueAgent _issueAgent;
    private readonly IIssueRepository _issueRepository;
    private readonly IUnitRepository _unitRepository;

    public IssueProcessingService(
        IIssueAgent issueAgent,
        IIssueRepository issueRepository,
        IUnitRepository unitRepository)
    {
        _issueAgent = issueAgent;
        _issueRepository = issueRepository;
        _unitRepository = unitRepository;
    }

    public async Task<IssueProcessingResult> ProcessAsync(
        IssueProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UnitId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid unit ID is required.",
                nameof(request));
        }

        if (request.Images is null || request.Images.Count == 0)
        {
            throw new ArgumentException(
                "At least one issue image is required.",
                nameof(request));
        }

        var unit = await _unitRepository.GetByIdAsync(
            request.UnitId,
            cancellationToken);

        if (unit is null)
        {
            throw new KeyNotFoundException(
                $"Unit '{request.UnitId}' was not found.");
        }

        var agentResult = await _issueAgent.AssessAsync(
            request.Images,
            cancellationToken);

        var issue = MapToIssue(
            request,
            unit,
            agentResult);

        await _issueRepository.AddAsync(
            issue,
            cancellationToken);

        await _issueRepository.SaveChangesAsync(
            cancellationToken);

        return new IssueProcessingResult
        {
            Issue = issue,
            AgentResult = agentResult
        };
    }

    private static Issue MapToIssue(
        IssueProcessingRequest request,
        Unit unit,
        IssueAgentResult agentResult)
    {
        var now = DateTime.UtcNow;

        var issue = new Issue
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            Unit = unit,
            Title = agentResult.Assessment.Title,
            Description = agentResult.Assessment.Description,
            ConditionAssessment =
                agentResult.Assessment.ConditionAssessment,
            Confidence = agentResult.Assessment.Confidence,
            CreatedAt = now,
            Status = ReviewStatus.Pending
        };

        foreach (var image in request.Images)
        {
            var assessment = agentResult.Assessment.ImageAssessments
                .FirstOrDefault(
                    x => string.Equals(
                        x.FileName,
                        image.FileName,
                        StringComparison.OrdinalIgnoreCase));

            var issueImage = new IssueImage
            {
                Id = Guid.NewGuid(),
                IssueId = issue.Id,
                FileName = image.FileName,
                FilePath = image.FilePath,
                Observation = assessment?.Observation,
                Confidence = assessment?.Confidence,
                Issue = issue
            };

            issue.Images.Add(issueImage);
        }

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            IssueId = issue.Id,
            UnitId = unit.Id,
            Title = agentResult.WorkOrder.Title,
            Description = agentResult.WorkOrder.Description,
            Status = WorkOrderStatus.Draft,
            CreatedAt = now,
            Issue = issue,
            Unit = unit
        };

        issue.WorkOrders.Add(workOrder);

        return issue;
    }
}
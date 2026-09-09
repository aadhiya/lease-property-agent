using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models.Workspace;
using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Services;

public class UnitWorkspaceService : IUnitWorkspaceService
{
    private readonly IUnitRepository _unitRepository;

    public UnitWorkspaceService(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<UnitWorkspaceResponse?> GetWorkspaceAsync(
        Guid unitId,
        CancellationToken cancellationToken = default)
    {
        if (unitId == Guid.Empty)
        {
            return null;
        }

        var unit = await _unitRepository.GetWorkspaceByIdAsync(
            unitId,
            cancellationToken);

        if (unit is null)
        {
            return null;
        }

        return MapWorkspace(unit);
    }

    private static UnitWorkspaceResponse MapWorkspace(Unit unit)
    {
        return new UnitWorkspaceResponse
        {
            UnitId = unit.Id,
            UnitNumber = unit.UnitNumber,
            UnitType = unit.UnitType,
            Area = unit.Area,
            Status = unit.Status.ToString(),

            Building = unit.Building is null
                ? null
                : new BuildingWorkspaceResponse
                {
                    Id = unit.Building.Id,
                    Name = unit.Building.Name
                },

            Property = unit.Building?.Property is null
                ? null
                : new PropertyWorkspaceResponse
                {
                    Id = unit.Building.Property.Id,
                    Name = unit.Building.Property.Name,
                    Address = unit.Building.Property.Address
                },

            Leases = unit.Leases
                .Select(MapLease)
                .ToList(),

            Issues = unit.Issues
                .Select(MapIssue)
                .ToList()
        };
    }

    private static LeaseWorkspaceResponse MapLease(Lease lease)
    {
        return new LeaseWorkspaceResponse
        {
            Id = lease.Id,
            DocumentName = lease.DocumentName,
            Status = lease.Status.ToString(),

            CommencementDate = lease.CommencementDate,
            ExpiryDate = lease.ExpiryDate,
            TermMonths = lease.TermMonths,

            MonthlyRent = lease.MonthlyRent,
            AnnualRent = lease.AnnualRent,
            RentFrequency = lease.RentFrequency,
            Currency = lease.Currency,
            DepositAmount = lease.DepositAmount,

            EscalationClause = lease.EscalationClause?.ToString(),
            RenewalTerms = lease.RenewalTerms,
            TerminationTerms = lease.TerminationTerms,

            Parties = lease.Parties
                .Select(party => new LeasePartyWorkspaceResponse
                {
                    Id = party.Id,
                    Role = party.Role.ToString(),
                    Name = party.Name,
                    IsPresent = party.IsPresent,
                    IsSigned = party.IsSigned
                })
                .ToList(),

            Fields = lease.Fields
                .Select(field => new LeaseFieldWorkspaceResponse
                {
                    Id = field.Id,
                    FieldName = field.FieldName,
                    ExtractedValue = field.ExtractedValue,
                    ReviewedValue = field.ReviewedValue,
                    Confidence = field.Confidence,
                    SourcePage = field.SourcePage,
                    SourceText = field.SourceText,
                    ReviewStatus = field.ReviewStatus.ToString()
                })
                .ToList(),

            Flags = lease.Flags
                .Select(flag => new LeaseFlagWorkspaceResponse
                {
                    Id = flag.Id,
                    Type = flag.Type,
                    Severity = flag.Severity.ToString(),
                    Message = flag.Message,
                    SourcePage = flag.SourcePage,
                    SourceText = flag.SourceText,
                    Status = flag.Status.ToString()
                })
                .ToList(),

            ValidationResults = lease.ValidationResults
                .Select(result => new ValidationWorkspaceResponse
                {
                    Id = result.Id,
                    ValidationRunId = result.ValidationRunId,
                    RuleId = result.RuleId,
                    Status = result.Status.ToString(),
                    Reason = result.Reason,
                    SourcePage = result.SourcePage,
                    SourceText = result.SourceText,
                    CreatedAt = result.CreatedAt
                })
                .ToList()
        };
    }

    private static IssueWorkspaceResponse MapIssue(Issue issue)
    {
        return new IssueWorkspaceResponse
        {
            Id = issue.Id,
            Title = issue.Title,
            Description = issue.Description,
            ConditionAssessment = issue.ConditionAssessment,
            Confidence = issue.Confidence,
            Status = issue.Status.ToString(),
            CreatedAt = issue.CreatedAt,

            Images = issue.Images
                .Select(image => new IssueImageWorkspaceResponse
                {
                    Id = image.Id,
                    FileName = image.FileName,
                    FilePath = image.FilePath,
                    Observation = image.Observation,
                    Confidence = image.Confidence
                })
                .ToList(),

            WorkOrders = issue.WorkOrders
                .Select(workOrder => new WorkOrderWorkspaceResponse
                {
                    Id = workOrder.Id,
                    UnitId = workOrder.UnitId,
                    Title = workOrder.Title,
                    Description = workOrder.Description,
                    Status = workOrder.Status.ToString(),
                    CreatedAt = workOrder.CreatedAt,
                    ReviewedAt = workOrder.ReviewedAt
                })
                .ToList()
        };
    }
}
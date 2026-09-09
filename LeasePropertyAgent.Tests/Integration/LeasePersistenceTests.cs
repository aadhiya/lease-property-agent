using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Domain.ValueObjects;
using LeasePropertyAgent.Infrastructure.Data;
using LeasePropertyAgent.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Tests.Integration;
[Collection("IntegrationTests")]
public class LeasePersistenceTests
{
    [Fact]
    public async Task LeaseRepository_ShouldPersistCompleteLeaseAggregate()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LeasePropertyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext =
            new LeasePropertyDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        // A lease must reference an existing Unit because UnitId is
        // a relational foreign key in the database.
var property = new Property
{
    Id = Guid.NewGuid(),
    Name = "Marina Crest Residences",
    Address = "Lusail Marina District, Doha"
};

var building = new Building
{
    Id = Guid.NewGuid(),
    PropertyId = property.Id,
    Name = "Tower B"
};

var unit = new Unit
{
    Id = Guid.NewGuid(),
    BuildingId = building.Id,
    UnitNumber = "MC-B-1204",
    UnitType = "2BR",
    Area = 118m,
    Status = UnitStatus.Available
};

property.Buildings.Add(building);
building.Units.Add(unit);

dbContext.Properties.Add(property);

await dbContext.SaveChangesAsync();

        var lease = new Lease
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            DocumentName = "sample-lease.txt",
            DocumentPath = "data/sample-lease.txt",
            Status = LeaseStatus.PendingReview,
            CommencementDate = new DateTime(2026, 1, 1),
            ExpiryDate = new DateTime(2027, 12, 31),
            TermMonths = 24,
            MonthlyRent = 12000m,
            AnnualRent = 144000m,
            RentFrequency = "Monthly",
            Currency = "QAR",
            DepositAmount = 12000m,
            EscalationClause = new EscalationClause
            {
                IsDefined = true,
                Type = "Percentage",
                Percentage = 5m,
                Frequency = "Annual",
                Description = "The rent may be increased by 5% annually."
            },
            RenewalTerms =
                "The lease may be renewed by mutual written agreement.",
            TerminationTerms =
                "Either party may terminate the lease by providing 60 days notice."
        };

        lease.Parties.Add(new LeaseParty
        {
            Id = Guid.NewGuid(),
            Role = PartyRole.Landlord,
            Name = "Marina Crest Holdings W.L.L.",
            IsPresent = true,
            IsSigned = true
        });

        lease.Parties.Add(new LeaseParty
        {
            Id = Guid.NewGuid(),
            Role = PartyRole.Tenant,
            Name = "Ahmed Hassan",
            IsPresent = true,
            IsSigned = true
        });

        lease.Fields.Add(new LeaseField
        {
            Id = Guid.NewGuid(),
            FieldName = "MonthlyRent",
            ExtractedValue = "12000",
            Confidence = 0.98m,
            SourcePage = 2,
            SourceText = "Monthly Rent: QAR 12,000",
            ReviewStatus = LeaseFieldReviewStatus.Pending
        });

        lease.Flags.Add(new LeaseFlag
        {
            Id = Guid.NewGuid(),
            Type = "SuspiciousValue",
            Severity = FlagSeverity.Medium,
            Message = "Monthly rent appears suspicious.",
            SourcePage = 2,
            SourceText = "Monthly Rent: QAR 12,000",
            Status = ReviewStatus.Pending
        });

        var validationRunId = Guid.NewGuid();

        lease.ValidationResults.Add(new ValidationResult
        {
            Id = Guid.NewGuid(),
            ValidationRunId = validationRunId,
            RuleId = "R1",
            Status = ValidationStatus.Pass,
            Reason = "Deposit is greater than or equal to monthly rent.",
            SourcePage = 2,
            SourceText = "Security Deposit: QAR 12,000",
            CreatedAt = DateTime.UtcNow
        });

        var repository = new LeaseRepository(dbContext);

        // Act
        await repository.AddAsync(lease);
        await repository.SaveChangesAsync();

        // Read the aggregate back from SQLite using a fresh context.
        await using var verificationContext =
            new LeasePropertyDbContext(options);

        var savedLease = await verificationContext.Leases
            .Include(x => x.Parties)
            .Include(x => x.Fields)
            .Include(x => x.Flags)
            .Include(x => x.ValidationResults)
            .Include(x => x.Unit)
            .SingleAsync(x => x.Id == lease.Id);

        // Assert
        Assert.NotNull(savedLease);

        Assert.Equal(
            "sample-lease.txt",
            savedLease.DocumentName);

        Assert.Equal(
            LeaseStatus.PendingReview,
            savedLease.Status);

        Assert.Equal(
            unit.Id,
            savedLease.UnitId);

        Assert.NotNull(savedLease.Unit);

        Assert.Equal(
            "MC-B-1204",
            savedLease.Unit!.UnitNumber);

        Assert.Equal(
            12000m,
            savedLease.MonthlyRent);

        Assert.Equal(
            144000m,
            savedLease.AnnualRent);

        Assert.Equal(
            12000m,
            savedLease.DepositAmount);

        Assert.True(
            savedLease.EscalationClause.IsDefined);

        Assert.Equal(
            5m,
            savedLease.EscalationClause.Percentage);

        Assert.Equal(
            "Annual",
            savedLease.EscalationClause.Frequency);

        Assert.Equal(
            2,
            savedLease.Parties.Count);

        Assert.Contains(
            savedLease.Parties,
            party =>
                party.Role == PartyRole.Landlord &&
                party.Name == "Marina Crest Holdings W.L.L." &&
                party.IsSigned);

        Assert.Contains(
            savedLease.Parties,
            party =>
                party.Role == PartyRole.Tenant &&
                party.Name == "Ahmed Hassan" &&
                party.IsSigned);

        Assert.Single(savedLease.Fields);

        var savedField = savedLease.Fields.Single();

        Assert.Equal(
            "MonthlyRent",
            savedField.FieldName);

        Assert.Equal(
            2,
            savedField.SourcePage);

        Assert.Equal(
            "Monthly Rent: QAR 12,000",
            savedField.SourceText);

        Assert.Single(savedLease.Flags);

        var savedFlag = savedLease.Flags.Single();

        Assert.Equal(
            "SuspiciousValue",
            savedFlag.Type);

        Assert.Equal(
            FlagSeverity.Medium,
            savedFlag.Severity);

        Assert.Equal(
            2,
            savedFlag.SourcePage);

        Assert.Single(savedLease.ValidationResults);

        var savedValidation =
            savedLease.ValidationResults.Single();

        Assert.Equal(
            "R1",
            savedValidation.RuleId);

        Assert.Equal(
            validationRunId,
            savedValidation.ValidationRunId);

        Assert.Equal(
            ValidationStatus.Pass,
            savedValidation.Status);

        Assert.Equal(
            2,
            savedValidation.SourcePage);
    }
}
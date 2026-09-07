using LeasePropertyAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Infrastructure.Data;

public class LeasePropertyDbContext : DbContext
{
    public LeasePropertyDbContext(DbContextOptions<LeasePropertyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<LeaseParty> LeaseParties => Set<LeaseParty>();
    public DbSet<LeaseField> LeaseFields => Set<LeaseField>();
    public DbSet<LeaseFlag> LeaseFlags => Set<LeaseFlag>();
    public DbSet<ValidationResult> ValidationResults => Set<ValidationResult>();

    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueImage> IssueImages => Set<IssueImage>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<ReviewAction> ReviewActions => Set<ReviewAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureProperty(modelBuilder);
        ConfigureBuilding(modelBuilder);
        ConfigureUnit(modelBuilder);

        ConfigureLease(modelBuilder);
        ConfigureLeaseParty(modelBuilder);
        ConfigureLeaseField(modelBuilder);
        ConfigureLeaseFlag(modelBuilder);
        ConfigureValidationResult(modelBuilder);

        ConfigureIssue(modelBuilder);
        ConfigureIssueImage(modelBuilder);
        ConfigureWorkOrder(modelBuilder);

        ConfigureReviewAction(modelBuilder);
    }

    private static void ConfigureProperty(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.HasMany(x => x.Buildings)
                .WithOne(x => x.Property)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureBuilding(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasMany(x => x.Units)
                .WithOne(x => x.Building)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureUnit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitNumber)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.UnitType)
                .HasMaxLength(100);

            entity.Property(x => x.Area)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.HasIndex(x => x.UnitNumber);
        });
    }

    private static void ConfigureLease(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lease>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DocumentName)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(x => x.DocumentPath)
                .HasMaxLength(1000);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.Property(x => x.MonthlyRent)
                .HasPrecision(18, 2);

            entity.Property(x => x.AnnualRent)
                .HasPrecision(18, 2);

            entity.Property(x => x.DepositAmount)
                .HasPrecision(18, 2);

            entity.Property(x => x.RentFrequency)
                .HasMaxLength(50);

            entity.Property(x => x.Currency)
                .HasMaxLength(10);

            entity.Property(x => x.RenewalTerms)
                .HasMaxLength(4000);

            entity.Property(x => x.TerminationTerms)
                .HasMaxLength(4000);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            // EscalationClause is stored as columns on the Lease table.
            entity.OwnsOne(x => x.EscalationClause, escalation =>
            {
                escalation.Property(x => x.IsDefined)
                    .HasColumnName("EscalationIsDefined");

                escalation.Property(x => x.Type)
                    .HasColumnName("EscalationType")
                    .HasMaxLength(100);

                escalation.Property(x => x.Percentage)
                    .HasColumnName("EscalationPercentage")
                    .HasPrecision(18, 2);

                escalation.Property(x => x.Amount)
                    .HasColumnName("EscalationAmount")
                    .HasPrecision(18, 2);

                escalation.Property(x => x.Frequency)
                    .HasColumnName("EscalationFrequency")
                    .HasMaxLength(100);

                escalation.Property(x => x.Description)
                    .HasColumnName("EscalationDescription")
                    .HasMaxLength(4000);
            });

            entity.HasOne(x => x.Unit)
                .WithMany(x => x.Leases)
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Parties)
                .WithOne(x => x.Lease)
                .HasForeignKey(x => x.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Fields)
                .WithOne(x => x.Lease)
                .HasForeignKey(x => x.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Flags)
                .WithOne(x => x.Lease)
                .HasForeignKey(x => x.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.ValidationResults)
                .WithOne(x => x.Lease)
                .HasForeignKey(x => x.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UnitId);
            entity.HasIndex(x => x.Status);
        });
    }

    private static void ConfigureLeaseParty(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaseParty>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Role)
                .HasConversion<int>();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(x => x.IsPresent)
                .IsRequired();

            entity.Property(x => x.IsSigned)
                .IsRequired();
        });
    }

    private static void ConfigureLeaseField(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaseField>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FieldName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.ExtractedValue)
                .HasMaxLength(4000);

            entity.Property(x => x.ReviewedValue)
                .HasMaxLength(4000);

            entity.Property(x => x.Confidence)
                .HasPrecision(5, 4);

            entity.Property(x => x.ReviewStatus)
                .HasConversion<int>();

            entity.Property(x => x.SourceText)
                .HasMaxLength(4000);

            entity.HasIndex(x => new
            {
                x.LeaseId,
                x.FieldName
            });
        });
    }

    private static void ConfigureLeaseFlag(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaseFlag>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Severity)
                .HasConversion<int>();

            entity.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(x => x.SourceText)
                .HasMaxLength(4000);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.HasIndex(x => new
            {
                x.LeaseId,
                x.Status
            });
        });
    }

    private static void ConfigureValidationResult(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ValidationResult>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RuleId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(x => x.SourceText)
                .HasMaxLength(4000);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.LeaseId,
                x.RuleId
            });
        });
    }

    private static void ConfigureIssue(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(x => x.ConditionAssessment)
                .HasMaxLength(2000);

            entity.Property(x => x.Confidence)
                .HasPrecision(5, 4);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Unit)
                .WithMany(x => x.Issues)
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Images)
                .WithOne(x => x.Issue)
                .HasForeignKey(x => x.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.WorkOrders)
                .WithOne(x => x.Issue)
                .HasForeignKey(x => x.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UnitId);
        });
    }

    private static void ConfigureIssueImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IssueImage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(1000);
        });
    }

    private static void ConfigureWorkOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Unit)
                .WithMany()
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.UnitId,
                x.Status
            });
        });
    }

    private static void ConfigureReviewAction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReviewAction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.PreviousValue)
                .HasMaxLength(4000);

            entity.Property(x => x.NewValue)
                .HasMaxLength(4000);

            entity.Property(x => x.Reason)
                .HasMaxLength(4000);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.EntityType,
                x.EntityId
            });
        });
    }
}
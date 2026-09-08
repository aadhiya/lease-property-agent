namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Synchronizes the configured property/unit catalog into the application's
/// relational database so extracted leases can be linked to real units.
/// </summary>
public interface IPropertyCatalogSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
namespace LeasePropertyAgent.Application.Models.Workspace;

public class UnitWorkspaceResponse
{
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string? UnitType { get; set; }
    public decimal? Area { get; set; }
    public string Status { get; set; } = string.Empty;

    public BuildingWorkspaceResponse? Building { get; set; }
    public PropertyWorkspaceResponse? Property { get; set; }

    public List<LeaseWorkspaceResponse> Leases { get; set; } = new();
    public List<IssueWorkspaceResponse> Issues { get; set; } = new();
}

public class BuildingWorkspaceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PropertyWorkspaceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
}
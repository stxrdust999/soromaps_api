using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class Report
{
    public Guid Id { get; set; }

    public Guid ReporterId { get; set; }
    public User Reporter { get; set; } = null!;

    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public Guid? ResolvedById { get; set; }
    public User? ResolvedBy { get; set; }

    public string? ResolutionNote { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

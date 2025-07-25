using System.ComponentModel.DataAnnotations;

namespace GestorTeocratico.Entities;

/// <summary>
/// Represents the configuration for assigning a responsibility to a meeting type,
/// including the quantity and related entities.
/// </summary>
public class ResponsibilityAssignmentConfig
{
    public required Guid ResponsibilityId { get; set; }
    public required Guid MeetingTypeId { get; set; }
    [Range(1, int.MaxValue)] public int Quantity { get; set; } = 1;

    public required Responsibility Responsibility { get; set; }
    public required MeetingType MeetingType { get; set; }
}
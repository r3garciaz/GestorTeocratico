using System.ComponentModel.DataAnnotations;

namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a type of meeting with its properties.
/// </summary>
public class MeetingType
{
    public Guid MeetingTypeId { get; set; } = Guid.CreateVersion7();
    [MaxLength(250)] public required string Name { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public required MeetingType Type { get; set; }
}
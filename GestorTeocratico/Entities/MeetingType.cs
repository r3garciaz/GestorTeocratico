using System.ComponentModel.DataAnnotations;

namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a type of meeting with its properties.
/// </summary>
public class MeetingType : SoftDeleteEntity
{
    public Guid MeetingTypeId { get; set; } = Guid.CreateVersion7();
    [MaxLength(250)] public required string Name { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public required Shared.Enums.MeetingType Type { get; set; }
}
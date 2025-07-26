namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a department within a congregation, including its properties and relationships.
/// </summary>
public class Department : SoftDeleteEntity
{
    public Guid DepartmentId { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public Guid? ResponsiblePublisherId { get; set; }
    
    public Publisher? ResponsiblePublisher { get; set; }
    public ICollection<Responsibility> Responsibilities { get; set; } = [];
}
namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a responsibility within the system, including its name, description, status, and related department.
/// </summary>
public class Responsibility : SoftDeleteEntity
{
    public Guid ResponsibilityId { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    
    public Department? Department { get; set; }
    public ICollection<PublisherResponsibility> PublisherResponsibilities { get; set; } = [];
}
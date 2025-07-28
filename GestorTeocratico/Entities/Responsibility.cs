namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a responsibility within the system, including its name, description, and status.
/// </summary>
public class Responsibility : SoftDeleteEntity
{
    public Guid ResponsibilityId { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    
    public Department? Department { get; set; }
    public ICollection<PublisherResponsibility> PublisherResponsibilities { get; set; } = [];
}
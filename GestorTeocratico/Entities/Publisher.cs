using System.ComponentModel.DataAnnotations;
using GestorTeocratico.Shared.Enums;

namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a publisher entity with personal and congregation-related information.
/// </summary>
public class Publisher : SoftDeleteEntity
{
    public Guid PublisherId { get; set; } = Guid.CreateVersion7();
    [MaxLength(250)] public string FirstName { get; set; }
    [MaxLength(250)] public string LastName { get; set; }
    [MaxLength(250)] public string? MotherLastName { get; set; }
    [DataType(DataType.PhoneNumber)] public string? Phone { get; set; }
    [DataType(DataType.EmailAddress)] public string? Email { get; set; }
    public Gender Gender { get; set; }
    public Privilege? Privilege { get; set; }

    public ICollection<Department> ResponsibleDepartments { get; set; } = [];
    public ICollection<PublisherResponsibility> PublisherResponsibilities { get; set; } = [];
}
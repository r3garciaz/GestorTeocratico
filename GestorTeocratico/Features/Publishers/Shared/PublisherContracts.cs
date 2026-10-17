using GestorTeocratico.Shared.Enums;

namespace GestorTeocratico.Features.Publishers;

/// <summary>Read model returned to the UI. Never exposes EF entities.</summary>
public sealed record PublisherDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MotherLastName,
    string? Phone,
    string? Email,
    Gender Gender,
    Privilege? Privilege,
    IReadOnlyList<Guid> ResponsibilityIds,
    IReadOnlyList<Guid> DepartmentIds,
    IReadOnlyList<string> ResponsibilityNames,
    IReadOnlyList<string> DepartmentNames);

/// <summary>Mutable model bound to the create/edit form.</summary>
public sealed class PublisherFormModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MotherLastName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Gender Gender { get; set; } = Gender.Male;
    public Privilege? Privilege { get; set; }
}

public sealed record ResponsibilityOptionDto(Guid Id, string Name, string? DepartmentName);

public sealed record DepartmentOptionDto(Guid Id, string Name);

public sealed record PublisherFormOptions(
    IReadOnlyList<ResponsibilityOptionDto> Responsibilities,
    IReadOnlyList<DepartmentOptionDto> Departments);

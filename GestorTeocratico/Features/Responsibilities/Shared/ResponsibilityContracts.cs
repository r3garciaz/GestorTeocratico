namespace GestorTeocratico.Features.Responsibilities;

/// <summary>Read model returned to the UI. Never exposes EF entities.</summary>
public sealed record ResponsibilityDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? DepartmentId,
    string? DepartmentName,
    IReadOnlyList<Guid> PublisherIds,
    IReadOnlyList<string> PublisherNames);

/// <summary>Mutable model bound to the create/edit form.</summary>
public sealed class ResponsibilityFormModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
}

public sealed record ResponsibilityDepartmentOptionDto(Guid Id, string Name);

public sealed record ResponsibilityPublisherOptionDto(Guid Id, string Name);

public sealed record ResponsibilityFormOptions(
    IReadOnlyList<ResponsibilityDepartmentOptionDto> Departments,
    IReadOnlyList<ResponsibilityPublisherOptionDto> Publishers);

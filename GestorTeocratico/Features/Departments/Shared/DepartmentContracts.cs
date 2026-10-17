namespace GestorTeocratico.Features.Departments;

/// <summary>Read model returned to the UI. Never exposes EF entities.</summary>
public sealed record DepartmentDto(
    Guid Id,
    string Name,
    Guid? ResponsiblePublisherId,
    string? ResponsiblePublisherName,
    IReadOnlyList<Guid> ResponsibilityIds,
    IReadOnlyList<string> ResponsibilityNames);

/// <summary>Mutable model bound to the create/edit form.</summary>
public sealed class DepartmentFormModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ResponsiblePublisherId { get; set; }
}

public sealed record DepartmentResponsibilityOptionDto(Guid Id, string Name);

public sealed record DepartmentPublisherOptionDto(Guid Id, string Name);

public sealed record DepartmentFormOptions(
    IReadOnlyList<DepartmentResponsibilityOptionDto> Responsibilities,
    IReadOnlyList<DepartmentPublisherOptionDto> Publishers);

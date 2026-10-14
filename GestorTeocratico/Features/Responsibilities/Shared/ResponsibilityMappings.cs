using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Responsibilities;

internal static class ResponsibilityMappings
{
    public static ResponsibilityDto ToDto(this Responsibility responsibility) => new(
        responsibility.ResponsibilityId,
        responsibility.Name,
        responsibility.Description,
        responsibility.DepartmentId,
        responsibility.Department?.Name,
        responsibility.PublisherResponsibilities.Select(pr => pr.PublisherId).ToList(),
        responsibility.PublisherResponsibilities
            .Where(pr => pr.Publisher is not null)
            .OrderBy(pr => pr.Publisher!.LastName)
            .ThenBy(pr => pr.Publisher!.MotherLastName)
            .ThenBy(pr => pr.Publisher!.FirstName)
            .Select(pr => PublisherDisplayName(pr.Publisher!))
            .ToList());

    public static string PublisherDisplayName(Publisher publisher)
    {
        var name = publisher.FirstName;
        if (!string.IsNullOrEmpty(publisher.LastName)) name += $" {publisher.LastName}";
        if (!string.IsNullOrEmpty(publisher.MotherLastName)) name += $" {publisher.MotherLastName}";
        return name;
    }
}

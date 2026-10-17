using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Publishers;

internal static class PublisherMappings
{
    public static PublisherDto ToDto(this Publisher publisher) => new(
        publisher.PublisherId,
        publisher.FirstName,
        publisher.LastName,
        publisher.MotherLastName,
        publisher.Phone,
        publisher.Email,
        publisher.Gender,
        publisher.Privilege,
        publisher.PublisherResponsibilities.Select(pr => pr.ResponsibilityId).ToList(),
        publisher.ResponsibleDepartments.Select(d => d.DepartmentId).ToList(),
        publisher.PublisherResponsibilities
            .Select(pr => pr.Responsibility?.Name ?? string.Empty)
            .Where(name => name.Length > 0)
            .OrderBy(name => name)
            .ToList(),
        publisher.ResponsibleDepartments
            .Select(d => d.Name)
            .OrderBy(name => name)
            .ToList());
}

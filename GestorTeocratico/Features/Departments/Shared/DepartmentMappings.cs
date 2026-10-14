using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Departments;

internal static class DepartmentMappings
{
    public static DepartmentDto ToDto(this Department department) => new(
        department.DepartmentId,
        department.Name,
        department.ResponsiblePublisherId,
        department.ResponsiblePublisher is null
            ? null
            : PublisherDisplayName(department.ResponsiblePublisher),
        department.Responsibilities.Select(r => r.ResponsibilityId).ToList(),
        department.Responsibilities.Select(r => r.Name).OrderBy(name => name).ToList());

    public static string PublisherDisplayName(Publisher publisher)
    {
        var name = publisher.FirstName;
        if (!string.IsNullOrEmpty(publisher.LastName)) name += $" {publisher.LastName}";
        if (!string.IsNullOrEmpty(publisher.MotherLastName)) name += $" {publisher.MotherLastName}";
        return name;
    }
}

using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

/// <summary>
/// Reconciles a publisher's responsibilities (N:N) and responsible departments (1:N) using
/// only foreign keys, so related Publisher/Responsibility/Department entities are never
/// accidentally marked as added or modified.
/// </summary>
internal static class PublisherAssignmentSync
{
    public static async Task SyncAsync(
        ApplicationDbContext context,
        Guid publisherId,
        IReadOnlyCollection<Guid> responsibilityIds,
        IReadOnlyCollection<Guid> departmentIds,
        CancellationToken cancellationToken)
    {
        var selectedResponsibilities = responsibilityIds.ToHashSet();
        var currentResponsibilities = await context.PublisherResponsibilities
            .Where(pr => pr.PublisherId == publisherId)
            .ToListAsync(cancellationToken);

        context.PublisherResponsibilities.RemoveRange(
            currentResponsibilities.Where(pr => !selectedResponsibilities.Contains(pr.ResponsibilityId)));

        var existingResponsibilities = currentResponsibilities.Select(pr => pr.ResponsibilityId).ToHashSet();
        foreach (var responsibilityId in selectedResponsibilities.Where(id => !existingResponsibilities.Contains(id)))
        {
            context.PublisherResponsibilities.Add(new PublisherResponsibility
            {
                PublisherId = publisherId,
                ResponsibilityId = responsibilityId
            });
        }

        var selectedDepartments = departmentIds.ToHashSet();
        var currentDepartments = await context.Departments
            .Where(d => d.ResponsiblePublisherId == publisherId)
            .ToListAsync(cancellationToken);

        foreach (var department in currentDepartments.Where(d => !selectedDepartments.Contains(d.DepartmentId)))
        {
            department.ResponsiblePublisherId = null;
        }

        var toAssign = await context.Departments
            .Where(d => selectedDepartments.Contains(d.DepartmentId))
            .ToListAsync(cancellationToken);

        foreach (var department in toAssign)
        {
            department.ResponsiblePublisherId = publisherId;
        }
    }
}

using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

/// <summary>
/// Reconciles the publishers assigned to a responsibility (N:N) using only foreign keys, so
/// related Publisher/Responsibility entities are never accidentally marked as added or modified.
/// </summary>
internal static class ResponsibilityPublisherSync
{
    public static async Task SyncAsync(
        ApplicationDbContext context,
        Guid responsibilityId,
        IReadOnlyCollection<Guid> publisherIds,
        CancellationToken cancellationToken)
    {
        var selected = publisherIds.ToHashSet();

        var current = await context.PublisherResponsibilities
            .Where(pr => pr.ResponsibilityId == responsibilityId)
            .ToListAsync(cancellationToken);

        context.PublisherResponsibilities.RemoveRange(
            current.Where(pr => !selected.Contains(pr.PublisherId)));

        var existing = current.Select(pr => pr.PublisherId).ToHashSet();
        foreach (var publisherId in selected.Where(id => !existing.Contains(id)))
        {
            context.PublisherResponsibilities.Add(new PublisherResponsibility
            {
                ResponsibilityId = responsibilityId,
                PublisherId = publisherId
            });
        }
    }
}

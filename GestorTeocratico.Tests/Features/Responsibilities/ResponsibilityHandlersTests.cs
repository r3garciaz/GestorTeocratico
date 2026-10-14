using GestorTeocratico.Features.Common;
using GestorTeocratico.Features.Publishers;
using GestorTeocratico.Features.Responsibilities;
using GestorTeocratico.Shared.Enums;
using GestorTeocratico.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GestorTeocratico.Tests.Features.Responsibilities;

public class ResponsibilityHandlersTests
{
    private static CreateResponsibilityHandler Create(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreateResponsibilityHandler>.Instance);

    [Fact]
    public async Task CreateHandler_duplicate_name_in_same_department_throws()
    {
        using var factory = new SqliteTestDbContextFactory();
        var create = Create(factory);

        await create.HandleAsync(new CreateResponsibilityCommand("Oración", null, null, []));

        await Assert.ThrowsAsync<DuplicateEntityException>(() =>
            create.HandleAsync(new CreateResponsibilityCommand("oración", null, null, [])));
    }

    [Fact]
    public async Task CreateHandler_persists_publisher_assignments()
    {
        using var factory = new SqliteTestDbContextFactory();
        var create = Create(factory);
        var createPublisher = new CreatePublisherHandler(factory, NullLogger<CreatePublisherHandler>.Instance);
        var getById = new GetResponsibilityByIdHandler(factory);

        var publisherId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Sara", "Nunez", null, null, null, Gender.Female, null, [], []));

        var responsibilityId = await create.HandleAsync(
            new CreateResponsibilityCommand("Micrófonos", "Sonido", null, new[] { publisherId }));

        var loaded = await getById.HandleAsync(responsibilityId);

        Assert.NotNull(loaded);
        Assert.Contains(publisherId, loaded!.PublisherIds);
    }

    [Fact]
    public async Task DeleteHandler_soft_deletes_and_excludes_from_list()
    {
        using var factory = new SqliteTestDbContextFactory();
        var create = Create(factory);
        var delete = new DeleteResponsibilityHandler(factory);

        var id = await create.HandleAsync(new CreateResponsibilityCommand("Anuncios", null, null, []));

        Assert.True(await delete.HandleAsync(id));
        Assert.False(await delete.HandleAsync(Guid.CreateVersion7()));

        var all = await new GetResponsibilitiesHandler(factory).HandleAsync();
        Assert.DoesNotContain(all, r => r.Id == id);
    }
}

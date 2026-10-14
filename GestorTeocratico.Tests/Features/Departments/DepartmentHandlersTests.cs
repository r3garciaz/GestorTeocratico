using GestorTeocratico.Features.Common;
using GestorTeocratico.Features.Departments;
using GestorTeocratico.Features.Responsibilities;
using GestorTeocratico.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GestorTeocratico.Tests.Features.Departments;

public class DepartmentHandlersTests
{
    private static CreateDepartmentHandler Create(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreateDepartmentHandler>.Instance);

    private static CreateResponsibilityHandler CreateResponsibility(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreateResponsibilityHandler>.Instance);

    [Fact]
    public async Task CreateHandler_duplicate_name_throws_duplicate_entity_exception()
    {
        using var factory = new SqliteTestDbContextFactory();
        var create = Create(factory);

        await create.HandleAsync(new CreateDepartmentCommand("Anuncios", null, []));

        await Assert.ThrowsAsync<DuplicateEntityException>(() =>
            create.HandleAsync(new CreateDepartmentCommand("anuncios", null, [])));
    }

    [Fact]
    public async Task DeleteHandler_soft_deletes_and_hides_entity_from_list()
    {
        using var factory = new SqliteTestDbContextFactory();
        var create = Create(factory);
        var delete = new DeleteDepartmentHandler(factory);

        var id = await create.HandleAsync(new CreateDepartmentCommand("Limpieza", null, []));

        Assert.True(await delete.HandleAsync(id));
        Assert.False(await delete.HandleAsync(Guid.CreateVersion7()));

        var all = await new GetDepartmentsHandler(factory).HandleAsync();
        Assert.DoesNotContain(all, d => d.Id == id);

        // The row must still exist physically (soft delete).
        await using var context = factory.CreateDbContext();
        var stillThere = await context.Departments
            .IgnoreQueryFilters()
            .AnyAsync(d => d.DepartmentId == id);
        Assert.True(stillThere);
    }

    [Fact]
    public async Task UpdateHandler_does_not_modify_related_responsibilities()
    {
        using var factory = new SqliteTestDbContextFactory();
        var create = Create(factory);
        var getById = new GetDepartmentByIdHandler(factory);
        var update = new UpdateDepartmentHandler(factory);
        var createResponsibility = CreateResponsibility(factory);
        var getResponsibility = new GetResponsibilityByIdHandler(factory);

        var responsibilityId = await createResponsibility.HandleAsync(
            new CreateResponsibilityCommand("Micrófonos", null, null, []));

        var id = await create.HandleAsync(new CreateDepartmentCommand(
            "Audio", null, new[] { responsibilityId }));

        var loaded = await getById.HandleAsync(id);
        Assert.NotNull(loaded);
        await update.HandleAsync(new UpdateDepartmentCommand(
            loaded!.Id, "Audio y Video", loaded.ResponsiblePublisherId, loaded.ResponsibilityIds));

        var responsibilityAfter = await getResponsibility.HandleAsync(responsibilityId);
        Assert.Equal("Micrófonos", responsibilityAfter!.Name);
        Assert.Equal(id, responsibilityAfter.DepartmentId);

        var refreshed = await getById.HandleAsync(id);
        Assert.Equal("Audio y Video", refreshed!.Name);
    }
}

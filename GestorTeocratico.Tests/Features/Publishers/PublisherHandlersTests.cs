using GestorTeocratico.Features.Common;
using GestorTeocratico.Features.Departments;
using GestorTeocratico.Features.Publishers;
using GestorTeocratico.Features.Responsibilities;
using GestorTeocratico.Shared.Enums;
using GestorTeocratico.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GestorTeocratico.Tests.Features.Publishers;

public class PublisherHandlersTests
{
    private static CreatePublisherHandler CreatePublisher(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreatePublisherHandler>.Instance);

    private static CreateDepartmentHandler CreateDepartment(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreateDepartmentHandler>.Instance);

    private static CreateResponsibilityHandler CreateResponsibility(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreateResponsibilityHandler>.Instance);

    [Fact]
    public async Task UpdateHandler_does_not_modify_related_responsibilities_or_departments()
    {
        using var factory = new SqliteTestDbContextFactory();
        var getById = new GetPublisherByIdHandler(factory);
        var update = new UpdatePublisherHandler(factory);
        var createPublisher = CreatePublisher(factory);
        var createDepartment = CreateDepartment(factory);
        var createResponsibility = CreateResponsibility(factory);
        var getDepartment = new GetDepartmentByIdHandler(factory);
        var getResponsibility = new GetResponsibilityByIdHandler(factory);

        var responsibilityId = await createResponsibility.HandleAsync(
            new CreateResponsibilityCommand("Lectura de la Biblia", null, null, []));

        var departmentId = await createDepartment.HandleAsync(
            new CreateDepartmentCommand("Escuela Bíblica", null, []));

        var publisherId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Juan", "Perez", null, null, null, Gender.Male, null,
            new[] { responsibilityId },
            new[] { departmentId }));

        // Act: simulate the UI edit flow (load, change a scalar, update).
        var loaded = await getById.HandleAsync(publisherId);
        Assert.NotNull(loaded);
        await update.HandleAsync(new UpdatePublisherCommand(
            loaded!.Id, "Juan Carlos", loaded.LastName, loaded.MotherLastName, loaded.Phone, loaded.Email,
            loaded.Gender, loaded.Privilege,
            loaded.ResponsibilityIds,
            loaded.DepartmentIds));

        // Assert: related entities were not overwritten.
        var responsibilityAfter = await getResponsibility.HandleAsync(responsibilityId);
        Assert.Equal("Lectura de la Biblia", responsibilityAfter!.Name);

        var departmentAfter = await getDepartment.HandleAsync(departmentId);
        Assert.Equal("Escuela Bíblica", departmentAfter!.Name);

        var refreshed = await getById.HandleAsync(publisherId);
        Assert.Equal("Juan Carlos", refreshed!.FirstName);
        Assert.Contains(responsibilityId, refreshed.ResponsibilityIds);
        Assert.Contains(departmentId, refreshed.DepartmentIds);
    }

    [Fact]
    public async Task CreateHandler_persists_assignments()
    {
        using var factory = new SqliteTestDbContextFactory();
        var createPublisher = CreatePublisher(factory);
        var createDepartment = CreateDepartment(factory);
        var createResponsibility = CreateResponsibility(factory);
        var getById = new GetPublisherByIdHandler(factory);

        var responsibilityId = await createResponsibility.HandleAsync(
            new CreateResponsibilityCommand("Oración", null, null, []));
        var departmentId = await createDepartment.HandleAsync(
            new CreateDepartmentCommand("Predicación", null, []));

        var publisherId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Ana", "Lopez", null, null, null, Gender.Female, null,
            new[] { responsibilityId },
            new[] { departmentId }));

        var loaded = await getById.HandleAsync(publisherId);

        Assert.NotNull(loaded);
        Assert.Contains(responsibilityId, loaded!.ResponsibilityIds);
        Assert.Contains(departmentId, loaded.DepartmentIds);
    }

    [Fact]
    public async Task CreateHandler_duplicate_name_throws_duplicate_entity_exception()
    {
        using var factory = new SqliteTestDbContextFactory();
        var createPublisher = CreatePublisher(factory);

        await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Pedro", "Gomez", null, null, null, Gender.Male, null, [], []));

        await Assert.ThrowsAsync<DuplicateEntityException>(() =>
            createPublisher.HandleAsync(new CreatePublisherCommand(
                "pedro", "gomez", null, null, null, Gender.Male, null, [], [])));
    }

    [Fact]
    public async Task DeleteHandler_soft_deletes_and_excludes_from_list()
    {
        using var factory = new SqliteTestDbContextFactory();
        var createPublisher = CreatePublisher(factory);
        var getPublishers = new GetPublishersHandler(factory);
        var delete = new DeletePublisherHandler(factory);

        var publisherId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Marta", "Ruiz", null, null, null, Gender.Female, null, [], []));

        var deleted = await delete.HandleAsync(publisherId);
        Assert.True(deleted);

        var all = await getPublishers.HandleAsync();
        Assert.DoesNotContain(all, p => p.Id == publisherId);

        Assert.False(await delete.HandleAsync(Guid.CreateVersion7()));
    }
}

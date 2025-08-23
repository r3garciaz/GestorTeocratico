using GestorTeocratico.Entities;
using GestorTeocratico.Shared;
using GestorTeocratico.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Data;

public static class DataSeederDevelopment
{
    // Fixed UUIDs for consistent seeding
    private static readonly Guid CongregationId = new("01934567-89AB-7DEF-8012-3456789ABCDE");

    // Department IDs
    private static readonly Guid AudioVisualDeptId = new("01934567-89AB-7DEF-8012-3456789ABCD1");
    private static readonly Guid AccommodationDeptId = new("01934567-89AB-7DEF-8012-3456789ABCD2");
    private static readonly Guid PublicationId = new("01934567-89AB-7DEF-8012-3456789ABCD3");
    // private static readonly Guid MicrophoneDeptId = new("01934567-89AB-7DEF-8012-3456789ABCD4");

    // Responsibility IDs
    private static readonly Guid[] ResponsibilityIds =
    [
        new("01934567-89AB-7DEF-8012-3456789ABC10"),
        new("01934567-89AB-7DEF-8012-3456789ABC11"),
        new("01934567-89AB-7DEF-8012-3456789ABC12"),
        new("01934567-89AB-7DEF-8012-3456789ABC13"),
        new("01934567-89AB-7DEF-8012-3456789ABC14"),
        new("01934567-89AB-7DEF-8012-3456789ABC15"),
        new("01934567-89AB-7DEF-8012-3456789ABC16"),
        new("01934567-89AB-7DEF-8012-3456789ABC17"),
        // new("01934567-89AB-7DEF-8012-3456789ABC18")
    ];

    // Publisher IDs - starting from 100
    private static readonly Guid[] PublisherIds =
    [
        new("01934567-89AB-7DEF-8012-34567890A100"),
        new("01934567-89AB-7DEF-8012-34567890A101"),
        new("01934567-89AB-7DEF-8012-34567890A102"),
        new("01934567-89AB-7DEF-8012-34567890A103"),
        new("01934567-89AB-7DEF-8012-34567890A104"),
        new("01934567-89AB-7DEF-8012-34567890A105"),
        new("01934567-89AB-7DEF-8012-34567890A106"),
        new("01934567-89AB-7DEF-8012-34567890A107"),
        new("01934567-89AB-7DEF-8012-34567890A108"),
        new("01934567-89AB-7DEF-8012-34567890A109"),
        new("01934567-89AB-7DEF-8012-34567890A110"),
        new("01934567-89AB-7DEF-8012-34567890A111"),
        new("01934567-89AB-7DEF-8012-34567890A112"),
        new("01934567-89AB-7DEF-8012-34567890A113"),
        new("01934567-89AB-7DEF-8012-34567890A114"),
        new("01934567-89AB-7DEF-8012-34567890A115"),
        new("01934567-89AB-7DEF-8012-34567890A116"),
        new("01934567-89AB-7DEF-8012-34567890A117"),
        new("01934567-89AB-7DEF-8012-34567890A118"),
        new("01934567-89AB-7DEF-8012-34567890A119"),
        new("01934567-89AB-7DEF-8012-34567890A120"),
        new("01934567-89AB-7DEF-8012-34567890A121"),
        new("01934567-89AB-7DEF-8012-34567890A122"),
        new("01934567-89AB-7DEF-8012-34567890A123"),
        new("01934567-89AB-7DEF-8012-34567890A124"),
        new("01934567-89AB-7DEF-8012-34567890A125"),
        new("01934567-89AB-7DEF-8012-34567890A126"),
        new("01934567-89AB-7DEF-8012-34567890A127"),
        new("01934567-89AB-7DEF-8012-34567890A128"),
        new("01934567-89AB-7DEF-8012-34567890A129"),
        new("01934567-89AB-7DEF-8012-34567890A130")
    ];

    // Role IDs
    private static readonly Guid[] RoleIds =
    [
        new("0198c2ae-6d60-77d7-9bfa-791a8abe8a0e"), // Admin
        new("0198c2ae-6d61-77ba-8039-408b8c3d31ef"), // Manager
        new("0198c2ae-6d61-77ba-8039-44037008daa7")  // User
    ];

    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        await SeedCongregationAsync(context);
        await SeedDepartmentsAsync(context);
        await SeedResponsibilitiesAsync(context);
        await SeedPublishersAsync(context);
        await SeedPublisherResponsibilitiesAsync(context);
        await SeedRolesAsync(context);
    }

    private static async Task SeedCongregationAsync(ApplicationDbContext context)
    {
        // If any congregation exists, do not seed another.
        if (await context.Congregations.AnyAsync()) return;

        var congregation = new Congregation
        {
            CongregationId = CongregationId,
            Name = "Nombre Congregación",
            MidweekMeetingDayEvenYear = DayOfWeek.Wednesday,
            MidweekMeetingDayOddYear = DayOfWeek.Thursday,
            WeekendMeetingDayEvenYear = DayOfWeek.Saturday,
            WeekendMeetingDayOddYear = DayOfWeek.Sunday,
            Address = "Dirección Salón del Reino",
            City = "Ciudad"
        };

        context.Congregations.Add(congregation);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDepartmentsAsync(ApplicationDbContext context)
    {
        var departmentsInDatabase = await context.Departments.IgnoreQueryFilters().ToDictionaryAsync(d => d.DepartmentId);

        var departments = new[]
        {
            new { Id = AudioVisualDeptId, Name = "Audio y Video" },
            new { Id = AccommodationDeptId, Name = "Acomodación" },
            new { Id = PublicationId, Name = "Publicaciones"}
            // new { Id = PlatformDeptId, Name = "Plataforma" },
            // new { Id = MicrophoneDeptId, Name = "Micrófonos" }
        };

        foreach (var dept in departments)
        {
            if (!departmentsInDatabase.ContainsKey(dept.Id))
            {
                context.Departments.Add(new Department
                {
                    DepartmentId = dept.Id,
                    Name = dept.Name
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedResponsibilitiesAsync(ApplicationDbContext context)
    {
        var responsibilitiesInDatabase = await context.Responsibilities.IgnoreQueryFilters().ToDictionaryAsync(r => r.ResponsibilityId);

        var responsibilities = new[]
        {
            new { Id = ResponsibilityIds[0], Name = "Audio", DepartmentId = AudioVisualDeptId },
            new { Id = ResponsibilityIds[1], Name = "Video", DepartmentId = AudioVisualDeptId },
            new { Id = ResponsibilityIds[2], Name = "Micrófono 1", DepartmentId = AudioVisualDeptId },
            new { Id = ResponsibilityIds[3], Name = "Micrófono 2", DepartmentId = AudioVisualDeptId },
            new { Id = ResponsibilityIds[4], Name = "Plataforma", DepartmentId = AudioVisualDeptId },
            new { Id = ResponsibilityIds[5], Name = "Acomodador Zoom", DepartmentId = AccommodationDeptId },
            new { Id = ResponsibilityIds[6], Name = "Acomodador Entrada", DepartmentId = AccommodationDeptId },
            new { Id = ResponsibilityIds[7], Name = "Acomodador Auditorio", DepartmentId = AccommodationDeptId }
        };

        foreach (var resp in responsibilities)
        {
            if (!responsibilitiesInDatabase.ContainsKey(resp.Id))
            {
                context.Responsibilities.Add(new Responsibility
                {
                    ResponsibilityId = resp.Id,
                    Name = resp.Name,
                    DepartmentId = resp.DepartmentId
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPublishersAsync(ApplicationDbContext context)
    {
        var publishersInDatabase = await context.Publishers.IgnoreQueryFilters().ToDictionaryAsync(p => p.PublisherId);
        var publisherData = new[]
        {
            new { Id = PublisherIds[0], FirstName = "Ivan", LastName = "Valenzuela" },
            new { Id = PublisherIds[1], FirstName = "Lee", LastName = "Alfaro" },
            new { Id = PublisherIds[2], FirstName = "Hector", LastName = "Lara" },
            new { Id = PublisherIds[3], FirstName = "Abraham", LastName = "Hoyos" },
            new { Id = PublisherIds[4], FirstName = "Miguel", LastName = "Reyes" },
            new { Id = PublisherIds[5], FirstName = "Benjamin", LastName = "Lagos" },
            new { Id = PublisherIds[6], FirstName = "Daniel", LastName = "Toledo" },
            new { Id = PublisherIds[7], FirstName = "Juan C.", LastName = "Aguilera" },
            new { Id = PublisherIds[8], FirstName = "Felipe", LastName = "Mariño" },
            new { Id = PublisherIds[9], FirstName = "Santiago", LastName = "Echaverria" },
            new { Id = PublisherIds[10], FirstName = "David", LastName = "Aguirre" },
            new { Id = PublisherIds[11], FirstName = "Javier", LastName = "Yañez" },
            new { Id = PublisherIds[12], FirstName = "Miguel", LastName = "Celis" },
            new { Id = PublisherIds[13], FirstName = "Mario", LastName = "Leiva" },
            new { Id = PublisherIds[14], FirstName = "Luis", LastName = "Olivos" },
            new { Id = PublisherIds[15], FirstName = "Pablo", LastName = "Poblete" },
            new { Id = PublisherIds[16], FirstName = "Orlando", LastName = "Rubio" },
            new { Id = PublisherIds[17], FirstName = "Jonathan", LastName = "Piñones" },
            new { Id = PublisherIds[18], FirstName = "Cristian", LastName = "Alvarez" },
            new { Id = PublisherIds[19], FirstName = "Matias", LastName = "Garcia" },
            new { Id = PublisherIds[20], FirstName = "Christopher", LastName = "Vargas" },
            new { Id = PublisherIds[21], FirstName = "Edil", LastName = "Gonzalez" },
            new { Id = PublisherIds[22], FirstName = "Gustavo", LastName = "Leiva" },
            new { Id = PublisherIds[23], FirstName = "Nikolas", LastName = "Diaz" },
            new { Id = PublisherIds[24], FirstName = "Jeremias", LastName = "Alvarez" },
            new { Id = PublisherIds[25], FirstName = "Patricio", LastName = "Alcalde" },
            new { Id = PublisherIds[26], FirstName = "Jose", LastName = "Barrionuevo" },
            new { Id = PublisherIds[27], FirstName = "Joaquin", LastName = "Vega" },
            new { Id = PublisherIds[28], FirstName = "Jose", LastName = "Ramirez" },
            new { Id = PublisherIds[29], FirstName = "Reinaldo", LastName = "Garcia" },
            new { Id = PublisherIds[30], FirstName = "Juan Carlos", LastName = "Aguilera" }
        };

        foreach (var pubData in publisherData)
        {
            if (!publishersInDatabase.ContainsKey(pubData.Id))
            {
                context.Publishers.Add(new Publisher
                {
                    PublisherId = pubData.Id,
                    FirstName = pubData.FirstName,
                    LastName = pubData.LastName,
                    Gender = Gender.Male
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPublisherResponsibilitiesAsync(ApplicationDbContext context)
    {
        // Check if relationships already exist
        if (await context.PublisherResponsibilities.AnyAsync()) return;

        var publisherResponsibilities = new List<PublisherResponsibility>();

        // Assign all publishers to all responsibilities for testing
        foreach (var publisherId in PublisherIds)
        {
            foreach (var responsibilityId in ResponsibilityIds)
            {
                publisherResponsibilities.Add(new PublisherResponsibility
                {
                    PublisherId = publisherId,
                    ResponsibilityId = responsibilityId
                });
            }
        }

        context.PublisherResponsibilities.AddRange(publisherResponsibilities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var roles = new[]
        {
            new IdentityRole
            {
                Id = RoleIds[0].ToString(),
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpperInvariant()
            },
            new IdentityRole
            {
                Id = RoleIds[1].ToString(),
                Name = Roles.Manager,
                NormalizedName = Roles.Manager.ToUpperInvariant()
            },
            new IdentityRole
            {
                Id = RoleIds[2].ToString(),
                Name = Roles.User,
                NormalizedName = Roles.User.ToUpperInvariant()
            }
        };

        var currentRoles = await context.Roles.ToListAsync();

        foreach (var roleName in roles)
        {
            if (currentRoles.All(r => r.Name != roleName.Name))
            {
                context.Roles.Add(roleName);
            }
        }

        await context.SaveChangesAsync();
    }
}

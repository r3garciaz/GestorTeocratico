using GestorTeocratico.Entities;
using GestorTeocratico.Shared;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestorTeocratico.Data;

public static class DataSeeder
{
    // Congregation ID
    private static readonly Guid CongregationId = new("01989b36-6035-7569-9eab-442b105964e7");
    
    // Department IDs
    private static readonly Guid SoundAndVideoId = new("01989b46-cd58-77bc-9920-2af8662ed1fb");
    private static readonly Guid AttendantId = new("01989b46-cd58-77bc-9920-2fbd2ad71a73");
    private static readonly Guid LiteratureId = new("01989b46-cd58-77bc-9920-2fbd2ad71a74");

    private static readonly Guid[] ResponsibilityIds =
    [
        new("01989b49-0575-76ae-9ab3-d44df6865dd2"),
        new("01989b49-0575-76ae-9ab3-db9ecb5992d9"),
        new("01989b49-0575-76ae-9ab3-dff1289486df"),
        new("01989b49-0575-76ae-9ab3-e1af42091182"),
        new("01989b49-0575-76ae-9ab3-e5a6ea861baa"),
        new ("01989b4e-93f7-70df-bd7f-836aabcdf1a7"),
        new ("01989b4e-93f7-70df-bd7f-876c8b421412"),
        new ("01989b4e-93f7-70df-bd7f-8a4e667e9633")
    ];
    
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
        await SeedRolesAsync(context);
    }
    
    private static async Task SeedCongregationAsync(ApplicationDbContext context)
    {
    // If any congregation exists, do not seed another. Enforce singleton in seeding.
    if (await context.Congregations.AnyAsync()) return;

        var congregation = new Congregation
        {
            CongregationId = CongregationId,
            Name = "Congregacion",
            MidweekMeetingDayEvenYear = DayOfWeek.Wednesday,
            MidweekMeetingDayOddYear = DayOfWeek.Wednesday,
            WeekendMeetingDayEvenYear = DayOfWeek.Sunday,
            WeekendMeetingDayOddYear = DayOfWeek.Sunday,
            Address = null,
            City = null
        };

        context.Congregations.Add(congregation);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedDepartmentsAsync(ApplicationDbContext context)
    {
        var departmentsInDatabase = await context.Departments.ToDictionaryAsync(d => d.DepartmentId);
        
        var departments = new[]
        {
            new { Id = SoundAndVideoId, Name = "Audio y Video" },
            new { Id = AttendantId, Name = "Acomodación" },
            new { Id = LiteratureId, Name = "Literatura" },
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
        var responsibilitiesInDatabase = await context.Responsibilities.ToDictionaryAsync(r => r.ResponsibilityId);
        
        var responsibilities = new[]
        {
            new { Id = ResponsibilityIds[0], Name = "Audio", DepartmentId = SoundAndVideoId },
            new { Id = ResponsibilityIds[1], Name = "Video", DepartmentId = SoundAndVideoId },
            new { Id = ResponsibilityIds[2], Name = "Micrófono A", DepartmentId = SoundAndVideoId },
            new { Id = ResponsibilityIds[3], Name = "Micrófono B", DepartmentId = SoundAndVideoId },
            new { Id = ResponsibilityIds[4], Name = "Plataforma", DepartmentId = SoundAndVideoId },
            new { Id = ResponsibilityIds[5], Name = "Acomodador Zoom", DepartmentId = SoundAndVideoId },
            new { Id = ResponsibilityIds[6], Name = "Acomodador Entrada", DepartmentId = AttendantId },
            new { Id = ResponsibilityIds[7], Name = "Acomodador Auditorio", DepartmentId = AttendantId }
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

    /// <summary>
    /// Seeds a local "break-glass" administrator account from configuration
    /// (EmergencyAdmin:Email / EmergencyAdmin:Password) so the app can be accessed
    /// with a local account when Google sign-in is unavailable. No-op when unset.
    /// </summary>
    public static async Task SeedEmergencyAdminAsync(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration["EmergencyAdmin:Email"];
        var password = configuration["EmergencyAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PublicId = GeneratePublicId()
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }

        if (!await userManager.IsInRoleAsync(user, Roles.Admin))
        {
            await userManager.AddToRoleAsync(user, Roles.Admin);
        }
    }

    private static string GeneratePublicId()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[8];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        }

        return new string(chars);
    }
}
using System.Linq.Expressions;
using GestorTeocratico.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    
    public DbSet<Congregation> Congregations { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<MeetingSchedule> MeetingSchedules { get; set; }
    public DbSet<MeetingType> MeetingTypes { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<PublisherResponsibility> PublisherResponsibilities { get; set; }
    public DbSet<Responsibility> Responsibilities { get; set; }
    public DbSet<ResponsibilityAssignment> ResponsibilityAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply a global query filter for all entities inheriting from SoftDeleteEntity.
        // This ensures that entities with IsDeleted == true are excluded from queries by default.
        // Also, explicitly sets the IsDeleted property as required for these entities.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Property(parameter, nameof(SoftDeleteEntity.IsDeleted));
                var filter = Expression.Lambda(
                    Expression.Equal(isDeletedProperty, Expression.Constant(false)),
                    parameter
                );
                // Apply global query filter for soft delete
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                // Explicitly set IsDeleted as required for all SoftDeleteEntity types
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(SoftDeleteEntity.IsDeleted))
                    .IsRequired();
            }
        }
        
        modelBuilder.Entity<Congregation>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(250).IsRequired();
            entity.Property(c => c.MidweekMeetingDayEvenYear).IsRequired().HasConversion<string>();
            entity.Property(c => c.MidweekMeetingDayOddYear).IsRequired().HasConversion<string>();
            entity.Property(c => c.WeekendMeetingDayEvenYear).IsRequired().HasConversion<string>();
            entity.Property(c => c.WeekendMeetingDayOddYear).IsRequired().HasConversion<string>();
            entity.Property(p => p.Address).HasMaxLength(500);
            entity.Property(p => p.City).HasMaxLength(250);
        });
        
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(250).IsRequired();

            entity.HasMany(d => d.Responsibilities)
                .WithOne(r => r.Department)
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<MeetingSchedule>(entity =>
        {
            entity.Property(ms => ms.Date).IsRequired();
            entity.Property(ms => ms.Month).IsRequired();
            entity.Property(ms => ms.Year).IsRequired();
            entity.Property(ms => ms.WeekOfYear).IsRequired();
            entity.Property(ms => ms.MeetingTypeId).IsRequired();
            
            entity.HasMany(m => m.ResponsibilityAssignments)
                .WithOne(ra => ra.MeetingSchedule)
                .HasForeignKey(ra => ra.MeetingScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MeetingType>(entity =>
        {
            entity.Property(mt => mt.Name).HasMaxLength(250).IsRequired();
            entity.Property(mt => mt.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.Property(p => p.FirstName).HasMaxLength(250).IsRequired();
            entity.Property(p => p.LastName).HasMaxLength(250);
            entity.Property(p => p.MotherLastName).HasMaxLength(250);
            entity.Property(p => p.Phone).HasMaxLength(15);
            entity.Property(p => p.Email).HasMaxLength(250);
            entity.Property(p => p.Gender).IsRequired();
            entity.Property(p => p.Privilege).IsRequired();
            
            entity.HasMany(p => p.PublisherResponsibilities)
                .WithOne(pr => pr.Publisher)
                .HasForeignKey(pr => pr.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PublisherResponsibility>(entity =>
        {
            entity.HasKey(pr => new { pr.PublisherId, pr.ResponsibilityId });
            entity.HasOne(pr => pr.Publisher)
                .WithMany(p => p.PublisherResponsibilities)
                .HasForeignKey(pr => pr.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pr => pr.Responsibility)
                .WithMany(r => r.PublisherResponsibilities)
                .HasForeignKey(pr => pr.ResponsibilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Responsibility>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(100).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.HasMany(r => r.PublisherResponsibilities)
                .WithOne(pr => pr.Responsibility)
                .HasForeignKey(pr => pr.ResponsibilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResponsibilityAssignment>(entity =>
        {
            entity.HasKey(ra => new { ra.MeetingScheduleId, ra.ResponsibilityId, ra.PublisherId });
            
            entity.HasOne(ra => ra.MeetingSchedule)
                .WithMany(ms => ms.ResponsibilityAssignments)
                .HasForeignKey(ra => ra.MeetingScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(ra => ra.Publisher)
                .WithMany()
                .HasForeignKey(ra => ra.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(ra => ra.Responsibility)
                .WithMany()
                .HasForeignKey(ra => ra.ResponsibilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

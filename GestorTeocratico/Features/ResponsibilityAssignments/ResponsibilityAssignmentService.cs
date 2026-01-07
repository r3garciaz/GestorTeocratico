using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.ResponsibilityAssignments;

public class ResponsibilityAssignmentService : IResponsibilityAssignmentService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<ResponsibilityAssignmentService> _logger;

    public ResponsibilityAssignmentService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<ResponsibilityAssignmentService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResponsibilityAssignments
            .Include(ra => ra.MeetingSchedule)
                .ThenInclude(ms => ms.MeetingType)
            .Include(ra => ra.Publisher)
            .Include(ra => ra.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(ra => ra.MeetingSchedule.Date)
            .ThenBy(ra => ra.Responsibility.Name)
            .ToListAsync();
    }

    public async Task<ResponsibilityAssignment?> GetByIdAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResponsibilityAssignments
            .Include(ra => ra.MeetingSchedule)
                .ThenInclude(ms => ms.MeetingType)
            .Include(ra => ra.Publisher)
            .Include(ra => ra.Responsibility)
                .ThenInclude(r => r.Department)
            .FirstOrDefaultAsync(ra => 
                ra.MeetingScheduleId == meetingScheduleId && 
                ra.ResponsibilityId == responsibilityId && 
                ra.PublisherId == publisherId);
    }

    public async Task<ResponsibilityAssignment> CreateAsync(ResponsibilityAssignment responsibilityAssignment)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Verificar que el MeetingSchedule existe
        var meetingSchedule = await context.MeetingSchedules.FindAsync(responsibilityAssignment.MeetingScheduleId);
        if (meetingSchedule == null)
            throw new ArgumentException("Meeting schedule not found", nameof(responsibilityAssignment.MeetingScheduleId));

        // Verificar que el Publisher existe
        var publisher = await context.Publishers.FindAsync(responsibilityAssignment.PublisherId);
        if (publisher == null)
            throw new ArgumentException("Publisher not found", nameof(responsibilityAssignment.PublisherId));

        // Verificar que la Responsibility existe
        var responsibility = await context.Responsibilities.FindAsync(responsibilityAssignment.ResponsibilityId);
        if (responsibility == null)
            throw new ArgumentException("Responsibility not found", nameof(responsibilityAssignment.ResponsibilityId));

        // Verificar que la asignación no existe ya
        var existing = await GetByIdAsync(
            responsibilityAssignment.MeetingScheduleId, 
            responsibilityAssignment.ResponsibilityId, 
            responsibilityAssignment.PublisherId);
        if (existing != null)
            throw new InvalidOperationException("Responsibility assignment already exists");

        context.ResponsibilityAssignments.Add(responsibilityAssignment);
        await context.SaveChangesAsync();
        return responsibilityAssignment;
    }

    public async Task<bool> DeleteAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var assignment = await context.ResponsibilityAssignments
            .FirstOrDefaultAsync(ra => 
                ra.MeetingScheduleId == meetingScheduleId && 
                ra.ResponsibilityId == responsibilityId && 
                ra.PublisherId == publisherId);

        if (assignment == null)
            return false;

        context.ResponsibilityAssignments.Remove(assignment);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetByMeetingScheduleIdAsync(Guid meetingScheduleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResponsibilityAssignments
            .Where(ra => ra.MeetingScheduleId == meetingScheduleId)
            .Include(ra => ra.Publisher)
            .Include(ra => ra.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(ra => ra.Responsibility.Name)
            .ThenBy(ra => ra.Publisher.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetByPublisherIdAsync(Guid publisherId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResponsibilityAssignments
            .Where(ra => ra.PublisherId == publisherId)
            .Include(ra => ra.MeetingSchedule)
                .ThenInclude(ms => ms.MeetingType)
            .Include(ra => ra.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(ra => ra.MeetingSchedule.Date)
            .ThenBy(ra => ra.Responsibility.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetByResponsibilityIdAsync(Guid responsibilityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResponsibilityAssignments
            .Where(ra => ra.ResponsibilityId == responsibilityId)
            .Include(ra => ra.MeetingSchedule)
                .ThenInclude(ms => ms.MeetingType)
            .Include(ra => ra.Publisher)
            .OrderBy(ra => ra.MeetingSchedule.Date)
            .ThenBy(ra => ra.Publisher.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResponsibilityAssignments
            .Where(ra => ra.MeetingSchedule.Date >= startDate && ra.MeetingSchedule.Date <= endDate)
            .Include(ra => ra.MeetingSchedule)
                .ThenInclude(ms => ms.MeetingType)
            .Include(ra => ra.Publisher)
            .Include(ra => ra.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(ra => ra.MeetingSchedule.Date)
            .ThenBy(ra => ra.Responsibility.Name)
            .ToListAsync();
    }

    public async Task<bool> AssignResponsibilityAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var meetingSchedule = await context.MeetingSchedules.FindAsync(meetingScheduleId);
            var publisher = await context.Publishers.FindAsync(publisherId);
            var responsibility = await context.Responsibilities.FindAsync(responsibilityId);

            if (meetingSchedule == null || publisher == null || responsibility == null)
                return false;

            // Remove any existing assignment for this meeting/responsibility (ensure only one publisher per responsibility per meeting)
            var existingAssignments = await context.ResponsibilityAssignments
                .Where(ra => ra.MeetingScheduleId == meetingScheduleId && ra.ResponsibilityId == responsibilityId)
                .ToListAsync();

            if (existingAssignments.Any())
            {
                context.ResponsibilityAssignments.RemoveRange(existingAssignments);
            }

            var assignment = new ResponsibilityAssignment
            {
                MeetingScheduleId = meetingScheduleId,
                ResponsibilityId = responsibilityId,
                PublisherId = publisherId,
                MeetingSchedule = meetingSchedule,
                Publisher = publisher,
                Responsibility = responsibility
            };

            context.ResponsibilityAssignments.Add(assignment);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveAssignmentAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId)
    {
        return await DeleteAsync(meetingScheduleId, responsibilityId, publisherId);
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetPublisherAssignmentsForMonthAsync(Guid publisherId, int month, int year)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResponsibilityAssignments
            .Where(ra => ra.PublisherId == publisherId && 
                        ra.MeetingSchedule.Month == month && 
                        ra.MeetingSchedule.Year == year)
            .Include(ra => ra.MeetingSchedule)
                .ThenInclude(ms => ms.MeetingType)
            .Include(ra => ra.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(ra => ra.MeetingSchedule.Date)
            .ThenBy(ra => ra.Responsibility.Name)
            .ToListAsync();
    }
}

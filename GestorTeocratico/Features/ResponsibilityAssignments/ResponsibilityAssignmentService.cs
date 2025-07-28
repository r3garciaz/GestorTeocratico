using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.ResponsibilityAssignments;

public class ResponsibilityAssignmentService : IResponsibilityAssignmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResponsibilityAssignmentService> _logger;

    public ResponsibilityAssignmentService(ApplicationDbContext context, ILogger<ResponsibilityAssignmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetAllAsync()
    {
        return await _context.ResponsibilityAssignments
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
        return await _context.ResponsibilityAssignments
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
        // Verificar que el MeetingSchedule existe
        var meetingSchedule = await _context.MeetingSchedules.FindAsync(responsibilityAssignment.MeetingScheduleId);
        if (meetingSchedule == null)
            throw new ArgumentException("Meeting schedule not found", nameof(responsibilityAssignment.MeetingScheduleId));

        // Verificar que el Publisher existe
        var publisher = await _context.Publishers.FindAsync(responsibilityAssignment.PublisherId);
        if (publisher == null)
            throw new ArgumentException("Publisher not found", nameof(responsibilityAssignment.PublisherId));

        // Verificar que la Responsibility existe
        var responsibility = await _context.Responsibilities.FindAsync(responsibilityAssignment.ResponsibilityId);
        if (responsibility == null)
            throw new ArgumentException("Responsibility not found", nameof(responsibilityAssignment.ResponsibilityId));

        // Verificar que la asignación no existe ya
        var existing = await GetByIdAsync(
            responsibilityAssignment.MeetingScheduleId, 
            responsibilityAssignment.ResponsibilityId, 
            responsibilityAssignment.PublisherId);
        if (existing != null)
            throw new InvalidOperationException("Responsibility assignment already exists");

        _context.ResponsibilityAssignments.Add(responsibilityAssignment);
        await _context.SaveChangesAsync();
        return responsibilityAssignment;
    }

    public async Task<bool> DeleteAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId)
    {
        var assignment = await _context.ResponsibilityAssignments
            .FirstOrDefaultAsync(ra => 
                ra.MeetingScheduleId == meetingScheduleId && 
                ra.ResponsibilityId == responsibilityId && 
                ra.PublisherId == publisherId);

        if (assignment == null)
            return false;

        _context.ResponsibilityAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ResponsibilityAssignment>> GetByMeetingScheduleIdAsync(Guid meetingScheduleId)
    {
        return await _context.ResponsibilityAssignments
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
        return await _context.ResponsibilityAssignments
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
        return await _context.ResponsibilityAssignments
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
        return await _context.ResponsibilityAssignments
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
        try
        {
            var meetingSchedule = await _context.MeetingSchedules.FindAsync(meetingScheduleId);
            var publisher = await _context.Publishers.FindAsync(publisherId);
            var responsibility = await _context.Responsibilities.FindAsync(responsibilityId);

            if (meetingSchedule == null || publisher == null || responsibility == null)
                return false;

            var assignment = new ResponsibilityAssignment
            {
                MeetingScheduleId = meetingScheduleId,
                ResponsibilityId = responsibilityId,
                PublisherId = publisherId,
                MeetingSchedule = meetingSchedule,
                Publisher = publisher,
                Responsibility = responsibility
            };

            await CreateAsync(assignment);
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
        return await _context.ResponsibilityAssignments
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

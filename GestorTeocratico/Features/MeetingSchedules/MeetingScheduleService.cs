using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public class MeetingScheduleService : IMeetingScheduleService
{
    private readonly ApplicationDbContext _context;

    public MeetingScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MeetingSchedule>> GetAllAsync()
    {
        return await _context.MeetingSchedules
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .OrderBy(ms => ms.Date)
            .ToListAsync();
    }

    public async Task<MeetingSchedule?> GetByIdAsync(Guid id)
    {
        return await _context.MeetingSchedules
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .FirstOrDefaultAsync(ms => ms.MeetingScheduleId == id);
    }

    public async Task<MeetingSchedule> CreateAsync(MeetingSchedule meetingSchedule)
    {
        _context.MeetingSchedules.Add(meetingSchedule);
        await _context.SaveChangesAsync();
        return meetingSchedule;
    }

    public async Task<MeetingSchedule?> UpdateAsync(MeetingSchedule meetingSchedule)
    {
        var existingSchedule = await _context.MeetingSchedules.FindAsync(meetingSchedule.MeetingScheduleId);
        if (existingSchedule == null)
            return null;

        existingSchedule.MeetingType = meetingSchedule.MeetingType;
        existingSchedule.Date = meetingSchedule.Date;
        existingSchedule.Month = meetingSchedule.Month;
        existingSchedule.Year = meetingSchedule.Year;
        existingSchedule.WeekOfYear = meetingSchedule.WeekOfYear;

        await _context.SaveChangesAsync();
        return existingSchedule;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var meetingSchedule = await _context.MeetingSchedules.FindAsync(id);
        if (meetingSchedule == null)
            return false;

        _context.MeetingSchedules.Remove(meetingSchedule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<MeetingSchedule>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.MeetingSchedules
            .Where(ms => ms.Date >= startDate && ms.Date <= endDate)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .OrderBy(ms => ms.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<MeetingSchedule>> GetByMonthAsync(int month, int year)
    {
        return await _context.MeetingSchedules
            .Where(ms => ms.Month == month && ms.Year == year)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .OrderBy(ms => ms.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<MeetingSchedule>> GetByWeekAsync(int weekOfYear, int year)
    {
        return await _context.MeetingSchedules
            .Where(ms => ms.WeekOfYear == weekOfYear && ms.Year == year)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .OrderBy(ms => ms.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<MeetingSchedule>> GetByMeetingTypeAsync(MeetingType meetingType)
    {
        return await _context.MeetingSchedules
            .Where(ms => ms.MeetingType == meetingType)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .OrderBy(ms => ms.Date)
            .ToListAsync();
    }

    public async Task<MeetingSchedule?> GetByDateAndMeetingTypeAsync(DateOnly date, MeetingType meetingType)
    {
        return await _context.MeetingSchedules
            .Where(ms => ms.Date == date && ms.MeetingType == meetingType)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .FirstOrDefaultAsync();
    }
}

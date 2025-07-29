using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

    public async Task<IEnumerable<MeetingSchedule>> GetOrCreateWeekSchedulesAsync(int weekOfYear, int year, Guid? congregationId = null)
    {
        var existingSchedules = await GetByWeekAsync(weekOfYear, year);
        
        if (existingSchedules.Any())
            return existingSchedules;

        // Create both midweek and weekend meetings for the week
        var midweekSchedule = await GetOrCreateMeetingScheduleAsync(weekOfYear, year, MeetingType.Midweek, congregationId);
        var weekendSchedule = await GetOrCreateMeetingScheduleAsync(weekOfYear, year, MeetingType.Weekend, congregationId);

        return new[] { midweekSchedule, weekendSchedule };
    }

    public async Task<MeetingSchedule> GetOrCreateMeetingScheduleAsync(int weekOfYear, int year, MeetingType meetingType, Guid? congregationId = null)
    {
        // Calculate the Monday of the given week
        var mondayOfWeek = ISOWeek.ToDateTime(year, weekOfYear, DayOfWeek.Monday);
        var dateOnly = DateOnly.FromDateTime(mondayOfWeek);

        // Try to find existing schedule
        var existingSchedule = await _context.MeetingSchedules
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .FirstOrDefaultAsync(ms => ms.WeekOfYear == weekOfYear && 
                               ms.Year == year && 
                               ms.MeetingType == meetingType);

        if (existingSchedule != null)
            return existingSchedule;

        // Create new schedule
        var newSchedule = new MeetingSchedule
        {
            Date = dateOnly,
            MeetingType = meetingType,
            Month = dateOnly.Month,
            Year = year,
            WeekOfYear = weekOfYear
        };

        return await CreateAsync(newSchedule);
    }

    public async Task<bool> CopyAssignmentsToWeekAsync(int sourceWeek, int sourceYear, int targetWeek, int targetYear)
    {
        try
        {
            var sourceSchedules = await GetByWeekAsync(sourceWeek, sourceYear);
            var targetSchedules = await GetOrCreateWeekSchedulesAsync(targetWeek, targetYear);

            foreach (var sourceSchedule in sourceSchedules)
            {
                var targetSchedule = targetSchedules.FirstOrDefault(ts => ts.MeetingType == sourceSchedule.MeetingType);
                if (targetSchedule == null) continue;

                // Clear existing assignments in target week
                var existingAssignments = await _context.ResponsibilityAssignments
                    .Where(ra => ra.MeetingScheduleId == targetSchedule.MeetingScheduleId)
                    .ToListAsync();
                
                _context.ResponsibilityAssignments.RemoveRange(existingAssignments);

                // Copy assignments from source
                foreach (var sourceAssignment in sourceSchedule.ResponsibilityAssignments)
                {
                    var newAssignment = new ResponsibilityAssignment
                    {
                        MeetingScheduleId = targetSchedule.MeetingScheduleId,
                        ResponsibilityId = sourceAssignment.ResponsibilityId,
                        PublisherId = sourceAssignment.PublisherId,
                        MeetingSchedule = targetSchedule,
                        Responsibility = sourceAssignment.Responsibility,
                        Publisher = sourceAssignment.Publisher
                    };

                    _context.ResponsibilityAssignments.Add(newAssignment);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

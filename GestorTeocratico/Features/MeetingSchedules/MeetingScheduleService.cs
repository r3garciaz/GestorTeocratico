using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GestorTeocratico.Features.MeetingSchedules;

public class MeetingScheduleService : IMeetingScheduleService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public MeetingScheduleService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<MeetingSchedule>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MeetingSchedules
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .OrderBy(ms => ms.Date)
            .ToListAsync();
    }

    public async Task<MeetingSchedule?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MeetingSchedules
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .FirstOrDefaultAsync(ms => ms.MeetingScheduleId == id);
    }

    public async Task<MeetingSchedule> CreateAsync(MeetingSchedule meetingSchedule)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.MeetingSchedules.Add(meetingSchedule);
        await context.SaveChangesAsync();
        return meetingSchedule;
    }

    public async Task<MeetingSchedule?> UpdateAsync(MeetingSchedule meetingSchedule)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingSchedule = await context.MeetingSchedules.FindAsync(meetingSchedule.MeetingScheduleId);
        if (existingSchedule == null)
            return null;

        existingSchedule.MeetingType = meetingSchedule.MeetingType;
        existingSchedule.Date = meetingSchedule.Date;
        existingSchedule.Month = meetingSchedule.Month;
        existingSchedule.Year = meetingSchedule.Year;
        existingSchedule.WeekOfYear = meetingSchedule.WeekOfYear;

        await context.SaveChangesAsync();
        return existingSchedule;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var meetingSchedule = await context.MeetingSchedules.FindAsync(id);
        if (meetingSchedule == null)
            return false;

        context.MeetingSchedules.Remove(meetingSchedule);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<MeetingSchedule>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MeetingSchedules
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MeetingSchedules
            .Where(ms => ms.Month == month && ms.Year == year)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher)
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Responsibility)
            .ThenInclude(r => r.Department)
            .OrderBy(ms => ms.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<MeetingSchedule>> GetByWeekAsync(int weekOfYear, int year)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MeetingSchedules
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MeetingSchedules
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MeetingSchedules
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Calculate the Monday of the given week
        var mondayOfWeek = ISOWeek.ToDateTime(year, weekOfYear, DayOfWeek.Monday);
        var dateOnly = DateOnly.FromDateTime(mondayOfWeek);

        // Try to find existing schedule
        var existingSchedule = await context.MeetingSchedules
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var sourceSchedules = await GetByWeekAsync(sourceWeek, sourceYear);
            var targetSchedules = await GetOrCreateWeekSchedulesAsync(targetWeek, targetYear);

            foreach (var sourceSchedule in sourceSchedules)
            {
                var targetSchedule = targetSchedules.FirstOrDefault(ts => ts.MeetingType == sourceSchedule.MeetingType);
                if (targetSchedule == null) continue;

                // Clear existing assignments in target week
                var existingAssignments = await context.ResponsibilityAssignments
                    .Where(ra => ra.MeetingScheduleId == targetSchedule.MeetingScheduleId)
                    .ToListAsync();
                
                context.ResponsibilityAssignments.RemoveRange(existingAssignments);

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

                    context.ResponsibilityAssignments.Add(newAssignment);
                }
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

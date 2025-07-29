using GestorTeocratico.Entities;
using GestorTeocratico.Shared.Enums;

namespace GestorTeocratico.Features.MeetingSchedules;

public interface IMeetingScheduleService
{
    Task<IEnumerable<MeetingSchedule>> GetAllAsync();
    Task<MeetingSchedule?> GetByIdAsync(Guid id);
    Task<MeetingSchedule> CreateAsync(MeetingSchedule meetingSchedule);
    Task<MeetingSchedule?> UpdateAsync(MeetingSchedule meetingSchedule);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<MeetingSchedule>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<IEnumerable<MeetingSchedule>> GetByMonthAsync(int month, int year);
    Task<IEnumerable<MeetingSchedule>> GetByWeekAsync(int weekOfYear, int year);
    Task<IEnumerable<MeetingSchedule>> GetByMeetingTypeAsync(MeetingType meetingType);
    Task<MeetingSchedule?> GetByDateAndMeetingTypeAsync(DateOnly date, MeetingType meetingType);
    Task<IEnumerable<MeetingSchedule>> GetOrCreateWeekSchedulesAsync(int weekOfYear, int year, Guid? congregationId = null);
    Task<MeetingSchedule> GetOrCreateMeetingScheduleAsync(int weekOfYear, int year, MeetingType meetingType, Guid? congregationId = null);
    Task<bool> CopyAssignmentsToWeekAsync(int sourceWeek, int sourceYear, int targetWeek, int targetYear);
}

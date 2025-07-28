using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.ResponsibilityAssignments;

public interface IResponsibilityAssignmentService
{
    Task<IEnumerable<ResponsibilityAssignment>> GetAllAsync();
    Task<ResponsibilityAssignment?> GetByIdAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId);
    Task<ResponsibilityAssignment> CreateAsync(ResponsibilityAssignment responsibilityAssignment);
    Task<bool> DeleteAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId);
    Task<IEnumerable<ResponsibilityAssignment>> GetByMeetingScheduleIdAsync(Guid meetingScheduleId);
    Task<IEnumerable<ResponsibilityAssignment>> GetByPublisherIdAsync(Guid publisherId);
    Task<IEnumerable<ResponsibilityAssignment>> GetByResponsibilityIdAsync(Guid responsibilityId);
    Task<IEnumerable<ResponsibilityAssignment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<bool> AssignResponsibilityAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId);
    Task<bool> RemoveAssignmentAsync(Guid meetingScheduleId, Guid responsibilityId, Guid publisherId);
    Task<IEnumerable<ResponsibilityAssignment>> GetPublisherAssignmentsForMonthAsync(Guid publisherId, int month, int year);
}

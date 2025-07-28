using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.MeetingTypes;

public interface IMeetingTypeService
{
    Task<IEnumerable<MeetingType>> GetAllAsync();
    Task<MeetingType?> GetByIdAsync(Guid id);
    Task<MeetingType> CreateAsync(MeetingType meetingType);
    Task<MeetingType?> UpdateAsync(MeetingType meetingType);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<MeetingType>> GetByTypeAsync(Shared.Enums.MeetingType type);
    Task<IEnumerable<MeetingType>> GetAvailableForSchedulingAsync();
}

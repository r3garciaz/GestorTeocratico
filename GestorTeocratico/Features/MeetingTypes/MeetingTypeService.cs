using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingTypes;

public class MeetingTypeService : IMeetingTypeService
{
    private readonly ApplicationDbContext _context;

    public MeetingTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MeetingType>> GetAllAsync()
    {
        return await _context.MeetingTypes
            .OrderBy(mt => mt.Type)
            .ThenBy(mt => mt.Name)
            .ToListAsync();
    }

    public async Task<MeetingType?> GetByIdAsync(Guid id)
    {
        return await _context.MeetingTypes
            .FirstOrDefaultAsync(mt => mt.MeetingTypeId == id);
    }

    public async Task<MeetingType> CreateAsync(MeetingType meetingType)
    {
        _context.MeetingTypes.Add(meetingType);
        await _context.SaveChangesAsync();
        return meetingType;
    }

    public async Task<MeetingType?> UpdateAsync(MeetingType meetingType)
    {
        var existingMeetingType = await _context.MeetingTypes.FindAsync(meetingType.MeetingTypeId);
        if (existingMeetingType == null)
            return null;

        existingMeetingType.Name = meetingType.Name;
        existingMeetingType.Description = meetingType.Description;
        existingMeetingType.Type = meetingType.Type;

        await _context.SaveChangesAsync();
        return existingMeetingType;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var meetingType = await _context.MeetingTypes.FindAsync(id);
        if (meetingType == null)
            return false;

        meetingType.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<MeetingType>> GetByTypeAsync(Shared.Enums.MeetingType type)
    {
        return await _context.MeetingTypes
            .Where(mt => mt.Type == type)
            .OrderBy(mt => mt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MeetingType>> GetAvailableForSchedulingAsync()
    {
        return await _context.MeetingTypes
            .OrderBy(mt => mt.Type)
            .ThenBy(mt => mt.Name)
            .ToListAsync();
    }
}

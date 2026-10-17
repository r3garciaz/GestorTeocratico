using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Congregations;

internal static class CongregationMappings
{
    public static CongregationDto ToDto(this Congregation congregation) => new(
        congregation.CongregationId,
        congregation.Name,
        congregation.MidweekMeetingDayEvenYear,
        congregation.MidweekMeetingDayOddYear,
        congregation.WeekendMeetingDayEvenYear,
        congregation.WeekendMeetingDayOddYear,
        congregation.Address,
        congregation.City);

    public static CongregationFormModel ToFormModel(this CongregationDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        MidweekMeetingDayEvenYear = dto.MidweekMeetingDayEvenYear,
        MidweekMeetingDayOddYear = dto.MidweekMeetingDayOddYear,
        WeekendMeetingDayEvenYear = dto.WeekendMeetingDayEvenYear,
        WeekendMeetingDayOddYear = dto.WeekendMeetingDayOddYear,
        Address = dto.Address,
        City = dto.City
    };
}

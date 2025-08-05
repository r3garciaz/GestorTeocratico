using GestorTeocratico.Entities;
using GestorTeocratico.Shared.Enums;

namespace GestorTeocratico.Features.PdfExport.Models;

public class MonthlySchedulePdfModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string? MonthName { get; set; }
    public List<ScheduleRowModel> ScheduleRows { get; set; } = [];
    public List<ResponsibilityColumnModel> ResponsibilityColumns { get; set; } = [];
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

public class ScheduleRowModel
{
    public DateOnly Date { get; set; }
    public MeetingType MeetingType { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public string MeetingTypeDisplay { get; set; } = string.Empty;
    public Dictionary<Guid, string> Assignments { get; set; } = new();
}

public class ResponsibilityColumnModel
{
    public Guid ResponsibilityId { get; set; }
    public string? Name { get; set; }
    public string? DepartmentName { get; set; }
}

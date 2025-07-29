namespace GestorTeocratico.Entities;

/// <summary>
/// Represents the association between a Publisher and a Responsibility.
/// </summary>
public class PublisherResponsibility
{
    public required Guid PublisherId { get; set; }
    public required Guid ResponsibilityId { get; set; }
    
    public Publisher? Publisher { get; set; }
    public Responsibility? Responsibility { get; set; }
}
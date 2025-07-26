namespace GestorTeocratico.Entities;

public abstract class SoftDeleteEntity
{
    public required bool IsDeleted { get; set; }
}
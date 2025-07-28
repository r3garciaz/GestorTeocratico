namespace GestorTeocratico.Entities;

public abstract class SoftDeleteEntity
{
    public bool IsDeleted { get; set; }
}
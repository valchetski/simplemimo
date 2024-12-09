using System.ComponentModel.DataAnnotations;

namespace SimpleMimo.Data.Entities;

public class Lesson : Entity
{
    [MaxLength(100)]
    public required string Name { get; init; }
    
    public int Order { get; init; }
    
    public long ChapterId { get; init; }
    
    public Chapter Chapter { get; init; } = null!;
}
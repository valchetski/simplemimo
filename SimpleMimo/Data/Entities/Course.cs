using System.ComponentModel.DataAnnotations;

namespace SimpleMimo.Data.Entities;

public class Course : Entity
{
    [MaxLength(100)]
    public required string Name { get; init; }
    
    public Achievement? Achievement { get; init; }
    
    public ICollection<Chapter> Chapters { get; } = new List<Chapter>();
}
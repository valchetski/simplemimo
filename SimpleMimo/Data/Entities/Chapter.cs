using System.ComponentModel.DataAnnotations;

namespace SimpleMimo.Data.Entities;

public class Chapter : Entity
{
    [MaxLength(100)]
    public required string Name { get; set; }
    
    // Added this field as task description mentions that chapters should be 
    // displayed in a specific order
    // It's not used as we don't have an endpoint that returns list of chapters
    public int Order { get; init; }
    
    public long CourseId { get; init; }
    
    public Course Course { get; init; } = null!;
    
    public ICollection<Lesson> Lessons { get; } = new List<Lesson>();
}
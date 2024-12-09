using System.ComponentModel.DataAnnotations;

namespace SimpleMimo.Data.Entities;

public class Achievement : Entity
{
    [MaxLength(100)]
    public required string Name { get; init; }
    
    public required AchievementCategory Category { get; init; }
    
    public required int Target { get; init; }
    
    // I was thinking to create a separate entity for Course Achievement
    // But then decided that it'll overcomplicate solution
    // Decided to keep CourseId and let it be null for other type of Achievements
    public long? CourseId { get; init; }
    
    public Course? Course { get; init; }
}
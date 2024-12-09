using Microsoft.EntityFrameworkCore;

namespace SimpleMimo.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(CourseId))]
public class UserCourse
{
    public required long UserId { get; init; }
    
    public required long CourseId { get; init; }
    
    public required DateTime CompletedDate { get; init; }
}
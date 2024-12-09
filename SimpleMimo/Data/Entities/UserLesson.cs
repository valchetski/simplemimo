namespace SimpleMimo.Data.Entities;

public class UserLesson : Entity
{
    public DateTime StartDate { get; init; }
    
    public DateTime CompleteDate { get; init; }
    
    public long UserId { get; init; }
    
    public User User { get; init; } = null!;
    
    public long LessonId { get; init; }
    
    public Lesson Lesson { get; init; } = null!;
}
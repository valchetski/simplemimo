namespace SimpleMimo.Data.Entities;

public class User : Entity
{
    public ICollection<UserLesson> UserLessons { get; } = new List<UserLesson>();

    public ICollection<UserChapter> UserChapters { get; } = new List<UserChapter>();
    
    public ICollection<UserCourse> UserCourses { get; } = new List<UserCourse>();
    
    public ICollection<UserAchievement> UserAchievements { get; } = new List<UserAchievement>();
}
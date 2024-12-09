using Microsoft.EntityFrameworkCore;

namespace SimpleMimo.Data.Entities;

public class MimoDbContext(DbContextOptions<MimoDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    
    public DbSet<Achievement> Achievements { get; init; }
    
    public DbSet<Course> Courses { get; init; }
    
    public DbSet<Chapter> Chapters { get; init; }
    
    public DbSet<Lesson> Lessons { get; init; }
    
    public DbSet<UserLesson> UserLessons { get; init; }
    
    public DbSet<UserChapter> UserChapters { get; init; }
    
    public DbSet<UserCourse> UserCourses { get; init; }
    
    public DbSet<UserAchievement> UserAchievements { get; init; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Achievement>()
            .Property(d => d.Category)
            .HasConversion<string>();
    }
}
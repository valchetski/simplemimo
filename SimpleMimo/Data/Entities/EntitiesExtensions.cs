using Microsoft.EntityFrameworkCore;

namespace SimpleMimo.Data.Entities;

public static class EntitiesExtensions
{
    public static IServiceCollection AddMimoDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        return services.AddDbContext<MimoDbContext>(options =>
            options.UseSqlite(connectionString)
                .UseSeeding((dbContext, _) =>
                {
                    if (!dbContext.Set<User>().Any())
                    {
                        dbContext.Set<User>().Add(new User());
                    }
                    
                    var swiftCourse = CreateCourseIfNotExists(dbContext, "Swift");
                    var jsCourse = CreateCourseIfNotExists(dbContext, "Javascript");
                    var csharpCourse = CreateCourseIfNotExists(dbContext, "C#");
                    
                    CreateAchievementIfNotExists(dbContext, "Complete 5 lessons", AchievementCategory.Lesson, 5);
                    CreateAchievementIfNotExists(dbContext, "Complete 25 lessons", AchievementCategory.Lesson, 25);
                    CreateAchievementIfNotExists(dbContext, "Complete 50 lessons", AchievementCategory.Lesson, 50);
                    CreateAchievementIfNotExists(dbContext, "Complete 1 chapter", AchievementCategory.Chapter, 1);
                    CreateAchievementIfNotExists(dbContext, "Complete 5 chapters", AchievementCategory.Chapter, 5);
                    
                    CreateAchievementIfNotExists(dbContext, "Complete the Swift course", AchievementCategory.Course, swiftCourse.Chapters.Count, swiftCourse.Id);
                    CreateAchievementIfNotExists(dbContext, "Complete the Javascript course", AchievementCategory.Course, jsCourse.Chapters.Count, jsCourse.Id);
                    CreateAchievementIfNotExists(dbContext, "Complete the C# course", AchievementCategory.Course, csharpCourse.Chapters.Count, csharpCourse.Id);
            
                    dbContext.SaveChanges();
                }));
    }

    private static Course CreateCourseIfNotExists(DbContext dbContext, string name)
    {
        var course = dbContext.Set<Course>().FirstOrDefault(c => c.Name == name);
        if (course == null)
        {
            course = dbContext.Set<Course>().Add(new Course
            {
                Name = name,
                Chapters =
                {
                    new Chapter
                    {
                        Name = "First Chapter",
                        Lessons =
                        {
                            new Lesson() { Name = "First Lesson" },
                            new Lesson() { Name = "Second Lesson" },
                            new Lesson() { Name = "Third Lesson" },
                        },
                    },
                    new Chapter()
                    {
                        Name = "Second Chapter",
                        Lessons =
                        {
                            new Lesson() { Name = "First Lesson" },
                            new Lesson() { Name = "Second Lesson" },
                            new Lesson() { Name = "Third Lesson" },
                        },
                    },
                },
            }).Entity;
            dbContext.SaveChanges();
        }

        return course;
    }

    private static void CreateAchievementIfNotExists(
        DbContext dbContext,
        string name,
        AchievementCategory category,
        int target,
        long? courseId = null)
    {
        var achievement = dbContext.Set<Achievement>().FirstOrDefault(a => a.Name == name);
        if (achievement == null)
        {
            dbContext.Set<Achievement>().Add(new Achievement()
            {
                Name = name,
                Category = category,
                Target = target,
                CourseId = courseId,
            });
        }
    }
}
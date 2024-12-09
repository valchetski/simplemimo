using SimpleMimo.Services.Dtos;

namespace SimpleMimo.Services;

public interface IUserAchievementService
{
    Task<AchievementDto[]> GetUserAchievementsAsync(long userId, CancellationToken cancellationToken);
    
    Task TrackLessonsAchievementsAsync(long userId, CompletedLessonDto[] completedLessons, CancellationToken cancellationToken);

    Task TrackChaptersAchievementsAsync(long userId, ChapterProgressDto[] chaptersProgresses, CancellationToken cancellationToken);

    Task TrackCoursesAchievementsAsync(long userId, CourseProgressDto[] coursesProgresses, CancellationToken cancellationToken);
}
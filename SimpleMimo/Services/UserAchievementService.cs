using Microsoft.EntityFrameworkCore;
using SimpleMimo.Data.Entities;
using SimpleMimo.Services.Dtos;
using SimpleMimo.Services.Exceptions;

namespace SimpleMimo.Services;

public class UserAchievementService(MimoDbContext dbContext) : IUserAchievementService
{
    public Task<AchievementDto[]> GetUserAchievementsAsync(long userId, CancellationToken cancellationToken)
    {
        return dbContext.UserAchievements
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(ua => new AchievementDto(ua.AchievementId, ua.IsCompleted, ua.Progress))
            .ToArrayAsync(cancellationToken);
    }

    public async Task TrackLessonsAchievementsAsync(long userId, CompletedLessonDto[] completedLessons, CancellationToken cancellationToken)
    {
        var lessonsAchievements = await dbContext.Achievements
            .AsNoTracking()
            .Where(x => x.Category == AchievementCategory.Lesson)
            .ToArrayAsync(cancellationToken);

        foreach (var lessonsAchievement in lessonsAchievements)
        {
            
            var existingUserAchievement = await dbContext
                .UserAchievements
                .FirstOrDefaultAsync(x => x.AchievementId == lessonsAchievement.Id && x.UserId == userId, cancellationToken);
            
            if (existingUserAchievement == null)
            {
                dbContext.UserAchievements.Add(new UserAchievement()
                {
                    UserId = userId,
                    AchievementId = lessonsAchievement.Id,
                    Progress = Math.Min(completedLessons.Length, lessonsAchievement.Target), // in case more lessons are completed than achievement needs
                    IsCompleted = completedLessons.Length >= lessonsAchievement.Target,
                });
            }
            else
            {
                if (existingUserAchievement.IsCompleted)
                {
                    continue;
                }
                
                existingUserAchievement.Progress = Math.Min(
                    existingUserAchievement.Progress + completedLessons.Length,
                    lessonsAchievement.Target);
                existingUserAchievement.IsCompleted = existingUserAchievement.Progress == lessonsAchievement.Target;
            }
        }
    }

    public async Task TrackChaptersAchievementsAsync(long userId, ChapterProgressDto[] chaptersProgresses, CancellationToken cancellationToken)
    {
        var chaptersAchievements = await dbContext.Achievements
            .AsNoTracking()
            .Where(x => x.Category == AchievementCategory.Chapter)
            .ToArrayAsync(cancellationToken);
        var completedChaptersCount = chaptersProgresses.Count(x => x.CompleteDate.HasValue);
        
        foreach (var chaptersAchievement in chaptersAchievements)
        {
            var existingUserAchievement = await dbContext
                .UserAchievements
                .FirstOrDefaultAsync(x => x.AchievementId == chaptersAchievement.Id && x.UserId == userId, cancellationToken);
            
            if (existingUserAchievement == null)
            {
                dbContext.UserAchievements.Add(new UserAchievement()
                {
                    UserId = userId,
                    AchievementId = chaptersAchievement.Id,
                    Progress = Math.Min(completedChaptersCount, chaptersAchievement.Target),
                    IsCompleted = completedChaptersCount >= chaptersAchievement.Target,
                });
            }
            else
            {
                if (existingUserAchievement.IsCompleted)
                {
                    continue;
                }
                
                existingUserAchievement.Progress = Math.Min(
                    existingUserAchievement.Progress += completedChaptersCount,
                    chaptersAchievement.Target);
                existingUserAchievement.IsCompleted = existingUserAchievement.Progress >= chaptersAchievement.Target;
            }
        }
    }
    
    public async Task TrackCoursesAchievementsAsync(long userId, CourseProgressDto[] coursesProgresses, CancellationToken cancellationToken)
    {
        foreach (var courseProgress in coursesProgresses)
        {
            var achievement = await dbContext.Achievements
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseId == courseProgress.CourseId, cancellationToken);
            
            if (achievement == null)
            {
                throw new InternalNotFoundException(courseProgress.CourseId.ToString(), nameof(Achievement));
            }
            
            var existingCourseAchievement = await dbContext
                .UserAchievements
                .FirstOrDefaultAsync(x => x.AchievementId == achievement.Id && x.UserId == userId, cancellationToken);
            
            if (existingCourseAchievement == null)
            {
                dbContext.UserAchievements.Add(new UserAchievement()
                {
                    UserId = userId,
                    AchievementId = achievement.Id,
                    Progress = courseProgress.CompletedChaptersCount,
                    IsCompleted = courseProgress.CompletedChaptersCount == achievement.Target,
                });
            }
            else
            {
                if (existingCourseAchievement.IsCompleted)
                {
                    continue;
                }
                
                existingCourseAchievement.Progress += courseProgress.CompletedChaptersCount;
                existingCourseAchievement.IsCompleted = courseProgress.CompletedChaptersCount == achievement.Target;
            }
        }
    }
}
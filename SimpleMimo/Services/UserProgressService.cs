using Microsoft.EntityFrameworkCore;
using SimpleMimo.Data.Entities;
using SimpleMimo.Models;
using SimpleMimo.Services.Dtos;
using SimpleMimo.Services.Exceptions;

namespace SimpleMimo.Services;

public class UserProgressService(IUserAchievementService userAchievementService, MimoDbContext dbContext)
    : IUserProgressService
{
    public async Task TrackAsync(long userId, CompletedLessonRequest[] completedLessons, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.UserLessons)
            .Include(x => x.UserChapters)
            .Include(x => x.UserCourses)
            .Include(x => x.UserAchievements)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(userId.ToString(), nameof(User));
        }
        
        var completedLessonsDto = await TrackLessonsAsync(user, completedLessons, cancellationToken);
        var chaptersProgresses = await TrackChaptersAsync(user, completedLessonsDto, cancellationToken);
        var coursesProgresses = await TrackCoursesAsync(user, chaptersProgresses, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        // In real-life, as we don't need to return achievements straight away in the response,
        // we could track achievements in the background.
        // for example below we could put completed lessons, chapters and courses info into queue
        // and let separate Achievement service to read from it.
        await userAchievementService.TrackLessonsAchievementsAsync(userId, completedLessonsDto, cancellationToken);
        await userAchievementService.TrackChaptersAchievementsAsync(userId, chaptersProgresses, cancellationToken);
        await userAchievementService.TrackCoursesAchievementsAsync(userId, coursesProgresses, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task<CompletedLessonDto[]> TrackLessonsAsync(
        User user,
        CompletedLessonRequest[] completedLessonsRequests,
        CancellationToken cancellationToken)
    {
        var completedLessonsIds = completedLessonsRequests.Select(x => x.Id).ToArray();
        var foundLessons = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => completedLessonsIds.Contains(x.Id))
            .ToArrayAsync(cancellationToken);
        var notFoundLessonsIds = completedLessonsIds.Except(foundLessons.Select(x => x.Id)).ToArray();
        if (notFoundLessonsIds.Length != 0)
        {
            throw new NotFoundException(string.Join(',', notFoundLessonsIds), nameof(Lesson));
        }
        
        var completedLessons = new List<CompletedLessonDto>();
        foreach (var completedLesson in completedLessonsRequests)
        {
            var userLesson = new UserLesson()
            {
                LessonId = completedLesson.Id,
                UserId = user.Id,
                StartDate = completedLesson.StartDate,
                CompleteDate = completedLesson.CompleteDate,
            };

            if (user.UserLessons.All(x => x.LessonId != userLesson.LessonId || x.UserId != userLesson.UserId)
                && completedLessons.All(x => x.LessonId != completedLesson.Id))
            {
                completedLessons.Add(
                    new CompletedLessonDto(
                        userLesson.LessonId,
                        foundLessons.First(l => l.Id == userLesson.LessonId).ChapterId,
                        userLesson.CompleteDate));
            }
            
            // as lessons can be completed multiple times we save all the completions
            user.UserLessons.Add(userLesson);
        }

        return completedLessons.ToArray();
    }

    private async Task<ChapterProgressDto[]> TrackChaptersAsync(
        User user,
        CompletedLessonDto[] completedLessons,
        CancellationToken cancellationToken)
    {
        var chaptersIds = completedLessons.Select(x => x.ChapterId).Distinct().ToArray();
        var chapters = await dbContext.Chapters
            .Include(x => x.Lessons)
            .AsNoTracking()
            .Where(x => chaptersIds.Contains(x.Id))
            .ToArrayAsync(cancellationToken);
        var chaptersProgresses = new List<ChapterProgressDto>();
        foreach (var chapter in chapters)
        {
            DateTime? completedDate = null;
            if (chapter.Lessons.All(x => user.UserLessons.Any(ul => ul.LessonId == x.Id)))
            {
                var completedChapterLessons = completedLessons
                    .Where(x => x.ChapterId == chapter.Id)
                    .ToArray();
                var userChapter = new UserChapter()
                {
                    UserId = user.Id,
                    ChapterId = chapter.Id,
                    CompletedDate = completedChapterLessons
                        .Select(x => x.CompleteDate)
                        .OrderDescending()
                        .FirstOrDefault(),
                };
                completedDate = userChapter.CompletedDate;
                user.UserChapters.Add(userChapter);
            }
            
            chaptersProgresses.Add(new ChapterProgressDto(chapter.CourseId, completedDate));
        }

        return chaptersProgresses.ToArray();
    }

    private async Task<CourseProgressDto[]> TrackCoursesAsync(User user, ChapterProgressDto[] chaptersProgresses, CancellationToken cancellationToken)
    {
        var coursesIds = chaptersProgresses.Select(x => x.CourseId).Distinct().ToArray();
        var courses = await dbContext.Courses
            .AsNoTracking()
            .Where(x => coursesIds.Contains(x.Id)).ToArrayAsync(cancellationToken);
        var coursesProgresses = new List<CourseProgressDto>();
        foreach (var course in courses)
        {
            var completedChapters = chaptersProgresses
                .Where(x => x.CourseId == course.Id && x.CompleteDate.HasValue)
                .ToArray();
            if (course.Chapters.All(x => user.UserChapters.Any(uc => uc.ChapterId == x.Id)))
            {
                var completeDate = completedChapters
                    .Select(x => x.CompleteDate)
                    .OrderDescending()
                    .FirstOrDefault();

                if (completeDate.HasValue)
                {
                    var userCourse = new UserCourse()
                    {
                        UserId = user.Id,
                        CourseId = course.Id,
                        CompletedDate = completeDate.Value,
                    };
                    user.UserCourses.Add(userCourse);
                }
            }
            
            coursesProgresses.Add(new CourseProgressDto(course.Id, completedChapters.Length));
        }
        
        return coursesProgresses.ToArray();
    }
}
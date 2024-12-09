using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleMimo.Data.Entities;
using SimpleMimo.IntegrationTests.Fixtures;
using SimpleMimo.Models;

namespace SimpleMimo.IntegrationTests;

[Collection("Mimo")]
public class UserAchievementsTests(MimoWebApplicationFactory webApplicationFactory)
    : BaseIntegrationTests(webApplicationFactory)
{
    [Fact]
    public async Task ShouldNotReturnAchievementsWhenNoProgressIsTracked()
    {
        // arrange
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        content.Should().NotBeNull()
            .And.BeEmpty();
    }
    
    [Fact]
    public async Task ShouldReturnAllAchievementsAsNotCompletedWhenOneLessonCompleted()
    {
        // arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();

        var chapters = dbContext.Courses.Include(x => x.Chapters).First().Chapters.ToArray();
        var chapterId = chapters.First().Id;
        var lessonId = dbContext.Lessons.First(x => x.ChapterId == chapterId).Id;
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = lessonId, 
                StartDate = DateTime.UtcNow.AddHours(-1),
                CompleteDate = DateTime.UtcNow,
            },
        };
        
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        achievements.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Id.Should().BeGreaterThan(0));
        achievements.Should().HaveCount(6);
        
        var fiveLessonsAchievement = achievements[0];
        var fiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiveLessonsAchievement.Id);
        fiveLessonsAchievementDb.Should().NotBeNull();
        fiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiveLessonsAchievementDb?.Target.Should().Be(5);
        fiveLessonsAchievement.Progress.Should().Be(1);
        fiveLessonsAchievement.Completed.Should().Be(false);
        
        var twentyFiveLessonsAchievement = achievements[1];
        var twentyFiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(twentyFiveLessonsAchievement.Id);
        twentyFiveLessonsAchievementDb.Should().NotBeNull();
        twentyFiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        twentyFiveLessonsAchievementDb?.Target.Should().Be(25);
        twentyFiveLessonsAchievement.Progress.Should().Be(1);
        twentyFiveLessonsAchievement.Completed.Should().Be(false);
        
        var fiftyLessonsAchievement = achievements[2];
        var fiftyLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiftyLessonsAchievement.Id);
        fiftyLessonsAchievementDb.Should().NotBeNull();
        fiftyLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiftyLessonsAchievementDb?.Target.Should().Be(50);
        fiftyLessonsAchievement.Progress.Should().Be(1);
        fiftyLessonsAchievement.Completed.Should().Be(false);
        
        var oneChapterAchievement = achievements[3];
        var oneChapterAchievementDb = await dbContext.Achievements.FindAsync(oneChapterAchievement.Id);
        oneChapterAchievementDb.Should().NotBeNull();
        oneChapterAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        oneChapterAchievementDb?.Target.Should().Be(1);
        oneChapterAchievement.Progress.Should().Be(0);
        oneChapterAchievement.Completed.Should().Be(false);
        
        var fiveChaptersAchievement = achievements[4];
        var fiveChaptersAchievementDb = await dbContext.Achievements.FindAsync(fiveChaptersAchievement.Id);
        fiveChaptersAchievementDb.Should().NotBeNull();
        fiveChaptersAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        fiveChaptersAchievementDb?.Target.Should().Be(5);
        fiveChaptersAchievement.Progress.Should().Be(0);
        fiveChaptersAchievement.Completed.Should().Be(false);
        
        var swiftCourseAchievement = achievements[5];
        var swiftCourseAchievementDb = await dbContext.Achievements.FindAsync(swiftCourseAchievement.Id);
        swiftCourseAchievementDb.Should().NotBeNull();
        swiftCourseAchievementDb?.Category.Should().Be(AchievementCategory.Course);
        swiftCourseAchievementDb?.Target.Should().Be(chapters.Length);
        swiftCourseAchievement.Progress.Should().Be(0);
        swiftCourseAchievement.Completed.Should().Be(false);
    }
    
    [Fact]
    public async Task ShouldNotUpdateAchievementsWhenOneLessonCompletedTwice()
    {
        // arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();

        var chapters = dbContext.Courses.Include(x => x.Chapters).First().Chapters.ToArray();
        var chapterId = chapters.First().Id;
        var lessonId = dbContext.Lessons.First(x => x.ChapterId == chapterId).Id;
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = lessonId, 
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
            new()
            {
                Id = lessonId, 
                StartDate = DateTime.UtcNow.AddHours(-1),
                CompleteDate = DateTime.UtcNow,
            },
        };
        
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        achievements.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Id.Should().BeGreaterThan(0));
        achievements.Should().HaveCount(6);
        
        var fiveLessonsAchievement = achievements[0];
        var fiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiveLessonsAchievement.Id);
        fiveLessonsAchievementDb.Should().NotBeNull();
        fiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiveLessonsAchievementDb?.Target.Should().Be(5);
        fiveLessonsAchievement.Progress.Should().Be(1);
        fiveLessonsAchievement.Completed.Should().Be(false);
        
        var twentyFiveLessonsAchievement = achievements[1];
        var twentyFiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(twentyFiveLessonsAchievement.Id);
        twentyFiveLessonsAchievementDb.Should().NotBeNull();
        twentyFiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        twentyFiveLessonsAchievementDb?.Target.Should().Be(25);
        twentyFiveLessonsAchievement.Progress.Should().Be(1);
        twentyFiveLessonsAchievement.Completed.Should().Be(false);
        
        var fiftyLessonsAchievement = achievements[2];
        var fiftyLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiftyLessonsAchievement.Id);
        fiftyLessonsAchievementDb.Should().NotBeNull();
        fiftyLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiftyLessonsAchievementDb?.Target.Should().Be(50);
        fiftyLessonsAchievement.Progress.Should().Be(1);
        fiftyLessonsAchievement.Completed.Should().Be(false);
        
        var oneChapterAchievement = achievements[3];
        var oneChapterAchievementDb = await dbContext.Achievements.FindAsync(oneChapterAchievement.Id);
        oneChapterAchievementDb.Should().NotBeNull();
        oneChapterAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        oneChapterAchievementDb?.Target.Should().Be(1);
        oneChapterAchievement.Progress.Should().Be(0);
        oneChapterAchievement.Completed.Should().Be(false);
        
        var fiveChaptersAchievement = achievements[4];
        var fiveChaptersAchievementDb = await dbContext.Achievements.FindAsync(fiveChaptersAchievement.Id);
        fiveChaptersAchievementDb.Should().NotBeNull();
        fiveChaptersAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        fiveChaptersAchievementDb?.Target.Should().Be(5);
        fiveChaptersAchievement.Progress.Should().Be(0);
        fiveChaptersAchievement.Completed.Should().Be(false);
        
        var swiftCourseAchievement = achievements[5];
        var swiftCourseAchievementDb = await dbContext.Achievements.FindAsync(swiftCourseAchievement.Id);
        swiftCourseAchievementDb.Should().NotBeNull();
        swiftCourseAchievementDb?.Category.Should().Be(AchievementCategory.Course);
        swiftCourseAchievementDb?.Target.Should().Be(chapters.Length);
        swiftCourseAchievement.Progress.Should().Be(0);
        swiftCourseAchievement.Completed.Should().Be(false);
    }
    
    [Fact]
    public async Task ShouldIncrementLessonsAchievementWhenOneMoreLessonCompleted()
    {
        // arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();

        var chapters = dbContext.Courses.Include(x => x.Chapters).First().Chapters.ToArray();
        var chapterId = chapters.First().Id;
        var lessons = dbContext.Lessons.Where(x => x.ChapterId == chapterId).ToArray();
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = lessons[0].Id, 
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
        };
        
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        request =
        [
            new CompletedLessonRequest
            {
                Id = lessons[1].Id,
                StartDate = DateTime.UtcNow.AddHours(-1),
                CompleteDate = DateTime.UtcNow,
            },
        ];
        
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        achievements.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Id.Should().BeGreaterThan(0));
        achievements.Should().HaveCount(6);
        
        var fiveLessonsAchievement = achievements[0];
        var fiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiveLessonsAchievement.Id);
        fiveLessonsAchievementDb.Should().NotBeNull();
        fiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiveLessonsAchievementDb?.Target.Should().Be(5);
        fiveLessonsAchievement.Progress.Should().Be(2);
        fiveLessonsAchievement.Completed.Should().Be(false);
        
        var twentyFiveLessonsAchievement = achievements[1];
        var twentyFiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(twentyFiveLessonsAchievement.Id);
        twentyFiveLessonsAchievementDb.Should().NotBeNull();
        twentyFiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        twentyFiveLessonsAchievementDb?.Target.Should().Be(25);
        twentyFiveLessonsAchievement.Progress.Should().Be(2);
        twentyFiveLessonsAchievement.Completed.Should().Be(false);
        
        var fiftyLessonsAchievement = achievements[2];
        var fiftyLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiftyLessonsAchievement.Id);
        fiftyLessonsAchievementDb.Should().NotBeNull();
        fiftyLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiftyLessonsAchievementDb?.Target.Should().Be(50);
        fiftyLessonsAchievement.Progress.Should().Be(2);
        fiftyLessonsAchievement.Completed.Should().Be(false);
        
        var oneChapterAchievement = achievements[3];
        var oneChapterAchievementDb = await dbContext.Achievements.FindAsync(oneChapterAchievement.Id);
        oneChapterAchievementDb.Should().NotBeNull();
        oneChapterAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        oneChapterAchievementDb?.Target.Should().Be(1);
        oneChapterAchievement.Progress.Should().Be(0);
        oneChapterAchievement.Completed.Should().Be(false);
        
        var fiveChaptersAchievement = achievements[4];
        var fiveChaptersAchievementDb = await dbContext.Achievements.FindAsync(fiveChaptersAchievement.Id);
        fiveChaptersAchievementDb.Should().NotBeNull();
        fiveChaptersAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        fiveChaptersAchievementDb?.Target.Should().Be(5);
        fiveChaptersAchievement.Progress.Should().Be(0);
        fiveChaptersAchievement.Completed.Should().Be(false);
        
        var swiftCourseAchievement = achievements[5];
        var swiftCourseAchievementDb = await dbContext.Achievements.FindAsync(swiftCourseAchievement.Id);
        swiftCourseAchievementDb.Should().NotBeNull();
        swiftCourseAchievementDb?.Category.Should().Be(AchievementCategory.Course);
        swiftCourseAchievementDb?.Target.Should().Be(chapters.Length);
        swiftCourseAchievement.Progress.Should().Be(0);
        swiftCourseAchievement.Completed.Should().Be(false);
    }
    
    [Fact]
    public async Task ShouldCompleteAchievementWhenChapterIsCompleted()
    {
        // arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();

        var chapters = dbContext.Courses.Include(x => x.Chapters).First().Chapters.ToArray();
        var chapterId = chapters.First().Id;
        var allChapterLessons = dbContext.Lessons.Where(x => x.ChapterId == chapterId).ToArray();
        var request = allChapterLessons.Select(x => new CompletedLessonRequest()
        {
            Id = x.Id,
            StartDate = DateTime.UtcNow.AddHours(-1),
            CompleteDate = DateTime.UtcNow,
        });
        
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        achievements.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Id.Should().BeGreaterThan(0));
        achievements.Should().HaveCount(6);
        
        var fiveLessonsAchievement = achievements[0];
        var fiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiveLessonsAchievement.Id);
        fiveLessonsAchievementDb.Should().NotBeNull();
        fiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiveLessonsAchievementDb?.Target.Should().Be(5);
        fiveLessonsAchievement.Progress.Should().Be(3);
        fiveLessonsAchievement.Completed.Should().Be(false);
        
        var twentyFiveLessonsAchievement = achievements[1];
        var twentyFiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(twentyFiveLessonsAchievement.Id);
        twentyFiveLessonsAchievementDb.Should().NotBeNull();
        twentyFiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        twentyFiveLessonsAchievementDb?.Target.Should().Be(25);
        twentyFiveLessonsAchievement.Progress.Should().Be(3);
        twentyFiveLessonsAchievement.Completed.Should().Be(false);
        
        var fiftyLessonsAchievement = achievements[2];
        var fiftyLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiftyLessonsAchievement.Id);
        fiftyLessonsAchievementDb.Should().NotBeNull();
        fiftyLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiftyLessonsAchievementDb?.Target.Should().Be(50);
        fiftyLessonsAchievement.Progress.Should().Be(3);
        fiftyLessonsAchievement.Completed.Should().Be(false);
        
        var oneChapterAchievement = achievements[3];
        var oneChapterAchievementDb = await dbContext.Achievements.FindAsync(oneChapterAchievement.Id);
        oneChapterAchievementDb.Should().NotBeNull();
        oneChapterAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        oneChapterAchievementDb?.Target.Should().Be(1);
        oneChapterAchievement.Progress.Should().Be(1);
        oneChapterAchievement.Completed.Should().Be(true);
        
        var fiveChaptersAchievement = achievements[4];
        var fiveChaptersAchievementDb = await dbContext.Achievements.FindAsync(fiveChaptersAchievement.Id);
        fiveChaptersAchievementDb.Should().NotBeNull();
        fiveChaptersAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        fiveChaptersAchievementDb?.Target.Should().Be(5);
        fiveChaptersAchievement.Progress.Should().Be(1);
        fiveChaptersAchievement.Completed.Should().Be(false);
        
        var swiftCourseAchievement = achievements[5];
        var swiftCourseAchievementDb = await dbContext.Achievements.FindAsync(swiftCourseAchievement.Id);
        swiftCourseAchievementDb.Should().NotBeNull();
        swiftCourseAchievementDb?.Category.Should().Be(AchievementCategory.Course);
        swiftCourseAchievementDb?.Target.Should().Be(chapters.Length);
        swiftCourseAchievement.Progress.Should().Be(1);
        swiftCourseAchievement.Completed.Should().Be(false);
    }
    
    [Fact]
    public async Task ShouldNotChangeChaptersAchievementWhenLessonCompletedSecondTime()
    {
        // arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();

        var chapters = dbContext.Courses.Include(x => x.Chapters).First().Chapters.ToArray();
        var chapterId = chapters.First().Id;
        var allChapterLessons = dbContext.Lessons.Where(x => x.ChapterId == chapterId).ToArray();
        var request = allChapterLessons.Select(x => new CompletedLessonRequest()
        {
            Id = x.Id,
            StartDate = DateTime.UtcNow.AddHours(-1),
            CompleteDate = DateTime.UtcNow,
        });
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        request = allChapterLessons.Take(1).Select(x => new CompletedLessonRequest()
        {
            Id = x.Id,
            StartDate = DateTime.UtcNow.AddHours(-1),
            CompleteDate = DateTime.UtcNow,
        });
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        achievements.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Id.Should().BeGreaterThan(0));
        achievements.Should().HaveCount(6);
        
        var fiveLessonsAchievement = achievements[0];
        var fiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiveLessonsAchievement.Id);
        fiveLessonsAchievementDb.Should().NotBeNull();
        fiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiveLessonsAchievementDb?.Target.Should().Be(5);
        fiveLessonsAchievement.Progress.Should().Be(3);
        fiveLessonsAchievement.Completed.Should().Be(false);
        
        var twentyFiveLessonsAchievement = achievements[1];
        var twentyFiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(twentyFiveLessonsAchievement.Id);
        twentyFiveLessonsAchievementDb.Should().NotBeNull();
        twentyFiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        twentyFiveLessonsAchievementDb?.Target.Should().Be(25);
        twentyFiveLessonsAchievement.Progress.Should().Be(3);
        twentyFiveLessonsAchievement.Completed.Should().Be(false);
        
        var fiftyLessonsAchievement = achievements[2];
        var fiftyLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiftyLessonsAchievement.Id);
        fiftyLessonsAchievementDb.Should().NotBeNull();
        fiftyLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiftyLessonsAchievementDb?.Target.Should().Be(50);
        fiftyLessonsAchievement.Progress.Should().Be(3);
        fiftyLessonsAchievement.Completed.Should().Be(false);
        
        var oneChapterAchievement = achievements[3];
        var oneChapterAchievementDb = await dbContext.Achievements.FindAsync(oneChapterAchievement.Id);
        oneChapterAchievementDb.Should().NotBeNull();
        oneChapterAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        oneChapterAchievementDb?.Target.Should().Be(1);
        oneChapterAchievement.Progress.Should().Be(1);
        oneChapterAchievement.Completed.Should().Be(true);
        
        var fiveChaptersAchievement = achievements[4];
        var fiveChaptersAchievementDb = await dbContext.Achievements.FindAsync(fiveChaptersAchievement.Id);
        fiveChaptersAchievementDb.Should().NotBeNull();
        fiveChaptersAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        fiveChaptersAchievementDb?.Target.Should().Be(5);
        fiveChaptersAchievement.Progress.Should().Be(1);
        fiveChaptersAchievement.Completed.Should().Be(false);
        
        var swiftCourseAchievement = achievements[5];
        var swiftCourseAchievementDb = await dbContext.Achievements.FindAsync(swiftCourseAchievement.Id);
        swiftCourseAchievementDb.Should().NotBeNull();
        swiftCourseAchievementDb?.Category.Should().Be(AchievementCategory.Course);
        swiftCourseAchievementDb?.Target.Should().Be(chapters.Length);
        swiftCourseAchievement.Progress.Should().Be(1);
        swiftCourseAchievement.Completed.Should().Be(false);
    }
    
    [Fact]
    public async Task ShouldCompleteAchievementWhenFiveLessonsAreCompleted()
    {
        // arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();

        var chaptersIds = dbContext.Courses
            .Include(x => x.Chapters)
            .First()
            .Chapters
            .Select(x => x.Id)
            .ToArray();
        var allChaptersLessons = dbContext.Lessons.Where(x => chaptersIds.Contains(x.ChapterId)).ToArray();
        var request = allChaptersLessons.Take(5).Select(x => new CompletedLessonRequest()
        {
            Id = x.Id,
            StartDate = DateTime.UtcNow.AddHours(-1),
            CompleteDate = DateTime.UtcNow,
        });
        
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        achievements.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Id.Should().BeGreaterThan(0));
        achievements.Should().HaveCount(6);
        
        var fiveLessonsAchievement = achievements[0];
        var fiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiveLessonsAchievement.Id);
        fiveLessonsAchievementDb.Should().NotBeNull();
        fiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiveLessonsAchievementDb?.Target.Should().Be(5);
        fiveLessonsAchievement.Progress.Should().Be(5);
        fiveLessonsAchievement.Completed.Should().Be(true);
        
        var twentyFiveLessonsAchievement = achievements[1];
        var twentyFiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(twentyFiveLessonsAchievement.Id);
        twentyFiveLessonsAchievementDb.Should().NotBeNull();
        twentyFiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        twentyFiveLessonsAchievementDb?.Target.Should().Be(25);
        twentyFiveLessonsAchievement.Progress.Should().Be(5);
        twentyFiveLessonsAchievement.Completed.Should().Be(false);
        
        var fiftyLessonsAchievement = achievements[2];
        var fiftyLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiftyLessonsAchievement.Id);
        fiftyLessonsAchievementDb.Should().NotBeNull();
        fiftyLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiftyLessonsAchievementDb?.Target.Should().Be(50);
        fiftyLessonsAchievement.Progress.Should().Be(5);
        fiftyLessonsAchievement.Completed.Should().Be(false);
        
        var oneChapterAchievement = achievements[3];
        var oneChapterAchievementDb = await dbContext.Achievements.FindAsync(oneChapterAchievement.Id);
        oneChapterAchievementDb.Should().NotBeNull();
        oneChapterAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        oneChapterAchievementDb?.Target.Should().Be(1);
        oneChapterAchievement.Progress.Should().Be(1);
        oneChapterAchievement.Completed.Should().Be(true);
        
        var fiveChaptersAchievement = achievements[4];
        var fiveChaptersAchievementDb = await dbContext.Achievements.FindAsync(fiveChaptersAchievement.Id);
        fiveChaptersAchievementDb.Should().NotBeNull();
        fiveChaptersAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        fiveChaptersAchievementDb?.Target.Should().Be(5);
        fiveChaptersAchievement.Progress.Should().Be(1);
        fiveChaptersAchievement.Completed.Should().Be(false);
        
        var swiftCourseAchievement = achievements[5];
        var swiftCourseAchievementDb = await dbContext.Achievements.FindAsync(swiftCourseAchievement.Id);
        swiftCourseAchievementDb.Should().NotBeNull();
        swiftCourseAchievementDb?.Category.Should().Be(AchievementCategory.Course);
        swiftCourseAchievementDb?.Target.Should().Be(chaptersIds.Length);
        swiftCourseAchievement.Progress.Should().Be(1);
        swiftCourseAchievement.Completed.Should().Be(false);
    }
    
        [Fact]
    public async Task ShouldCompleteAchievementWhenCourseCompleted()
    {
        // arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();

        var chaptersIds = dbContext.Courses
            .Include(x => x.Chapters)
            .First()
            .Chapters
            .Select(x => x.Id)
            .ToArray();
        var allChaptersLessons = dbContext.Lessons.Where(x => chaptersIds.Contains(x.ChapterId)).ToArray();
        var request = allChaptersLessons.Select(x => new CompletedLessonRequest()
        {
            Id = x.Id,
            StartDate = DateTime.UtcNow.AddHours(-1),
            CompleteDate = DateTime.UtcNow,
        });
        
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        // act
        var response = await Client.GetAsync("/user/achievements");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<AchievementResponse[]>();
        achievements.Should().NotBeNullOrEmpty()
            .And.AllSatisfy(x => x.Id.Should().BeGreaterThan(0));
        achievements.Should().HaveCount(6);
        
        var fiveLessonsAchievement = achievements[0];
        var fiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiveLessonsAchievement.Id);
        fiveLessonsAchievementDb.Should().NotBeNull();
        fiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiveLessonsAchievementDb?.Target.Should().Be(5);
        fiveLessonsAchievement.Progress.Should().Be(5);
        fiveLessonsAchievement.Completed.Should().Be(true);
        
        var twentyFiveLessonsAchievement = achievements[1];
        var twentyFiveLessonsAchievementDb = await dbContext.Achievements.FindAsync(twentyFiveLessonsAchievement.Id);
        twentyFiveLessonsAchievementDb.Should().NotBeNull();
        twentyFiveLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        twentyFiveLessonsAchievementDb?.Target.Should().Be(25);
        twentyFiveLessonsAchievement.Progress.Should().Be(6);
        twentyFiveLessonsAchievement.Completed.Should().Be(false);
        
        var fiftyLessonsAchievement = achievements[2];
        var fiftyLessonsAchievementDb = await dbContext.Achievements.FindAsync(fiftyLessonsAchievement.Id);
        fiftyLessonsAchievementDb.Should().NotBeNull();
        fiftyLessonsAchievementDb?.Category.Should().Be(AchievementCategory.Lesson);
        fiftyLessonsAchievementDb?.Target.Should().Be(50);
        fiftyLessonsAchievement.Progress.Should().Be(6);
        fiftyLessonsAchievement.Completed.Should().Be(false);
        
        var oneChapterAchievement = achievements[3];
        var oneChapterAchievementDb = await dbContext.Achievements.FindAsync(oneChapterAchievement.Id);
        oneChapterAchievementDb.Should().NotBeNull();
        oneChapterAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        oneChapterAchievementDb?.Target.Should().Be(1);
        oneChapterAchievement.Progress.Should().Be(1);
        oneChapterAchievement.Completed.Should().Be(true);
        
        var fiveChaptersAchievement = achievements[4];
        var fiveChaptersAchievementDb = await dbContext.Achievements.FindAsync(fiveChaptersAchievement.Id);
        fiveChaptersAchievementDb.Should().NotBeNull();
        fiveChaptersAchievementDb?.Category.Should().Be(AchievementCategory.Chapter);
        fiveChaptersAchievementDb?.Target.Should().Be(5);
        fiveChaptersAchievement.Progress.Should().Be(2);
        fiveChaptersAchievement.Completed.Should().Be(false);
        
        var swiftCourseAchievement = achievements[5];
        var swiftCourseAchievementDb = await dbContext.Achievements.FindAsync(swiftCourseAchievement.Id);
        swiftCourseAchievementDb.Should().NotBeNull();
        swiftCourseAchievementDb?.Category.Should().Be(AchievementCategory.Course);
        swiftCourseAchievementDb?.Target.Should().Be(chaptersIds.Length);
        swiftCourseAchievement.Progress.Should().Be(2);
        swiftCourseAchievement.Completed.Should().Be(true);
    }
}
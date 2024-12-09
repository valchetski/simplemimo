using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SimpleMimo.IntegrationTests.Fixtures;
using SimpleMimo.Models;

namespace SimpleMimo.IntegrationTests;

[Collection("Mimo")]
public class UserLessonsTests(MimoWebApplicationFactory webApplicationFactory)
    : BaseIntegrationTests(webApplicationFactory)
{
    [Fact]
    public async Task ShouldTrackLessonsSuccessfullyWhenNoLessonsTrackedBefore()
    {
        // arrange
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = 1, 
                StartDate = DateTime.UtcNow.AddHours(-1),
                CompleteDate = DateTime.UtcNow,
            },
        };
        
        // act
        var response = await Client.PostAsJsonAsync("/user/lessons", request);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEmpty("no response is expected");
    }
    
    [Fact]
    public async Task ShouldTrackAnotherLessonSuccessfullyWhenOneLessonTrackedBefore()
    {
        // arrange
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = 1, 
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
        };
        await Client.PostAsJsonAsync("/user/lessons", request);
        
        request =
        [
            new CompletedLessonRequest
            {
                Id = 2,
                StartDate = DateTime.UtcNow.AddHours(-1),
                CompleteDate = DateTime.UtcNow,
            },
        ];
        
        // act
        var response = await Client.PostAsJsonAsync("/user/lessons", request);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEmpty("no response is expected");
    }

    [Fact]
    public async Task ShouldHandleInvalidRequest()
    {
        // arrange
        // act
        var response = await Client.PostAsJsonAsync("/user/lessons", "invalid");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestWithoutLessonId()
    {
        // arrange
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
            new()
            {
                Id = 1,
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
        };
        
        // act
        var response = await Client.PostAsJsonAsync("/user/lessons", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestWithoutStartDate()
    {
        // arrange
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = 1,
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
            new()
            {
                Id = 1,
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
        };
        
        // act
        var response = await Client.PostAsJsonAsync("/user/lessons", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestWithoutEndDate()
    {
        // arrange
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = 1,
                StartDate = DateTime.UtcNow.AddHours(-3),
            },
            new()
            {
                Id = 1,
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
        };
        
        // act
        var response = await Client.PostAsJsonAsync("/user/lessons", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ShouldReturnNotFoundWhenLessonDoesntExist()
    {
        // arrange
        var request = new List<CompletedLessonRequest>()
        {
            new()
            {
                Id = -1,
                StartDate = DateTime.UtcNow.AddHours(-3),
                CompleteDate = DateTime.UtcNow.AddHours(-2),
            },
        };
        
        // act
        var response = await Client.PostAsJsonAsync("/user/lessons", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content.Message.Should().NotBeNullOrEmpty();
        content.Errors.Should().NotBeNull()
            .And.BeEmpty();
    }
}
using SimpleMimo.Models;

namespace SimpleMimo.Services;

public interface IUserProgressService
{
    Task TrackAsync(long userId, CompletedLessonRequest[] completedLessons, CancellationToken cancellationToken);
}
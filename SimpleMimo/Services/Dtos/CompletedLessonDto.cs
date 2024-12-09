namespace SimpleMimo.Services.Dtos;

public record CompletedLessonDto(long LessonId, long ChapterId, DateTime CompleteDate)
{
}
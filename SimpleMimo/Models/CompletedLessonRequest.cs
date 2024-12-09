namespace SimpleMimo.Models;

public class CompletedLessonRequest
{
    public long Id { get; init; }
    
    public DateTime StartDate { get; init; }
    
    public DateTime CompleteDate { get; init; }
}
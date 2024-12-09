using Microsoft.EntityFrameworkCore;

namespace SimpleMimo.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(ChapterId))]
public class UserChapter
{
    public required long UserId { get; init; }
    
    public required long ChapterId { get; init; }
    
    public required DateTime CompletedDate { get; init; }
}
using Microsoft.EntityFrameworkCore;

namespace SimpleMimo.Data.Entities;

[PrimaryKey(nameof(UserId),nameof(AchievementId))]
public class UserAchievement
{
    public required long UserId { get; init; }
    
    public required long AchievementId { get; init; }
    
    public required int Progress { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public User User { get; init; } = null!;
    
    public Achievement Achievement { get; init; } = null!;
}
namespace SimpleMimo.Services;

public class UserService : IUserService
{
    public long GetCurrentUserId()
    {
        // in this test task we hardcode user id
        // in real-life scenario it would get some user identifier from JWT token for example
        return 1;
    }
}
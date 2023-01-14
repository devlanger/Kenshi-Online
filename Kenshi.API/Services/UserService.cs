namespace Kenshi.API.Services;

public class UserService
{
    /// <summary>
    /// <username, playerId>
    /// </summary>
    public static Dictionary<string, string> userIds = new Dictionary<string, string>();

    public string GetConnectionIdByUsername(string username)
    {
        if (!userIds.ContainsKey(username))
        {
            return "";
        }
        return userIds[username];
    }
}
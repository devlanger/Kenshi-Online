namespace Kenshi.API.Services;

public class UserService
{
    /// <summary>
    /// <username, playerId>
    /// </summary>
    public static Dictionary<string, string> userIds = new Dictionary<string, string>();

    public static List<string> UsersInLobby = new List<string>();
    
    public string GetConnectionIdByUsername(string username)
    {
        if (!userIds.ContainsKey(username))
        {
            return "";
        }
        return userIds[username];
    }
}
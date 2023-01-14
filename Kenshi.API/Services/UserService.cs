namespace Kenshi.API.Services;

public class UserService
{
    /// <summary>
    /// <username, playerId>
    /// </summary>
    public static Dictionary<string, string> userIds = new Dictionary<string, string>();

    public static List<string> UsersInLobby = new List<string>();
    public static List<string> LoggedUsers = new List<string>();

    public static bool IsUserLogged(string username)
    {
        return LoggedUsers.Contains(username);
    }
    
    public string GetConnectionIdByUsername(string username)
    {
        if (!userIds.ContainsKey(username))
        {
            return "";
        }
        return userIds[username];
    }
}
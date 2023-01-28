using Prometheus;

namespace Kenshi.API;

public class MetricsService
{
    private readonly Gauge _usersLoggedIn = Prometheus.Metrics.CreateGauge("users_logged_in", "The number of users currently logged in");
    private readonly Histogram _userLoginHistogram = Prometheus.Metrics.CreateHistogram("users_logged_logs", "User logged logs");

    public void SetPlayersCount(int players)
    {
        _usersLoggedIn.Set(players);
    }
    
    public void PlayerLoggedState(int players)
    {
        _usersLoggedIn.Set(players);
    }
}
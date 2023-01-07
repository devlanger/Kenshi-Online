namespace Kenshi.Shared.Models
{
    public class ContainerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Port { get; set; }
        public int PlayersCount { get; set; }
        public int MaxPlayersCount { get; set; } = 10;
    }
}
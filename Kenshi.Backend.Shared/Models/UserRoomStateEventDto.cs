namespace Kenshi.Backend.Shared.Models
{
    public enum RoomEventState : byte
    {
        Joined = 1,
        Left = 2,
    }
    
    public class UserRoomStateEventDto
    {
        public string RoomId;
        public string Username;
        public RoomEventState State;
    }
}
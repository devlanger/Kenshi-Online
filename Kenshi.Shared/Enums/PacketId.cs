namespace Kenshi.Shared.Enums
{
    public enum PacketId : byte
    {
        LoginRequest = 1,
        LoginResponse = 2,
        LoginEvent = 3,
        PositionUpdateRequest = 4,
        PositionUpdateEvent = 5,
        LogoutEvent = 6
    }
}
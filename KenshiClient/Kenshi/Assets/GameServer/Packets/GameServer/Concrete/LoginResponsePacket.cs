using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginResponsePacket : SendablePacket
    {
        public override PacketId packetId => PacketId.LoginResponse;
        public int _playerId;

        public LoginResponsePacket()
        {
        }

        public LoginResponsePacket(int playerId)
        {
            _playerId = playerId;
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(_playerId);
        }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.GetInt();
        }
    }
}
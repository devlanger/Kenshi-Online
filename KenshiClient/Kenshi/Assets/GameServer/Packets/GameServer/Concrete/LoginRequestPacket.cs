using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginRequestPacket : SendablePacket
    {
        
        public override PacketId packetId => PacketId.LoginRequest;
        public LoginRequestPacket()
        {
            
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.GetInt();
        }

        public int _playerId { get; set; }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(_playerId);
        }
    }
}
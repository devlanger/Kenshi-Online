using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginEventPacket : SendablePacket
    {
        public int _playerId;

        public override PacketId packetId => PacketId.LoginEvent;

        public LoginEventPacket()
        {
            
        }
        public LoginEventPacket(int playerId)
        {
            _playerId = playerId;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.GetInt();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(_playerId);
        }
    }
}
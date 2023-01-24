using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginEventPacket : SendablePacket
    {
        public int _playerId;
        public string username;

        public override PacketId packetId => PacketId.LoginEvent;

        public LoginEventPacket()
        {
            
        }
        public LoginEventPacket(int playerId, string username)
        {
            _playerId = playerId;
            this.username = username;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.GetInt();
            username = reader.GetString();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(_playerId);
            writer.Put(username);
        }
    }
}
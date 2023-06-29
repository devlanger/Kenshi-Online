using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginEventPacket : SendablePacket
    {
        public class LoginData
        {
            public int _playerId;
            public string username;
            public bool isBot;
        }

        public LoginData data;

        public override PacketId packetId => PacketId.LoginEvent;

        public LoginEventPacket()
        {
            
        }
        public LoginEventPacket(LoginData data)
        {
            this.data = data;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            data = new LoginData()
            {
                _playerId = reader.GetInt(),
                username = reader.GetString(),
                isBot = reader.GetBool(),
            };
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(data._playerId);
            writer.Put(data.username);
            writer.Put(data.isBot);
        }
    }
}
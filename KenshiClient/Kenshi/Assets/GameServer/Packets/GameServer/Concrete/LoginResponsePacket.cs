using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginResponsePacket : SendablePacket
    {
        public override PacketId packetId => PacketId.LoginResponse;
        public int _playerId;

        public Data data = new Data();
        
        public class Data
        {
            public string mapId;
        }

        public LoginResponsePacket()
        {
        }

        public LoginResponsePacket(int playerId, Data data)
        {
            _playerId = playerId;
            this.data = data;
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(_playerId);
            writer.Put(data.mapId);
        }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.GetInt();
            data = new Data();
            data.mapId = reader.GetString();
        }
    }
}
using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class DeathmatchModeEndPacket : SendablePacket
    {
        private Data _data;

        public class Data
        {
            
        }
        
        public override PacketId packetId => PacketId.DeathmatchModeEnd;

        public DeathmatchModeEndPacket()
        {
            
        }
        public DeathmatchModeEndPacket(Data data)
        {
            _data = data;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
        }
    }
}
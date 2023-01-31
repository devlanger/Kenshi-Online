using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class StatEventPacket : SendablePacket
    {
        public enum StatId
        {
            health = 1,
            mana = 2,
            stamina = 3,
            experience = 4,
            level = 5,
            username = 6,
        }
        
        public class Data
        {
            public int playerId;
            public StatId statId;
            public object value;
            public object maxValue;
        }

        public Data data = new Data();
        
        public override PacketId packetId => PacketId.StatEvent;

        public StatEventPacket()
        {
            
        }
        public StatEventPacket(Data data)
        {
            this.data = data;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            data.playerId = reader.GetInt();
            data.statId = (StatId)reader.GetByte();
            switch (data.statId)
            {
                case StatId.health:
                case StatId.mana:
                    data.value = reader.GetUShort();
                    data.maxValue = reader.GetUShort();
                    break;
            }
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(data.playerId);
            writer.Put((byte)data.statId);
            switch (data.statId)
            {
                case StatId.health:
                case StatId.mana:
                    writer.Put((ushort)data.value);
                    writer.Put((ushort)data.maxValue);
                    break;
            }
        }
    }
}
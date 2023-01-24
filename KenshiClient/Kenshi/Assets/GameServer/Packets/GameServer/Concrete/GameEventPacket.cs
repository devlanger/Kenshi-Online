using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class GameEventPacket : SendablePacket
    {
        public override PacketId packetId => PacketId.GameEventPacket;

        public enum GameEventId
        {
            player_died = 1,
        }
        
        public GameEventId eventId;
        
        public class Data
        {
            public int playerId;
        }

        public class PlayerDied : Data
        {
            public string attackerName;
            public string targetName;
            public DeathType dt;
            
            public enum DeathType
            {
                melee = 1,
                skill = 2,
                self = 3,
            }
        }

        public Data data = new Data();
        public PlayerDied diedData = new PlayerDied();

        public GameEventPacket()
        {
            
        }
        
        public GameEventPacket(PlayerDied data)
        {
            eventId = GameEventId.player_died;
            this.diedData = data;
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)eventId);
            switch (eventId)
            {
                case GameEventId.player_died:
                    writer.Put(diedData.attackerName);
                    writer.Put(diedData.targetName);
                    writer.Put((byte)diedData.dt);
                    break;
            }
        }

        public override void Deserialize(NetDataReader reader)
        {
            eventId = (GameEventId)reader.GetByte();
            switch (eventId)
            {
                case GameEventId.player_died:
                    diedData = new PlayerDied()
                    {
                        attackerName = reader.GetString(),
                        targetName = reader.GetString(),
                        dt = (PlayerDied.DeathType)reader.GetByte(),
                    };
                    break;
            }
        }
    }
}
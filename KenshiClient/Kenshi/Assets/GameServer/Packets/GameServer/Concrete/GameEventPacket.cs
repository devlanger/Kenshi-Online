using System.Threading;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;
using UnityEngine;

namespace Kenshi.Shared.Packets.GameServer
{
    public class GameEventPacket : SendablePacket
    {
        public override PacketId packetId => PacketId.GameEventPacket;

        public enum GameEventId
        {
            player_died = 1,
            player_respawn = 2,
            score_changed = 3,
        }
        
        public GameEventId eventId;

        public class PlayerRespawn
        {
            public int playerId;
            public Vector3 respawnPos;
        }
        
        public class ScoreChanged
        {
            public string nickname;
            public int score;
            public int maxScore;
        }
        
        public class PlayerDied
        {
            public int playerId;
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

        public PlayerDied diedData = new PlayerDied();
        public PlayerRespawn respawnData = new PlayerRespawn();
        public ScoreChanged scoreChanged = new ScoreChanged();

        public GameEventPacket()
        {
            
        }
        
        public GameEventPacket(PlayerRespawn data)
        {
            eventId = GameEventId.player_respawn;
            this.respawnData = data;
        }
        
        public GameEventPacket(PlayerDied data)
        {
            eventId = GameEventId.player_died;
            this.diedData = data;
        }
        
        public GameEventPacket(ScoreChanged data)
        {
            eventId = GameEventId.score_changed;
            this.scoreChanged = data;
        }
        
        public override void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)eventId);
            switch (eventId)
            {
                case GameEventId.player_died:
                    writer.Put(diedData.playerId);
                    writer.Put(diedData.attackerName);
                    writer.Put(diedData.targetName);
                    writer.Put((byte)diedData.dt);
                    break;
                
                case GameEventId.player_respawn:
                    writer.Put(respawnData.playerId);
                    PutVector3(respawnData.respawnPos);
                    break;
                
                case GameEventId.score_changed:
                    writer.Put(scoreChanged.nickname);
                    writer.Put(scoreChanged.score);
                    writer.Put(scoreChanged.maxScore);
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
                        playerId = reader.GetInt(),
                        attackerName = reader.GetString(),
                        targetName = reader.GetString(),
                        dt = (PlayerDied.DeathType)reader.GetByte(),
                    };
                    break;
                
                case GameEventId.player_respawn:
                    respawnData = new PlayerRespawn()
                    {
                        playerId = reader.GetInt(),
                        respawnPos = ReadVector3(reader)
                    };
                    break;
                
                case GameEventId.score_changed:
                    scoreChanged = new ScoreChanged()
                    {
                        nickname = reader.GetString(),
                        score = reader.GetInt(),
                        maxScore = reader.GetInt(),
                    };
                    break;
            }
        }
    }
}
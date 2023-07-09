using System.IO;
using DefaultNamespace;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Models;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class GameModeEventPacket : SendablePacket
    {
        public DeathmatchMode.Data _dmData;
        public TeamDeathmatchMode.Data _tmData;

        public GameType GameType;
        
        public override PacketId packetId => PacketId.GameModeEvent;

        public GameModeEventPacket()
        {
            
        }
        
        public GameModeEventPacket(DeathmatchMode.Data dmData)
        {
            GameType = GameType.DEATHMATCH;
            _dmData = dmData;
        }

        public GameModeEventPacket(TeamDeathmatchMode.Data tmData)
        {
            GameType = GameType.TEAM_DEATHMATCH;
            _tmData = tmData;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            GameType = (GameType)reader.GetByte();
            switch (GameType)
            {
                case GameType.DEATHMATCH:
                    _dmData = new DeathmatchMode.Data()
                    {
                        winnerUsername = reader.GetString(),
                        winnerScore = reader.GetInt(),
                        currentScore = reader.GetInt(),
                        scoreToFinish = reader.GetInt(),
                        finished = reader.GetBool(),
                        scores = reader.GetList<DeathmatchMode.PlayerScore>(score =>
                        {
                            score.username = reader.GetString();
                            score.deaths = reader.GetInt();
                            score.kills = reader.GetInt();
                        })
                    };
                    break;
                case GameType.TEAM_DEATHMATCH:
                    _tmData = new TeamDeathmatchMode.Data()
                    {
                        winnerScore = reader.GetInt(),
                        scoreToFinish = reader.GetInt(),
                        finished = reader.GetBool(),
                        blueTeamData = new TeamDeathmatchMode.TeamData()
                        {
                          score = reader.GetInt(),
                          scores = reader.GetList<TeamDeathmatchMode.PlayerScore>(score =>
                          {
                              score.username = reader.GetString();
                              score.deaths = reader.GetInt();
                              score.kills = reader.GetInt();
                          })
                        },
                        redTeamData = new TeamDeathmatchMode.TeamData()
                        {
                            score = reader.GetInt(),
                            scores = reader.GetList<TeamDeathmatchMode.PlayerScore>(score =>
                            {
                                score.username = reader.GetString();
                                score.deaths = reader.GetInt();
                                score.kills = reader.GetInt();
                            })
                        }
                    };
                    break;
            }
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put((byte)GameType);
            switch (GameType)
            {
                case GameType.DEATHMATCH:
                    writer.Put(_dmData.winnerUsername);
                    writer.Put(_dmData.winnerScore);
                    writer.Put(_dmData.currentScore);
                    writer.Put(_dmData.scoreToFinish);
                    writer.Put(_dmData.finished);
                    writer.PutList(_dmData.scores, score =>
                    {
                        writer.Put(score.username);
                        writer.Put(score.deaths);
                        writer.Put(score.kills);
                    });
                    break;
                case GameType.TEAM_DEATHMATCH:
                    writer.Put(_tmData.winnerScore);
                    writer.Put(_tmData.scoreToFinish);
                    writer.Put(_tmData.finished);
                    writer.Put(_tmData.blueTeamData.score);
                    writer.PutList(_tmData.blueTeamData.scores, score =>
                    {
                        writer.Put(score.username);
                        writer.Put(score.deaths);
                        writer.Put(score.kills);
                    });
                    writer.Put(_tmData.redTeamData.score);
                    writer.PutList(_tmData.redTeamData.scores, score =>
                    {
                        writer.Put(score.username);
                        writer.Put(score.deaths);
                        writer.Put(score.kills);
                    });
                    break;
            }
        }
    }
}
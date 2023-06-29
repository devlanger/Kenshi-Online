using System.IO;
using DefaultNamespace;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class DeathmatchModeEventPacket : SendablePacket
    {
        public DeathmatchMode.Data _data;

        public override PacketId packetId => PacketId.DeathmatchModeEnd;

        public DeathmatchModeEventPacket()
        {
            
        }
        public DeathmatchModeEventPacket(DeathmatchMode.Data data)
        {
            _data = data;
        }
        
        public DeathmatchModeEventPacket(DeathmatchMode deathmatchMode)
        {
            _data = deathmatchMode.data;
        }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);

            _data = new DeathmatchMode.Data()
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
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            
            writer.Put(_data.winnerUsername);
            writer.Put(_data.winnerScore);
            writer.Put(_data.currentScore);
            writer.Put(_data.scoreToFinish);
            writer.Put(_data.finished);
            writer.PutList(_data.scores, score =>
            {
                writer.Put(score.username);
                writer.Put(score.deaths);
                writer.Put(score.kills);
            });
        }
    }
}
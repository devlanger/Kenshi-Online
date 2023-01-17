using System.IO;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LogoutEventPacket : SendablePacket
    {
        public override PacketId packetId => PacketId.LogoutEvent;
        public int PlayerId { get; set; }

        public LogoutEventPacket()
        {
            
        }

        public LogoutEventPacket(int playerId)
        {
            PlayerId = playerId;
        }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            PlayerId = reader.GetInt();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(PlayerId);
        }
    }
}
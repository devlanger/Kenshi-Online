using System.IO;
using Kenshi.Shared.Enums;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LogoutEventPacket : SendablePacket
    {
        public uint PlayerId { get; set; }

        public LogoutEventPacket() : base(PacketId.LogoutEvent)
        {
            
        }

        public LogoutEventPacket(uint playerId) : base(PacketId.LogoutEvent)
        {
            PlayerId = playerId;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            PlayerId = reader.ReadUInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(PlayerId);
        }
    }
}
using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;

namespace Kenshi.Shared.Packets.GameServer
{
    public abstract class SendablePacket : ISendable
    {
        public byte packetId;
        
        public SendablePacket(PacketId packetId)
        {
            this.packetId = (byte)packetId;
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(packetId);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            packetId = reader.ReadByte();
        }
    }
}
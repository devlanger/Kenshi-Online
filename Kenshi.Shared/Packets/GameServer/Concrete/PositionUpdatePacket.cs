using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;

namespace Kenshi.Shared.Packets.GameServer
{
    public class PositionUpdatePacket : SendablePacket
    {
        public uint playerId;
        public float x;
        public float y;
        public float z;
        public byte rotY;

        public PositionUpdatePacket() : base(PacketId.PositionUpdateEvent)
        {
            
        }
        
        public PositionUpdatePacket(uint playerId, float x, float y, float z, byte rotY) : base(PacketId.PositionUpdateEvent)
        {
            this.playerId = playerId;
            this.x = x;
            this.y = y;
            this.z = z;
            this.rotY = rotY;
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            
            writer.Write(playerId);
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(rotY);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);

            playerId = reader.ReadUInt32();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            rotY = reader.ReadByte();
        }
    }
}
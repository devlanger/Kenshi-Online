using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;

namespace Kenshi.Shared.Packets.GameServer
{
    public class PositionUpdateRequestPacket : SendablePacket
    {
        public int playerId;
        public float x;
        public float y;
        public float z;
        public byte rotY;

        public PositionUpdateRequestPacket() : base(PacketId.PositionUpdateRequest)
        {
            
        }
        
        public PositionUpdateRequestPacket(int playerId, float x, float y, float z, byte rotY) : base(PacketId.PositionUpdateRequest)
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

            playerId = reader.ReadInt32();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            rotY = reader.ReadByte();
        }
    }
}
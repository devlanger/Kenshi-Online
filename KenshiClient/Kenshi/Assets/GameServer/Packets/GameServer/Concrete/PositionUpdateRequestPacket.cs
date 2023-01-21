using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public class PositionUpdateRequestPacket : SendablePacket
    {
        public int playerId;
        public float x;
        public float y;
        public float z;
        public byte rotY;
        public float speed;
        
        public PositionUpdateRequestPacket()
        {
            
        }
        
        public PositionUpdateRequestPacket(int playerId, float x, float y, float z, byte rotY, float speed)
        {
            this.playerId = playerId;
            this.x = x;
            this.y = y;
            this.z = z;
            this.rotY = rotY;
            this.speed = speed;
        }

        public override PacketId packetId => PacketId.PositionUpdateRequest;
        
        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            
            writer.Put(playerId);
            writer.Put(x);
            writer.Put(y);
            writer.Put(z);
            writer.Put(rotY);
            writer.Put(speed);
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);

            playerId = reader.GetInt();
            x = reader.GetFloat();
            y = reader.GetFloat();
            z = reader.GetFloat();
            rotY = reader.GetByte();
            speed = reader.GetFloat();
        }
    }
}
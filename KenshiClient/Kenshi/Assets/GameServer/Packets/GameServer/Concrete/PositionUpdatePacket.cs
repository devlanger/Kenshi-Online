using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;
using LiteNetLib.Utils;
using UnityEngine;

namespace Kenshi.Shared.Packets.GameServer
{
    public class PositionUpdatePacket : SendablePacket
    {
        public override PacketId packetId => PacketId.PositionUpdateEvent;
        public int playerId;
        public float x;
        public float y;
        public float z;
        public byte rotY;

        public Vector3 Position => new Vector3(x, y, z); 
        
        public PositionUpdatePacket()
        {
            
        }
        
        public PositionUpdatePacket(int playerId, float x, float y, float z, byte rotY)
        {
            this.playerId = playerId;
            this.x = x;
            this.y = y;
            this.z = z;
            this.rotY = rotY;
        }
        
        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            
            writer.Put(playerId);
            writer.Put(x);
            writer.Put(y);
            writer.Put(z);
            writer.Put(rotY);
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);

            playerId = reader.GetInt();
            x = reader.GetFloat();
            y = reader.GetFloat();
            z = reader.GetFloat();
            rotY = reader.GetByte();
        }
    }
}
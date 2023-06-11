using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;
using LiteNetLib.Utils;
using UnityEngine;

namespace Kenshi.Shared.Packets.GameServer
{
    [System.Serializable]
    public class PositionUpdatePacket : SendablePacket
    {
        public override PacketId packetId => PacketId.PositionUpdateEvent;
        public int playerId;
        public float x;
        public float y;
        public float z;
        public byte rotY;
        public float speed;

        public Vector3 Position => new Vector3(x, y, z); 
        
        public PositionUpdatePacket()
        {
            
        }
        
        public PositionUpdatePacket(int playerId, float x, float y, float z, byte rotY, float speed)
        {
            this.playerId = playerId;
            this.x = x;
            this.y = y;
            this.z = z;
            this.rotY = rotY;
            this.speed = speed;
        }
        
        public PositionUpdatePacket(Player player, float speed)
        {
            this.playerId = player.NetworkId;
            this.x = player.transform.position.x;
            this.y = player.transform.position.y;
            this.z = player.transform.position.z;
            this.rotY = (byte)(player.transform.eulerAngles.y / 5);
            this.speed = speed;
        }
        
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
using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;
using LiteNetLib.Utils;
using UnityEngine;

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
        public Vector3 _inputHitPoint;

        public PositionUpdateRequestPacket()
        {
            
        }
        
        public PositionUpdateRequestPacket(int playerId, float x, float y, float z, byte rotY, float speed,
            Vector3 inputHitPoint)
        {
            this.playerId = playerId;
            this.x = x;
            this.y = y;
            this.z = z;
            this.rotY = rotY;
            this.speed = speed;
            _inputHitPoint = inputHitPoint;
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
            writer.Put(_inputHitPoint.x);
            writer.Put(_inputHitPoint.y);
            writer.Put(_inputHitPoint.z);
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
            _inputHitPoint = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
    }
}
using System.IO;
using System.Numerics;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;
using Unity.VisualScripting;

namespace Kenshi.Shared.Packets.GameServer
{
    public class UpdateFsmStatePacket : SendablePacket
    {
        public override PacketId packetId => PacketId.FsmUpdate;
        public int _playerId;
        public FSMStateId stateId;
        public Vector3 aimPoint;
        
        public UpdateFsmStatePacket() 
        {
        }

        public UpdateFsmStatePacket(int playerId) 
        {
            _playerId = playerId;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.GetInt();
            stateId = (FSMStateId)reader.GetByte();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(_playerId);
            writer.Put((byte)stateId);
            // writer.Put((float)aimPoint.x);
            // writer.Put((float)aimPoint.y);
            // writer.Put((float)aimPoint.z);
        }
    }
}
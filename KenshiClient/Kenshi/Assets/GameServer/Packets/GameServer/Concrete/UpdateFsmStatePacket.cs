using System.IO;
using System.Numerics;
using Kenshi.Shared.Enums;
using LiteNetLib.Utils;
using StarterAssets.CombatStates;
using Unity.VisualScripting;

namespace Kenshi.Shared.Packets.GameServer
{
    public class UpdateFsmStatePacket : SendablePacket
    {
        public override PacketId packetId => PacketId.FsmUpdate;

        public int targetId;
        public FSMStateId stateId;
        public AttackState.Data attackData;
        public HitState.Data hitData;
        
        public UpdateFsmStatePacket() 
        {
        }
        
        public UpdateFsmStatePacket(int targetId, AttackState.Data data)
        {
            this.targetId = targetId;
            stateId = FSMStateId.attack;
            this.attackData = data;
        }
        
        public UpdateFsmStatePacket(int targetId, HitState.Data data)
        {
            this.targetId = targetId;
            stateId = FSMStateId.hit;
            this.hitData = data;
        }
        
        public override void Deserialize(NetDataReader reader)
        {
            stateId = (FSMStateId)reader.GetByte();
            targetId = reader.GetInt();
            
            switch (stateId)
            {
                case FSMStateId.attack:
                    attackData = new AttackState.Data
                    {
                        pos = ReadVector3(reader),
                        rot = reader.GetFloat(),
                    };
                    break;
                case FSMStateId.hit:
                    hitData = new HitState.Data
                    {
                        attackerId = reader.GetInt(),
                        targetId = reader.GetInt(),
                        direction = ReadVector3(reader),
                        hitPos = ReadVector3(reader),
                    };
                    break;
            }
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)stateId);
            writer.Put((int)targetId);

            switch (stateId)
            {
                case FSMStateId.attack:
                    PutVector3(attackData.pos);
                    writer.Put(attackData.rot);
                    break;
                
                case FSMStateId.hit:
                    writer.Put(hitData.attackerId);
                    writer.Put(hitData.targetId);
                    PutVector3(hitData.direction);
                    PutVector3(hitData.hitPos);
                    break;
            }
        }
    }
}
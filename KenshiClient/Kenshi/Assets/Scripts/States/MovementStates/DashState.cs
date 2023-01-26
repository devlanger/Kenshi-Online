using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets.CombatStates;
using UnityEngine;

namespace StarterAssets
{
    public class DashState : FSMState
    {
        private Data data;
        
        public override FSMStateId Id => FSMStateId.dash;

        private bool startedInAir = false;

        public DashState(DashState.Data data)
        {
            this.data = data;
        }

        protected override void OnUpdate(PlayerStateMachine machine)
        {
            if (ElapsedTime > 0.4f)
            {
                if (machine.Variables.Grounded && startedInAir)
                {
                    machine.ChangeState(new LandState());
                }
                else
                {
                    machine.ChangeState(new StandState());
                }
            }
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal && ElapsedTime < 0.4f)
            {
                Vector3 dir = Vector3.zero;
                switch (data.dashIndex)
                {
                    case Data.DashIndex.forward:
                        dir = Camera.main.transform.forward;
                        break;
                    
                    case Data.DashIndex.right:
                        dir = Camera.main.transform.right;
                        break;
                    
                    case Data.DashIndex.left:
                        dir = -Camera.main.transform.right;
                        break;
                    
                    case Data.DashIndex.back:
                        dir = -Camera.main.transform.forward;
                        break;
                }

                if (stateMachine.Variables.Grounded)
                {
                    dir.y = 0;
                    dir = dir.normalized;
                }
                
                stateMachine.Target.tps.SetVelocity(dir * 10);
                if (!stateMachine.Variables.Grounded)
                {
                    stateMachine.Target.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                }
                else
                {
                    Vector3 rot = Camera.main.transform.forward;
                    rot.y = 0;
                    stateMachine.Target.transform.rotation = Quaternion.LookRotation(rot);
                }
            }
        }

        protected override void OnInputUpdate(PlayerStateMachine machine)
        {
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal)
            {
                startedInAir = stateMachine.Variables.Jumping;
                GameRoomNetworkController.SendPacketToServer(new UpdateFsmStatePacket(0, data), DeliveryMethod.ReliableOrdered);
            }

            if (GameServer.IsServer)
            {
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, data), DeliveryMethod.ReliableOrdered);
            }
        }

        protected override void OnExit(PlayerStateMachine machine)
        {
            if (machine.Target.animator != null)
            {
                machine.Target.animator?.SetBool("Grounded", machine.Variables.Grounded);
            }
        }

        public class Data
        {
            public enum DashIndex
            {
                none = 0,
                forward = 1,
                right = 2,
                left = 3,
                back = 4,
            }
            
            public DashIndex dashIndex = DashIndex.none;
        }
    }
}
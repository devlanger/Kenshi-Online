using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets.CombatStates;
using UnityEngine;

namespace StarterAssets
{
    public class DashState : FSMState
    {
        public Data data;
        
        public override FSMStateId Id => FSMStateId.dash;

        public bool StartedInAir { get; private set; } = false;
        public float Duration { get; set; }

        public DashState(DashState.Data data)
        {
            Duration = 0.65f;
            this.data = data;
        }

        public override bool Validate(PlayerStateMachine machine)
        {
            switch (machine.Target.playerStateMachine.CurrentState.Id)
            {
                case FSMStateId.hit:
                case FSMStateId.stunned:
                case FSMStateId.dead:
                    return false;
            }
            
            return base.Validate(machine);
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal && ElapsedTime < Duration - 0.1f)
            {
                float speed = 20;
                Vector3 dir = Vector3.zero;
                switch (data.dashIndex)
                {
                    case Data.DashIndex.forward:
                        speed = 25;
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
                }

                Vector3 direction = dir.normalized * speed;

                if (!stateMachine.Variables.Grounded && data.dashIndex == Data.DashIndex.forward)
                {
                    stateMachine.Target.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                    stateMachine.Target.tps.SetVelocity(direction, false);
                }
                else
                {
                    stateMachine.Target.tps.UpdateGravity();
                    stateMachine.Target.tps.SetVelocity(direction, true);
                    Vector3 rot = Camera.main.transform.forward;
                    rot.y = 0;
                    stateMachine.Target.transform.rotation = Quaternion.LookRotation(rot);
                }
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.IsLocal)
            {
                stateMachine.Target.tps.SetVelocity(Vector3.zero);
                StartedInAir = stateMachine.Variables.Jumping;
                GameRoomNetworkController.SendPacketToServer(new UpdateFsmStatePacket(0, data), DeliveryMethod.ReliableOrdered);
            }

            stateMachine.Variables.AlternateDashAnimation = !stateMachine.Variables.AlternateDashAnimation;

            if (GameServer.IsServer)
            {
                GameRoomNetworkController.SendPacketToAll(new UpdateFsmStatePacket(stateMachine.Target.NetworkId, data), DeliveryMethod.ReliableOrdered);
            }
            else
            {
                if (stateMachine.Target.TryGetComponent(out AnimationsController animationsController))
                {
                    if (data != null && stateMachine.Variables != null)
                    {
                        var animData = animationsController.GetDashAnimation(data.dashIndex,
                            stateMachine.Variables.AlternateDashAnimation);

                        SetAnimation(stateMachine, animData?.value ?? 0);

                        if (animData.fx != null)
                        {
                            VfxController.Instance.SpawnFx(animData.fx, stateMachine.Target.transform.position,
                                stateMachine.Target.transform.rotation);
                        }
                    }
                };
                
            }

            if (SfxController.Instance != null)
            {
                var clip = SfxController.Instance.manager.GetRandomDashSfx();
                SfxController.Instance.PlaySound(clip.sfxId, 0.5f);
            }
        }

        private void SetAnimation(PlayerStateMachine stateMachine, int v)
        {
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetTrigger("dash");
                stateMachine.Target.animator.SetInteger("dash_id", v);
            }
        }

        protected override void OnExit(PlayerStateMachine machine)
        {
            if (machine.Target.animator != null)
            {
                machine.Target.animator?.SetBool("Grounded", machine.Variables.Grounded);
            }
            
            SetAnimation(machine, 0);
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
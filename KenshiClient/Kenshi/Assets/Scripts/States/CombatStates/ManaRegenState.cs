using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using UnityEngine;

namespace StarterAssets.CombatStates
{
    public class ManaRegenState : FSMState
    {
        public override FSMStateId Id => FSMStateId.mana_regen;

        private GameObject manaLoadEffect;

        private float value = 0;
        private float startValue;
        
        protected override void OnUpdate(PlayerStateMachine stateMachine)
        {
        }

        protected override void OnFixedUpdate(PlayerStateMachine stateMachine)
        {
            value += Time.deltaTime * 25;
            value = Mathf.Clamp(value, 0, 100);
            if (stateMachine.Target.GetStat(StatEventPacket.StatId.mana, out ushort v))
            {
                if (stateMachine.IsLocal)
                {
                    CombatController.Instance.SetPlayerStat(new StatEventPacket.Data()
                    {
                        value = (ushort)value,
                        maxValue = (ushort)100,
                        playerId = stateMachine.Target.NetworkId,
                        statId = StatEventPacket.StatId.mana
                    });
                }
            }
        }

        protected override void OnInputUpdate(PlayerStateMachine stateMachine)
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                stateMachine.ChangeState(new IdleState());
            }
        }

        protected override void OnEnter(PlayerStateMachine stateMachine)
        {
            stateMachine.Target.GetStat<ushort>(StatEventPacket.StatId.mana, out var startV);
            value = (float)startV;
            SyncStateOverNetwork(stateMachine);

            if (StatesController.Instance && StatesController.Instance.manaLoadEffect != null)
            {
                manaLoadEffect = GameObject.Instantiate(StatesController.Instance.manaLoadEffect,
                    stateMachine.Target.transform.position,
                    stateMachine.Target.transform.rotation);
                manaLoadEffect.transform.parent = stateMachine.Target.transform;
            }

            stateMachine.Target.movementStateMachine.ChangeState(new StandState());
            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("mana_regen", true);
            }
        }

        protected override void OnExit(PlayerStateMachine stateMachine)
        {
            if (StatesController.Instance && StatesController.Instance.manaLoadEffect != null)
            {
                GameObject.Destroy(manaLoadEffect);
            }

            if (stateMachine.Target.animator != null)
            {
                stateMachine.Target.animator.SetBool("mana_regen", false);
            }
        }
    }
}
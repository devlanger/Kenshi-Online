using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using Kenshi.Utils;
using StarterAssets.CombatStates;
using UnityEngine;
using UnityEngine.AI;

public class AggressiveBot : MonoBehaviour
{
    private Player _player;
    private Player _target;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(LookForTarget());
        StartCoroutine(ChasePlayer());
        StartCoroutine(AttackTarget());
        StartCoroutine(CastAbility());
    }

    private IEnumerator CastAbility()
    {
        while (true)
        {
            if (_target != null)
            {
                yield return new WaitForSeconds(0.2f);

                if (GetDistanceToTarget() > 7)
                {
                    _player.playerStateMachine.ChangeState(new AbilityCastState(new AbilityCastState.Data
                    {
                        abilityId = 23,
                        hitPoint = _target.transform.position + Vector3.up,
                        startPos = transform.position,
                    }));
                    yield return new WaitForSeconds(UnityEngine.Random.Range(3, 8));
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Update()
    {
        _player.playerStateMachine.CurrentState?.UpdateInput(_player.playerStateMachine);
    }

    private IEnumerator AttackTarget()
    {
        while (true)
        {
            Attack();

            yield return new WaitForSeconds(1);
        }
    }

    private void Attack()
    {
        if (_target != null)
        {
            if (GetDistanceToTarget() <= 3)
            {
                _player.Input.leftClick = true;
            }
            else
            {
                _player.Input.leftClick = false;
            }
        }
        else
        {
            _player.Input.leftClick = false;
        }
    }

    private IEnumerator ChasePlayer()
    {
        while (true)
        {
            if (_target != null && !_target.Input.leftClick)
            {
                if (GetDistanceToTarget() < 3)
                {
                    //_agent.SetDestination(transform.position);
                }
                else
                {
                    _agent.SetDestination(_target.transform.position);
                }
            }
            
            GameRoomNetworkController.SendPacketToAll(new PositionUpdatePacket(_player, _agent.speed));
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator LookForTarget()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            if (_target == null)
            {
                var p = PlayerUtils.GetPlayersAtPos(transform.position, 20, _player);
                if (p.Count > 0)
                {
                    _target = p[0];
                }
            }
            else
            {
                if (GetDistanceToTarget() > 20 || 
                    _target.playerStateMachine.CurrentState.Id == FSMStateId.dead)
                {
                    _target = null;
                }
            }
        }
    }

    private float GetDistanceToTarget() => Vector3.Distance(transform.position, _target.transform.position);
}

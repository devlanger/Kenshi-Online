using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;
using UnityEngine.AI;

public class Player : Mob
{
    public PlayerInterpolation Interpolation;
    public ThirdPersonController tps;
    
    public StarterAssetsInputs Input = new StarterAssetsInputs();
    public bool IsLocalPlayer { get; set; }
    public bool IsBot { get; set; }
    public int NetworkId { get; set; }
    public float Ping => peer != null ? (float)((float)peer.Ping / 1000f) : 0f;
    
    public PlayerStateMachine playerStateMachine;
    public PlayerStateMachine movementStateMachine;
    public Animator animator;
    public NavMeshAgent agent;

    public NetPeer peer;
    public Dictionary<StatEventPacket.StatId, object> stats = new Dictionary<StatEventPacket.StatId, object>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStateMachine = new PlayerStateMachine();
        playerStateMachine.Target = this;
        playerStateMachine.Variables = GetComponent<StateMachineVariables>();
        playerStateMachine.ChangeState(new IdleState());
        
        movementStateMachine = new PlayerStateMachine();
        movementStateMachine.Target = this;
        movementStateMachine.Variables = GetComponent<StateMachineVariables>();
        movementStateMachine.ChangeState(new StandState());

        stats[StatEventPacket.StatId.health] = (ushort)100;
        stats[StatEventPacket.StatId.mana] = (ushort)100;
        stats[StatEventPacket.StatId.experience] = (uint)0;
        stats[StatEventPacket.StatId.level] = (byte)1;
        stats[StatEventPacket.StatId.username] = "";
    }

    public void SetStat(StatEventPacket.StatId id, object value)
    {
        if (stats == null)
        {
            stats = new Dictionary<StatEventPacket.StatId, object>();
        }

        stats[id] = value;
    }
    
    public bool GetStat<T>(StatEventPacket.StatId id, out T result)
    {
        if (!stats.ContainsKey(id))
        {
            stats.Add(id, default(T));
        }
        
        if (stats[id] is T)
        {
            result = (T)stats[id];
            return true;
        }

        result = default(T);
        return false;
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            var state = UIInputController.Instance.CurrentState;
            if (state != UIInputController.State.WRITING_CHAT && 
                state != UIInputController.State.ESCAPE)
            {
                movementStateMachine.CurrentState?.UpdateInput(movementStateMachine);
                playerStateMachine.CurrentState?.UpdateInput(playerStateMachine);
            }
        }
        
        playerStateMachine.CurrentState?.FixedUpdate(playerStateMachine);
        playerStateMachine.CurrentState?.Update(playerStateMachine);
        
        movementStateMachine.CurrentState?.FixedUpdate(movementStateMachine);
        movementStateMachine.CurrentState?.Update(movementStateMachine);
        
        playerStateMachine?.UpdateQueue();
        movementStateMachine?.UpdateQueue();
        
        if (IsLocalPlayer)
        {
            Input.jump = false;
            Input.rightClick = false;
            Input.leftClick = false;
            Input.dashing = false;
            Input.dashIndex = DashState.Data.DashIndex.none;
            
            tps.CameraRotation();
        }
    }

    public void ActivateNavAgent(bool b)
    {
        if (TryGetComponent<NavMeshAgent>(out var c))
        {
            c.enabled = b; 
            c.isStopped = !b;
        }
    }
}

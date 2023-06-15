using System.Collections;
using System.Linq;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using Kenshi.Utils;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;
using UnityEngine.AI;

public class BotAbilityCastState : GenericFSMState<AggressiveBot.State>
{
    public readonly Player _target;
    public override FSMStateId Id { get; }
    public override AggressiveBot.State StateId => AggressiveBot.State.CAST_ABILITY_MELEE;

    public BotAbilityCastState(Player target)
    {
        _target = target;
    }

    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
        stateMachine.Target.playerStateMachine.ChangeState(new AbilityCastState(new AbilityCastState.Data
        {
            abilityId = 23,
            hitPoint = _target.transform.position + Vector3.up,
            startPos = stateMachine.Target.transform.position,
        }));
    }

    protected override void OnExit(PlayerStateMachine stateMachine)
    {
    }
}

public class BotAttackState : GenericFSMState<AggressiveBot.State>
{
    public readonly Player _target;
    public override FSMStateId Id { get; }
    public override AggressiveBot.State StateId => AggressiveBot.State.ATTACK;

    public BotAttackState(Player target)
    {
        _target = target;
    }
    
    protected override void OnUpdate(PlayerStateMachine stateMachine)
    {
        if (_target == null)
        {
            return;
        }
        
        stateMachine.Target.Input.leftClick = true;

        Vector3 dir = _target.transform.position - stateMachine.Target.transform.position;
        dir.y = 0;
        
        stateMachine.Target.Input.CameraForward = dir;
    }

    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
    }

    protected override void OnExit(PlayerStateMachine stateMachine)
    {
        stateMachine.Target.Input.leftClick = false;
    }
}

public class BotChaseState : GenericFSMState<AggressiveBot.State>
{
    public override AggressiveBot.State StateId => AggressiveBot.State.CHASE;
    public override FSMStateId Id { get; }
    public Player _target;
    public NavMeshAgent _agent;

    public BotChaseState(Player target)
    {
        _target = target;
    }

    protected override void OnUpdate(PlayerStateMachine stateMachine)
    {
        _agent.SetDestination(_target.transform.position);
        var dir = GetDirToTarget();

        _agent.transform.rotation = Quaternion.LookRotation(dir);
        stateMachine.Target.Input.CameraForward = dir;
    }

    private Vector3 GetDirToTarget()
    {
        Vector3 dir = _target.transform.position - _agent.transform.position;
        dir.y = 0;
        return dir;
    }
    
    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
        _agent = stateMachine.Target.GetComponent<NavMeshAgent>();
    }

    protected override void OnExit(PlayerStateMachine stateMachine)
    {
        _agent.SetDestination(_agent.transform.position);
    }
}

public class BotIdleState : GenericFSMState<AggressiveBot.State>
{
    public override FSMStateId Id { get; }
    public override AggressiveBot.State StateId => AggressiveBot.State.IDLE;

    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
    }

    protected override void OnExit(PlayerStateMachine stateMachine)
    {
    }

}

public class BotRoamState : GenericFSMState<AggressiveBot.State>
{
    public override FSMStateId Id { get; }

    private Vector3 destination;
    public NavMeshAgent _agent;

    public override AggressiveBot.State StateId => AggressiveBot.State.ROAM;

    public Player GetTarget(PlayerStateMachine stateMachine)
    {
        var p = PlayerUtils.GetPlayersAtPos(stateMachine.Target.transform.position, 20, stateMachine.Target).Where(p => p.playerStateMachine.CurrentState.Id != FSMStateId.dead).ToList();
        if (p.Count > 0)
        {
            return p[0];
        }

        return null;
    }
    
    private Vector3 GetDirToTarget()
    {
        Vector3 dir = destination - _agent.transform.position;
        dir.y = 0;
        return dir;
    }
    
    protected override void OnUpdate(PlayerStateMachine stateMachine)
    {
        _agent.SetDestination(destination);
        var dir = GetDirToTarget();
        _agent.transform.rotation = Quaternion.LookRotation(dir);
        stateMachine.Target.Input.CameraForward = dir;
    }

    protected override void OnInputUpdate(PlayerStateMachine stateMachine)
    {
    }

    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
        _agent = stateMachine.Target.GetComponent<NavMeshAgent>();
        
        destination = stateMachine.Target.transform.position + new Vector3(UnityEngine.Random.Range(-25, 25), stateMachine.Target.transform.position.y,
            UnityEngine.Random.Range(-25, 25));
    }

    protected override void OnExit(PlayerStateMachine stateMachine)
    {
    }
}

public class AggressiveBot : MonoBehaviour
{
    private Player _player;
    private Player _target;
    private NavMeshAgent _agent;

    public State state;

    public PlayerStateMachine stateMachine;
    private float _rangedAbilityCastTime;
    
    public enum State
    {
        ROAM = 1,
        CHASE = 2,
        ATTACK = 3,
        CAST_ABILITY_MELEE = 4,
        FORFEIT = 5,
        LOAD_MANA = 6,
        CAST_ABILITY_RANGED = 7,
        JUMP = 8,
        DASH_FORWARD = 9,
        DASH_RIGHT = 10,
        DASH_BACK = 11,
        IDLE = 12
    }

    private void Start()
    {
        _player.stats[StatEventPacket.StatId.username] = $"Bot-{UnityEngine.Random.Range(1, 9999)}";
    }

    private void OnStateEnter(FSMState state)
    {
        switch (state)
        {
            case BotChaseState chaseState:
                _rangedAbilityCastTime = UnityEngine.Random.Range(2, 7);
                break;
        }
    }
    
    public void UpdateStateManagement()
    {
        var currentState = stateMachine.CurrentState;

        switch (_player.playerStateMachine.CurrentState.Id)
        {
            case FSMStateId.stunned:
            case FSMStateId.dead:
            case FSMStateId.hit:
                if (!(currentState is BotIdleState))
                {
                    stateMachine.ChangeState(new BotIdleState());
                }
                return;
        }
        
        switch (currentState)
        {
            case BotIdleState idleState:
                stateMachine.ChangeState(new BotRoamState());
                break;
            case BotAttackState attackState:
                UpdateAttackState(attackState);
                break;
            case BotRoamState roamState:
                UpdateRoamState(roamState);
                break;
            case BotChaseState chaseState:
                UpdateChaseState(chaseState);
                break;
            case BotAbilityCastState abilityState:
                UpdateAbilityState(abilityState);
                break;
        }
    }

    private void UpdateAbilityState(BotAbilityCastState abilityState)
    {
        if (abilityState.ElapsedTime > 2f)
        {
            stateMachine.ChangeState(
                abilityState._target == null || abilityState._target.playerStateMachine.CurrentState.Id == FSMStateId.dead
                    ? new BotRoamState()
                    : new BotChaseState(abilityState._target));
        }
    }

    private void UpdateChaseState(BotChaseState chaseState)
    {
        if (chaseState._target == null || chaseState._target.playerStateMachine.CurrentState.Id == FSMStateId.dead)
        {
            stateMachine.ChangeState(new BotRoamState());
            return;
        }

        if (chaseState.ElapsedTime > _rangedAbilityCastTime)
        {
            stateMachine.ChangeState(new BotAbilityCastState(chaseState._target));
            return;
        }

        if (GetDistanceToTarget(chaseState._target) <= 2f)
        {
            stateMachine.ChangeState(new BotAttackState(chaseState._target));
            return;
        }
        
        if (GetDistanceToTarget(chaseState._target) > 25f)
        {
            stateMachine.ChangeState(new BotRoamState());
        }
    }

    private void UpdateAttackState(BotAttackState state)
    {
        if (state._target == null || state._target.playerStateMachine.CurrentState.Id == FSMStateId.dead)
        {
            stateMachine.ChangeState(new BotRoamState());
            return;
        }

        if (GetDistanceToTarget(state._target) > 2.5f)
        {
            stateMachine.ChangeState(new BotChaseState(state._target));
        }
    }

    private void UpdateRoamState(BotRoamState roamState)
    {
        if (roamState.ElapsedTime > 10)
        {
            stateMachine.ChangeState(new BotRoamState());
            return;
        }

        var t = roamState.GetTarget(stateMachine);
        if (t != null)
        {
            stateMachine.ChangeState(new BotChaseState(t));
        }
    }

    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
        stateMachine.Target = GetComponent<Player>();
        stateMachine.ChangeState(new BotRoamState());
        stateMachine.OnStateEnter += OnStateEnter;
        
        _player = GetComponent<Player>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        StartCoroutine(SendPosition());
    }

    private IEnumerator SendPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            GameRoomNetworkController.SendPacketToAll(new PositionUpdatePacket(_player, _agent.speed));
        }
    }

    private float GetDistanceToTarget(Player _target) =>
        _target == null ? 999f : Vector3.Distance(stateMachine.Target.transform.position, _target.transform.position);

    private void Update()
    {
        UpdateStateManagement();

        _player._playerStateManagement?.UpdateInputStateManagement(_player.playerStateMachine);
        _player._movementStateManagement?.UpdateInputStateManagement(_player.movementStateMachine);
        
        stateMachine.CurrentState?.Update(stateMachine);
        _player.playerStateMachine.CurrentState?.UpdateInput(_player.playerStateMachine);
        _player.movementStateMachine.CurrentState?.UpdateInput(_player.movementStateMachine);
    }
}
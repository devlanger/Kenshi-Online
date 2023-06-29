using System;
using System.Collections;
using System.Collections.Generic;
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
    private int _skillId;
    public override FSMStateId Id { get; }
    public override AggressiveBot.State StateId => AggressiveBot.State.CAST_ABILITY_MELEE;

    public BotAbilityCastState(Player target, int skillId)
    {
        _skillId = skillId;
        _target = target;
    }

    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
        stateMachine.Target.playerStateMachine.ChangeState(new AbilityCastState(new AbilityCastState.Data
        {
            abilityId = _skillId,
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
    public Vector3 dir;

    public BotAttackState(Player target)
    {
        _target = target;
    }
    
    protected override void OnUpdate(PlayerStateMachine stateMachine)
    {
        dir = _target.transform.position - stateMachine.Target.transform.position;
        dir.y = 0;
        stateMachine.Target.Input.CameraForward = dir;
    }

    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
        stateMachine.Target.Input.leftClick = true;
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
    private NavMeshPath path;
    private AggressiveBot _bot;

    public BotChaseState(Player target, AggressiveBot bot)
    {
        path = new NavMeshPath();
        _target = target;
        _bot = bot;
    }

    public static bool MoveBot(Player target, Vector3 destination, AggressiveBot bot)
    {
        float distance = PlayerUtils.FlatDistance(target.transform.position, bot.Destination);
        if (distance < 1)
        {
            target.Input.move = new Vector2(0, 0);
            return false;
        }

        Vector3 dir = destination - target.transform.position;
        dir.y = 0;
        dir.Normalize();

        if (distance > 2)
        {
            if (Physics.SphereCast(bot.transform.position, 1.2f, dir, out var hit, 2, bot.collisionMask))
            {
                dir = Quaternion.LookRotation(Vector3.right) * dir;
            }
        }

        if (target.playerStateMachine.CurrentState.Id != FSMStateId.attack)
        {
            target.Input.CameraForward = dir;
        }
        target.Input.move = new Vector2(0, 1);
        return true;
    }
    
    protected override void OnUpdate(PlayerStateMachine stateMachine)
    {
        stateMachine.Target.Input.sprint = true;

        _bot.SetDestination(_target.transform.position);

        MoveBot(stateMachine.Target, _bot.Destination, _bot);
    }
    
    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
    }

    protected override void OnExit(PlayerStateMachine stateMachine)
    {
        stateMachine.Target.Input.move = Vector3.zero;
        _target.Input.sprint = false;
    }
}

public static class NavAgentUtils
{
    public static void SetClosestDestination(this NavMeshAgent agent, Vector3 pos)
    {
        agent.SetDestination(pos);

        // if (NavMesh.SamplePosition(pos, out var hit, 50, NavMesh.AllAreas))
        // {
        //     agent.SetDestination(hit.position);
        // }
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
    public NavMeshPath path;
    private AggressiveBot _bot;

    public override AggressiveBot.State StateId => AggressiveBot.State.ROAM;

    public BotRoamState(AggressiveBot bot)
    {
        _bot = bot;
    }
    
    public Player GetTarget(PlayerStateMachine stateMachine)
    {
        var p = PlayerUtils.GetPlayersAtPos(stateMachine.Target.transform.position, 20, stateMachine.Target).Where(p => p.playerStateMachine.CurrentState.Id != FSMStateId.dead).ToList();
        if (p.Count > 0)
        {
            return p[0];
        }

        return null;
    }
    
    protected override void OnUpdate(PlayerStateMachine stateMachine)
    {
        if (PlayerUtils.FlatDistance(stateMachine.Target.transform.position, destination) < 1)
        {
            stateMachine.Target.Input.move = Vector2.zero;
            return;
        }
        
        stateMachine.Target.Input.sprint = true;

        BotChaseState.MoveBot(stateMachine.Target, destination, _bot);
    }

    protected override void OnInputUpdate(PlayerStateMachine stateMachine)
    {
    }

    protected override void OnEnter(PlayerStateMachine stateMachine)
    {
        path = new NavMeshPath();
        stateMachine.Target.Input.sprint = true;

        if (Physics.Raycast(stateMachine.Target.transform.position, -Vector3.up, out var hit))
        {
            destination = stateMachine.Target.transform.position + new Vector3(UnityEngine.Random.Range(-25, 25), hit.point.y,
                UnityEngine.Random.Range(-25, 25));
        }
        else
        {
            destination = stateMachine.Target.transform.position + new Vector3(UnityEngine.Random.Range(-25, 25), stateMachine.Target.transform.position.y,
                UnityEngine.Random.Range(-25, 25));
        }
        
        //NavMeshHit myNavHit;
        // if(NavMesh.SamplePosition(destination, out myNavHit, 100 , NavMesh.AllAreas))
        // {
        //     destination = myNavHit.position;
        // }
    }

    protected override void OnExit(PlayerStateMachine stateMachine)
    {
        stateMachine.Target.Input.sprint = false;
        stateMachine.Target.Input.move = Vector3.zero;
    }
}

public static class BotUtils
{
}

public class AggressiveBot : MonoBehaviour
{
    private Player _player;
    private Player _target;
    private NavMeshAgent _agent;
    public LayerMask collisionMask;

    public State state;

    public PlayerStateMachine stateMachine;
    private float _rangedAbilityCastTime;
    [SerializeField] private List<int> skillIds;

    [SerializeField] private CustomizationManager _customizationManager;
    
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
        _player.GetComponent<PlayerCustomization>()?.Randomize();
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
        if(currentState is GenericFSMState<AggressiveBot.State> s)
        {
            state = s.StateId;
        }
        
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
                stateMachine.ChangeState(new BotRoamState(this));
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
                    ? new BotRoamState(this)
                    : new BotChaseState(abilityState._target, this));
        }
    }

    private void UpdateChaseState(BotChaseState chaseState)
    {
        if (chaseState._target == null || chaseState._target.playerStateMachine.CurrentState.Id == FSMStateId.dead)
        {
            stateMachine.ChangeState(new BotRoamState(this));
            return;
        }

        if (chaseState.ElapsedTime > _rangedAbilityCastTime)
        {
            stateMachine.ChangeState(new BotAbilityCastState(chaseState._target, GetRandomSkillId()));
            return;
        }

        if (GetDistanceToTarget(chaseState._target) <= 2.6f)
        {
            stateMachine.ChangeState(new BotAttackState(chaseState._target));
            return;
        }
        
        if (GetDistanceToTarget(chaseState._target) > 25f)
        {
            stateMachine.ChangeState(new BotRoamState(this));
        }
    }

    private int GetRandomSkillId()
    {
        return skillIds[UnityEngine.Random.Range(0, skillIds.Count - 1)];
    }

    private void UpdateAttackState(BotAttackState state)
    {
        if (state._target == null || state._target.playerStateMachine.CurrentState.Id == FSMStateId.dead)
        {
            stateMachine.ChangeState(new BotRoamState(this));
            return;
        }

        if (GetDistanceToTarget(state._target) > 2.6f)
        {
            stateMachine.ChangeState(new BotChaseState(state._target, this));
        }
    }

    private void UpdateRoamState(BotRoamState roamState)
    {
        if (roamState.ElapsedTime > 10)
        {
            stateMachine.ChangeState(new BotRoamState(this));
            return;
        }

        var t = roamState.GetTarget(stateMachine);
        if (t != null)
        {
            stateMachine.ChangeState(new BotChaseState(t, this));
        }
    }

    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
        stateMachine.Target = GetComponent<Player>();
        stateMachine.ChangeState(new BotRoamState(this));
        stateMachine.OnStateEnter += OnStateEnter;
        
        _player = GetComponent<Player>();
        _agent = GetComponent<NavMeshAgent>();
        if(_agent)
            _agent.updateRotation = false;

        StartCoroutine(SendPosition());
    }

    private IEnumerator SendPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            GameRoomNetworkController.SendPacketToAll(new PositionUpdatePacket(_player, _player.playerStateMachine.GetSpeed(_player.playerStateMachine)));
        }
    }

    private float GetDistanceToTarget(Player _target) =>
        _target == null ? 999f : PlayerUtils.FlatDistance(stateMachine.Target.transform.position, _target.transform.position);

    private void Update()
    {
        UpdateStateManagement();

        stateMachine.CurrentState?.UpdateInput(stateMachine);
        stateMachine.CurrentState?.Update(stateMachine);
        
        _player._playerStateManagement?.UpdateInputStateManagement(_player.playerStateMachine);
        _player._movementStateManagement?.UpdateInputStateManagement(_player.movementStateMachine);
        
        _player.playerStateMachine.CurrentState?.UpdateInput(_player.playerStateMachine);
        _player.movementStateMachine.CurrentState?.UpdateInput(_player.movementStateMachine);
    }

    public void SetDestination(Vector3 destination)
    {
        Destination = destination;
    }

    public Vector3 Destination { get; private set; }
}
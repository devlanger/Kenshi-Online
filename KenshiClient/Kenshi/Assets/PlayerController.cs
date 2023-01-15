using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = UnityEngine.Windows.Input;

public enum PlayerState
{
    IDLE = 0,
    ABILITY_CAST = 1,
    ATTACK = 2,
}

public enum MovementState
{
    STAND = 1,
    MOVING = 2,
    JUMP = 3,
    DASH_LEFT = 4,
    DASH_RIGHT = 5,
    DASH_BACK = 6,
    DASH_FORWARD = 7,
}

[System.Serializable]
public class DashState
{
    public KeyCode lastKey;
    public float keyTime;

    public Dictionary<KeyCode, MovementState> keys = new Dictionary<KeyCode, MovementState>()
    {
        {KeyCode.A, MovementState.DASH_LEFT}, {KeyCode.W, MovementState.DASH_FORWARD}, { KeyCode.S, MovementState.DASH_BACK }, {KeyCode.D, MovementState.DASH_RIGHT}
    };
    
    public bool IsDashing => keys.Keys.Contains(lastKey) && Time.time < keyTime + 0.2f;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private LayerMask _aimLayerMask;

    public Player localPlayer;
    public Camera camera;
    public ThirdPersonController tpsController;
    public TargetController _targetController;
    public AbilitiesController _abilitiesController;
    private PlayerStateMachine<PlayerState> playerState;
    private PlayerStateMachine<MovementState> movementState;

    private DashState dashState = new DashState();
    
    private Animator animator;
    private int attackIndex = 0;
    private float lastAttackTime = 0;

    private void Awake()
    {
        playerState = new PlayerStateMachine<PlayerState>(PlayerState.IDLE);
        movementState = new PlayerStateMachine<MovementState>(MovementState.STAND);
        playerState.OnStateEnter += PlayerStateOnOnStateEnter;
        playerState.OnStateExit += PlayerStateOnOnStateExit;
        
        movementState.OnStateEnter += MovementStateOnOnStateEnter;
        movementState.OnStateExit += MovementStateOnOnStateExit;

        animator = localPlayer.GetComponent<Animator>();

        localPlayer.gameObject.layer = 10;
    }

    private void Update()
    {        
        UpdatePlayerState();
        UpdateMovementState();
        
        if (attackIndex > 0 && Time.time > lastAttackTime + 1)
        {
            attackIndex = 0;
        }
        
        _input.rightClick = false;
        _input.leftClick = false;
        _input.dashing = false;
    }

    private void MovementStateOnOnStateExit(MovementState obj)
    {
        switch (movementState.state)
        {
            case MovementState.STAND:
                break;
            case MovementState.MOVING:
                break;
            case MovementState.DASH_BACK:

                break;
            case MovementState.DASH_FORWARD:

                break;
            case MovementState.DASH_LEFT:

                break;
            case MovementState.DASH_RIGHT:

                break;
        }
    }

    private void MovementStateOnOnStateEnter(MovementState obj)
    {
        switch (movementState.state)
        {
            case MovementState.STAND:
                break;
            case MovementState.MOVING:
                break;
            case MovementState.DASH_BACK:

                break;
            case MovementState.DASH_FORWARD:

                break;
            case MovementState.DASH_LEFT:

                break;
            case MovementState.DASH_RIGHT:

                break;
        }
    }
    
    private void UpdateMovementState()
    {
        switch (movementState.state)
        {
            case MovementState.STAND:
                UpdateDashState();
                tpsController.UpdateGravity();
                tpsController.UpdateMovement();
                break;
            case MovementState.MOVING:
                tpsController.UpdateGravity();
                tpsController.UpdateMovement();
                break;
            case MovementState.DASH_BACK:
                if (dashFinished(movementState.ElapsedTime))
                {
                    movementState.ChangeState(MovementState.STAND);
                }
                localPlayer.transform.position -= camera.transform.forward * 10 * Time.deltaTime;
                break;
            case MovementState.DASH_FORWARD:
                if (dashFinished(movementState.ElapsedTime))
                {
                    movementState.ChangeState(MovementState.STAND);
                }
                localPlayer.transform.position += camera.transform.forward * 10 * Time.deltaTime;
                break;
            case MovementState.DASH_LEFT:
                if (dashFinished(movementState.ElapsedTime))
                {
                    movementState.ChangeState(MovementState.STAND);
                }
                localPlayer.transform.position -= camera.transform.right * 10 * Time.deltaTime;
                break;
            case MovementState.DASH_RIGHT:
                if (dashFinished(movementState.ElapsedTime))
                {
                    movementState.ChangeState(MovementState.STAND);
                }
                localPlayer.transform.position += camera.transform.right * 10 * Time.deltaTime;
                break;
        }
    }

    private bool dashFinished(float elapsedTime) => elapsedTime > 0.4f;
    
    private void UpdateDashState()
    {
        foreach (var key in dashState.keys)
        {
            if (UnityEngine.Input.GetKeyDown(key.Key))
            {
                if (dashState.IsDashing)
                {
                    dashState.lastKey = KeyCode.None;
                    _input.dashing = true;
                    movementState.ChangeState(key.Value);
                }
                
                dashState.lastKey = key.Key;
                dashState.keyTime = Time.time;
            }
        }
    }

    private void PlayerStateOnOnStateExit(PlayerState obj)
    {
        switch (obj)
        {
            case PlayerState.IDLE:

                break;
            case PlayerState.ABILITY_CAST:
                animator.SetTrigger("ability");
                animator.SetInteger("ability_id", 0);
                break;
            case PlayerState.ATTACK:
                animator.SetTrigger("attack");
                animator.SetInteger("attack_id", 0);
                lastAttackTime = Time.time;
                _playerInput.enabled = true;
                break;
        }
    }

    private void PlayerStateOnOnStateEnter(PlayerState obj)
    {
        switch (obj)
        {
            case PlayerState.IDLE:
                break;
            case PlayerState.ATTACK:
                attackIndex++;
                animator.SetTrigger("attack");
                animator.SetInteger("attack_id", attackIndex);
                if (attackIndex == 4)
                {
                    attackIndex = 0;
                }
                break;
            case PlayerState.ABILITY_CAST:
                Physics.Raycast(camera.ScreenPointToRay(UnityEngine.Input.mousePosition), out RaycastHit hit, 100, _aimLayerMask);
                animator.SetTrigger("ability");
                animator.SetInteger("ability_id", 1);
                _abilitiesController.CastAbility(new AbilityInfo()
                {   
                    user = localPlayer,
                    abilityId = 1,
                    aimPoint = hit.collider == null ? camera.transform.forward : hit.point
                });
                break;
        }
    }

    private void UpdatePlayerState()
    {
        switch (playerState.state)
        {
            case PlayerState.IDLE:
                UpdateAttackInput();
                if (_input.rightClick)
                {
                    playerState.ChangeState(PlayerState.ABILITY_CAST);
                }
                break;
            case PlayerState.ABILITY_CAST:
                if (playerState.ElapsedTime > 0.3f)
                {
                    playerState.ChangeState(PlayerState.IDLE);
                }
                break;
            case PlayerState.ATTACK:
                Vector3 forward = camera.transform.forward;
                forward.y = 0;
                localPlayer.transform.rotation = Quaternion.LookRotation(forward); 
                localPlayer.transform.position += localPlayer.transform.forward * Time.deltaTime;

                bool lastAttack = attackIndex == 0;
                if (playerState.ElapsedTime > (lastAttack ? 0.6f : 0.3f))
                {
                    if (!UpdateAttackInput())
                    {
                        if (playerState.ElapsedTime > (lastAttack ? 1 : 0.6f))
                        {
                            playerState.ChangeState(PlayerState.IDLE);
                        }  
                    }
                }
                break;
        }
    }

    private bool UpdateAttackInput()
    {
        if (_input.leftClick)
        {
            playerState.ChangeState(PlayerState.ATTACK);
            return true;
        }

        return false;
    }
}

public class PlayerStateMachine<T> where T : IComparable
{
    public T state;
    private float enterTime;

    public float ElapsedTime => Time.time - enterTime;

    public event Action<T> OnStateExit; 
    public event Action<T> OnStateEnter;

    public PlayerStateMachine(T startState)
    {
        state = startState;
    }
    
    public void ChangeState(T state)
    {
        enterTime = Time.time;
        OnStateExit?.Invoke(this.state);
        
        this.state = state;

        OnStateEnter?.Invoke(state);
    }
}
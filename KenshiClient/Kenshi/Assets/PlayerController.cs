using System;
using System.Collections;
using System.Collections.Generic;
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
    DASH_BACK = 5,
    DASH_FORWARD = 5,
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

    private Animator animator;
    private int attackIndex = 0;
    private float lastAttackTime = 0;

    private void Awake()
    {
        playerState = new PlayerStateMachine<PlayerState>();
        movementState = new PlayerStateMachine<MovementState>();
        playerState.OnStateEnter += PlayerStateOnOnStateEnter;
        playerState.OnStateExit += PlayerStateOnOnStateExit;

        animator = localPlayer.GetComponent<Animator>();

        localPlayer.gameObject.layer = 10;
    }

    private void Update()
    {        
        UpdatePlayerState();

        if (attackIndex > 0 && Time.time > lastAttackTime + 1)
        {
            attackIndex = 0;
        }
        
        _input.rightClick = false;
        _input.leftClick = false;
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
        switch (playerState.playerState)
        {
            case PlayerState.IDLE:
                UpdateAttackInput();
                if (_input.rightClick)
                {
                    playerState.ChangePlayerState(PlayerState.ABILITY_CAST);
                }
                tpsController.UpdateGravity();
                tpsController.UpdateMovement();
                break;
            case PlayerState.ABILITY_CAST:
                if (playerState.ElapsedTime > 0.3f)
                {
                    playerState.ChangePlayerState(PlayerState.IDLE);
                }
                
                tpsController.UpdateGravity();
                tpsController.UpdateMovement();
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
                            playerState.ChangePlayerState(PlayerState.IDLE);
                        }  
                    }
                }
                
                tpsController.UpdateGravity();
                //tpsController.UpdateMovement();
                break;
        }
    }

    private bool UpdateAttackInput()
    {
        if (_input.leftClick)
        {
            playerState.ChangePlayerState(PlayerState.ATTACK);
            return true;
        }

        return false;
    }
}

public class PlayerStateMachine<T> where T : IComparable
{
    public T playerState;
    public MovementState MovementState;
    private float enterTime;

    public float ElapsedTime => Time.time - enterTime;

    public event Action<T> OnStateExit; 
    public event Action<T> OnStateEnter;
    
    public void ChangePlayerState(T state)
    {
        enterTime = Time.time;
        OnStateExit?.Invoke(playerState);
        
        this.playerState = state;

        OnStateEnter?.Invoke(state);
    }
}
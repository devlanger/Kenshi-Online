using System;
using Kenshi.Shared.Enums;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        public float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private Rigidbody _controller;
        private GameObject _mainCamera;

        private Player target;
        
        private StarterAssetsInputs _input => target.Input;

        private const float _threshold = 0.01f;

        private int jumpIndex = 0;

        private bool _hasAnimator;
        [SerializeField] private float cameraSpeedY = 3;
        [SerializeField] private float cameraSpeedX = 3;

        private float _speedBlendTarget;
        
//         private bool IsCurrentDeviceMouse
//         {
//             get
//             {
// #if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
//                 return _playerInput.currentControlScheme == "KeyboardMouse";
// #else
// 				return false;
// #endif
//             }
//         }


        private void Awake()
        {
            SetPlayer(GetComponent<Player>());
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        public void SetPlayer(Player player)
        {
            target = player;
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = player.TryGetComponent(out _animator);
            _controller = player.GetComponent<Rigidbody>();

            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        public bool GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(target.transform.position.x, target.transform.position.y - GroundedOffset,
                target.transform.position.z);
            
            var grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            return grounded;
        }

        private void Update()
        {
            _animationBlend = Mathf.Lerp(_animationBlend, _speed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;
            
            _animator?.SetFloat(_animIDSpeed, _animationBlend);
            _animator?.SetFloat(_animIDMotionSpeed, GetInputMagnitude());
            
            Grounded = GroundedCheck();
            target.playerStateMachine.Variables.Grounded = Grounded;
            target.playerStateMachine.Variables.Jumping = Time.time > _jumpTime + 0.15f;
        }

        public void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = 1f;

                _cinemachineTargetYaw += _input.look.x * cameraSpeedY * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * cameraSpeedX * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        public void UpdateMovement(Vector3 velocity)
        {
            UpdateRotation();
            _controller.velocity = velocity;
        }

        public Vector3 GetVelocity()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            var inputMagnitude = GetInputMagnitude();
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            var velocity = (targetDirection.normalized * _speed) +
                       new Vector3(0.0f, _verticalVelocity, 0.0f);
            return velocity;
        }

        private float GetInputMagnitude()
        {
            return _input.analogMovement ? _input.move.magnitude : 1f;
        }

        private void UpdateRotation()
        {
            Vector3 inputDirection = _input.InputDirection;
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(target.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                target.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        public double _jumpTime = 0;

        public void UpdateGravity()
        {
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        public void StopMoving()
        {
            if (_controller != null)
            {
                Vector3 vel = Vector3.zero;
                vel.y = _controller.velocity.y;
                _controller.velocity = vel;
            }
            _speed = 0;
        }

        public void SetSpeed(float speed)
        {
            _speed = speed;
        }

        public void SetVerticalVelocity()
        {
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
        
        public void SetVelocity(Vector3 velocity, bool ignoreY = false)
        {
            Vector3 v = velocity;
            if (ignoreY)
            {
                _controller.velocity = new Vector3(v.x, _verticalVelocity, v.z);
            }
            else
            {
                _controller.velocity = velocity;
            }
        }
    }
}
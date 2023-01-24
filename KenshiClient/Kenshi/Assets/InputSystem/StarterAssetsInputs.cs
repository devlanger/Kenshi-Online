using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[System.Serializable]
	public class StarterAssetsInputs
	{
		public bool rightClick;
		public bool leftClick;

		
		public Vector3 InputDirection => new Vector3(move.x, 0.0f, move.y).normalized;
		public float RotationY;

		public Quaternion LocalRotation => Quaternion.Euler(0.0f, RotationY, 0.0f);
		public AbilityInfo abilityInfo;

		public Vector3 CameraForward;
		[FormerlySerializedAs("AimDirection")] public Vector3 HitPoint;

		public Vector3 LocalDirection;

		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool tab;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		public bool dashing = false;
		
		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}
		
		public void LeftClickInput(bool leftClick)
		{
			this.leftClick = leftClick;
		}
		
		public void RightClickInput(bool rightClick)
		{
			this.rightClick = rightClick;
		}
		
		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
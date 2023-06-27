using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

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
		public DashState.Data.DashIndex dashIndex;

		public AbilityInfo abilityInfo;

		public Vector3 CameraForward;
		public Vector3 HitPoint;

		public Vector3 LocalDirection;

		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool tab;

		public bool IsSprinting()
		{
			var sprinting = sprint;
			var sideOrForward = move.x != 0 && move.y >= 0 || move.y > 0;
			
			sprinting = sprinting && sideOrForward;

			return sprinting;
		}
		
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
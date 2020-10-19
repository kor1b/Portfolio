using UnityEngine;

namespace Bimicore.BSG.ThirdPerson
{
	public abstract class MovementOnGroundAbstractAbility : MovementAbstractAbility
	{
		[Header ("Movement on ground")]
		[SerializeField] protected float maxSpeed = 10f;
		[SerializeField] protected float acceleration = 30f;

		protected override void CommonEnter()
		{
			base.CommonEnter();

			ResetVelocityOnSwitchInputDirection();
			
			ClampSpeedToMaxAllowed();

			inputSystem.movementInputHandleEventHandler += RestartMovement;
		}

		protected override void CommonExit()
		{
			base.CommonExit();
			inputSystem.movementInputHandleEventHandler -= RestartMovement;
		}
		
		protected void ClampSpeedToMaxAllowed()
		{
			if (physicalProperties.targetVelocity.magnitude > maxSpeed)
			{
				SetTargetVelocity (maxSpeed);
			}
		}

		/// <summary>
		/// Handle situations when player changes his direction while running
		/// (press A button and release D (or vice versa) button in the one frame).
		/// </summary>
		protected async void RestartMovement()
		{
			ResetVelocityOnSwitchInputDirection();

			thirdPersonRotation.TryAlignHorizontalLookRotationWith (inputSystem.InputHorizontalMovementDirection);

			CalculateTargetVelocityDirection (inputSystem.InputHorizontalMovementDirection);

			if (!IsIncreasingVelocity)
				await IncreaseTargetVelocity (acceleration, maxSpeed);
		}
	}
}
using System;
using Bimicore.BSG.StateMachinePattern;
using NaughtyAttributes;
using UnityEngine;

namespace Bimicore.BSG.ThirdPerson
{
	public class JumpAbility : MovementInAirAbstractAbility, IState
	{
		public int StateRestrictorsCount { get; set; } = 0;

		private const float LOW_JUMP_FALL_ACCELERATION_MULTP = 2;
		
		[Header ("Jump")] 
		
		[SerializeField] private float jumpHeight = 3f;
		[ValidateInput ("IsNotGreaterThanJumpHeight", "Carefully, minJumpHeight should be less than jumpHeight.")]
		[SerializeField] private float minJumpHeight = 0.5f;

		[Space]
		
		[Tooltip ("Acceleration that pushes player upward")]
		[SerializeField] private float jumpUpAcceleration = 2.6f;
		[SerializeField] private bool enableCustomLowJumpFallAcceleration = false; // Used only to expand Inspector
		[EnableIf ("enableCustomLowJumpFallAcceleration")]
		[SerializeField] private float lowJumpFallAcceleration;

		private float _jumpStartHeight;
		private bool _jumpButtonReleasedInAir = false;

		#region Inspector

		private bool IsNotGreaterThanJumpHeight (float value) => value <= jumpHeight;

		#endregion

		protected override void Awake()
		{
			base.Awake();

			TrySetDefaultLowJumpFallAcceleration();
		}

		private void TrySetDefaultLowJumpFallAcceleration()
		{
			if (!enableCustomLowJumpFallAcceleration)
				lowJumpFallAcceleration = physicalProperties.fallAcceleration * LOW_JUMP_FALL_ACCELERATION_MULTP;
		}

		public async void OnEnter()
		{
			CommonEnter();

			view.StartAnimation (animatorManager);

			thirdPersonRotation.TryAlignHorizontalLookRotationWith (inputSystem.InputHorizontalMovementDirection);
			
			CalculateTargetVelocityDirection (inputSystem.InputHorizontalMovementDirection);

			Jump();

			if (inputSystem.InputHorizontalMovementDirection != 0)
			{
				await IncreaseTargetVelocity (physicalProperties.accelerationInAir, maxHorizontalSpeed);
			}
		}

		public void Tick()
		{
			CommonTick();
		}

		public void FixedTick()
		{
			AddGravityToJumpUp();

			CommonFixedTick();
		}

		public void OnExit()
		{
			CommonExit();

			//Reset for a new jump
			_jumpButtonReleasedInAir = false;

			//Restricts Bunny Hop possibility
			inputSystem.JumpButtonHold = false;
		}

		/// <summary>
		/// Add vertical velocity to character 
		/// </summary>
		private void Jump()
		{
			_jumpStartHeight = physicalProperties.Rb.position.y;
			physicalProperties.targetVelocity.y = 0f;

			float jumpUpSpeed = CalculateJumpSpeed (jumpHeight, jumpUpAcceleration);

			physicalProperties.targetVelocity += Vector3.up * jumpUpSpeed;
		}

		private float CalculateJumpSpeed (float height, float upwardAcceleration)
		{
			//formula sqrt(2 * jumpHeight * upward_acceleration)
			return Mathf.Sqrt (2 * height * upwardAcceleration);
		}
		
		private void AddGravityToJumpUp()
		{
			float downwardAcceleration = GetDownwardAcceleration();
			AddGravity (downwardAcceleration);
		}
		
		private float GetDownwardAcceleration()
		{
			if (_jumpButtonReleasedInAir && IsMinJumpHeightReached())
			{
				return lowJumpFallAcceleration;
			}

			return physicalProperties.fallAcceleration;
		}

		public bool IsMinJumpHeightReached() => physicalProperties.Rb.position.y >= _jumpStartHeight + minJumpHeight;
		
		private void JumpButtonRelease() => _jumpButtonReleasedInAir = true;

		protected override void SubscribeOnInput()
		{
			base.SubscribeOnInput();

			inputSystem.jumpInputCancelEventHandler += JumpButtonRelease;
		}

		protected override void UnsubscribeFromInput()
		{
			base.UnsubscribeFromInput();

			inputSystem.jumpInputCancelEventHandler -= JumpButtonRelease;
		}
	}
}
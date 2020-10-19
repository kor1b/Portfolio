using Bimicore.BSG.StateMachinePattern;
using UnityEngine;

namespace Bimicore.BSG.ThirdPerson
{
	public class RunAbility : MovementOnGroundAbstractAbility, IState
	{
		public int StateRestrictorsCount { get; set; } = 0;
	
		public async void OnEnter()
		{
			CommonEnter();
			
			view.StartAnimation (animatorManager);
			
			thirdPersonRotation.TryAlignHorizontalLookRotationWith (inputSystem.InputHorizontalMovementDirection);
			
			CalculateTargetVelocityDirection (inputSystem.InputHorizontalMovementDirection);
			
			AlignTargetVelocity();
			
			// Smooth velocity increasing at the beginning
			await IncreaseTargetVelocity (acceleration, maxSpeed);
		}

		public void Tick()
		{
			CommonTick();
			
			CalculateTargetVelocityDirection (inputSystem.InputHorizontalMovementDirection);
			AlignTargetVelocity();
			
			animatorManager.SetFloatParameter ("HorizontalSpeed",
			                                   Mathf.Abs (physicalProperties.targetVelocity.x) / maxSpeed);
		}

		public void FixedTick()
		{
			CommonFixedTick();
		}

		public void OnExit()
		{
			CommonExit();
		}
	}
}

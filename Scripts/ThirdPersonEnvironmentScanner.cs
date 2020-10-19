using System;
using Bimicore.BSG.CoolTools;
using UnityEngine;

namespace Bimicore.BSG.ThirdPerson
{
	public class ThirdPersonEnvironmentScanner : MonoBehaviour
	{
// Disabled warnings like 'Field is never assigned to, and will always have its default value'
// because structs initialization is in Inspector
#pragma warning disable CS0649

		[SerializeField] private GroundCheck groundCheck;

		[Space] 
		
		[SerializeField] private ObstacleCheck obstacleCheck;

		private ThirdPersonMovementPhysicalProperties _physicalProperties;

#pragma warning restore

		private void Awake()
		{
			_physicalProperties = GetComponent<ThirdPersonMovementPhysicalProperties>();
		}

		private void Update()
		{
			DebugPlus.LogOnScreen("OnGround: " + IsGrounded());
			DebugPlus.LogOnScreen("GroundType: " + GetGroundTypeBelow());
			DebugPlus.LogOnScreen("IsOnSlide: " + IsOnSlide());
		}

		// Warning: method can return true if player are very close to the ground. Modify distToGround to control that.
		public bool IsGrounded()
		{
			var supposedGround = GetGroundHit(groundCheck.ray);
			
			if (supposedGround.collider)
			{
				if (LayersCoolTool.IsSameLayer (groundCheck.groundMask, supposedGround.collider.gameObject.layer))
				{
					return true;
				}
			}

			return false;
		}

		private RaycastHit GetGroundHit(RaySettings raySettings)
		{
			Physics.Raycast(raySettings.origin.position,
			                -transform.up,
			                out var hit,
			                raySettings.checkDistance,
			                groundCheck.groundMask);

			return hit;
		}
		
		public GroundType GetGroundTypeBelow()
		{
			GroundType groundType = GetGroundTypeByRay(GetRayDown());

			return groundType;
		}

		private RaySettings GetRayDown()
		{
			GroundType groundTypeInFront = GetGroundTypeByRay(groundCheck.slope.frontRay);

			// If the ground in front is lower (or absent) than the current ground, then hold on to the last on the current ground.
			if (IsGroundGoesDownOrAbsent(groundTypeInFront))
			{
				return groundCheck.slope.backRay;
			}

			return groundCheck.slope.frontRay;
		}

		private GroundType GetGroundTypeByRay(RaySettings raySettings)
		{
			var ground = GetGroundHit(raySettings);

			float groundAngle = GetGroundAngle(ground);

			return GetGroundTypeByAngle(groundAngle);
		}
		
		public float GetGroundAngle()
		{
			RaycastHit hit = GetGroundHit(GetRayDown());

			// Get the SIGNED angle to slope and multiply it with -1 to define difference btw ascend and descend
			float slopeAngle = Vector2.SignedAngle(hit.normal, Vector2.up) * -1;

			return slopeAngle;
		}

		private float GetGroundAngle(RaycastHit hit) => 
			Vector2.Angle(hit.normal, GetMovementDirectionBasedOnVelocity());

		
		private Vector3 GetMovementDirectionBasedOnVelocity()
		{
			Vector3 directionRelativeToMovement;

			if (_physicalProperties.targetVelocityDirection.x != 0)
				directionRelativeToMovement = new Vector3(Mathf.Sign(_physicalProperties.targetVelocity.x), 0, 0);
			else // if character doesn't move, use face direction
				directionRelativeToMovement = new Vector3(transform.forward.x, 0, 0);

			return directionRelativeToMovement;
		}

		private GroundType GetGroundTypeByAngle(float groundAngle)
		{
			if (Math.Abs(groundAngle - 90) <= groundCheck.slope.groundAngleTolerance)
				return GroundType.Flat;

			if (groundAngle < 90 - groundCheck.slope.groundAngleTolerance && groundAngle > 0)
				return GroundType.Descent;

			if (groundAngle > 90 + groundCheck.slope.groundAngleTolerance)
				return GroundType.Ascent;

			return GroundType.NoGround;
		}

		private bool IsGroundGoesDownOrAbsent(GroundType groundTypeInFront) =>
			groundTypeInFront == GroundType.NoGround || groundTypeInFront == GroundType.Descent;

		public bool IsOnSlide()
		{
			if (GetGroundTypeBelow() == GroundType.Ascent || GetGroundTypeBelow() == GroundType.Descent)
			{
				return Mathf.Abs(GetGroundAngle()) > groundCheck.slide.minAngleToSlide &&
				       Mathf.Abs(GetGroundAngle()) < groundCheck.slide.maxAngleToSlide;
			}

			return false;
		}

		public int GetSlideDirection() => 
			(int) Mathf.Sign(GetGroundHit(GetRayDown()).normal.x);

		public bool IsObstacleInDirection(float direction) =>
			Physics.Raycast(obstacleCheck.ray.origin.position,
			                Vector2.right * direction,
			                obstacleCheck.ray.checkDistance,
			                obstacleCheck.obstacleMask);

		private void OnDrawGizmos()
		{
			// Ground check ray
			Gizmos.color = Color.red;

			Gizmos.DrawRay(groundCheck.ray.origin.position + Vector3.up * 0.2f,
			               -Vector3.up * (groundCheck.ray.checkDistance + 0.1f));

			// Front slope check ray
			Gizmos.color = Color.black;

			Gizmos.DrawRay(groundCheck.slope.frontRay.origin.position,
			               -transform.up * groundCheck.slope.frontRay.checkDistance);

			// Back slope check ray
			Gizmos.color = Color.black;

			Gizmos.DrawRay(groundCheck.slope.backRay.origin.position,
			               -transform.up * groundCheck.slope.backRay.checkDistance);
		}

#pragma warning disable CS0649

		[Serializable]
		private struct GroundCheck
		{
			public LayerMask groundMask;
			public RaySettings ray;
			[Space] public SlopeCheck slope;
			[Space] public SlideCheck slide;
		}

		[Serializable]
		private struct SlopeCheck
		{
			public float groundAngleTolerance;
			public RaySettings frontRay;
			public RaySettings backRay;
		}

		[Serializable]
		private struct SlideCheck
		{
			public float minAngleToSlide;
			public float maxAngleToSlide;
		}

		[Serializable]
		private struct ObstacleCheck
		{
			public LayerMask obstacleMask;
			public RaySettings ray;
		}

		[Serializable]
		private struct RaySettings
		{
			public Transform origin;
			public float checkDistance;
		}

#pragma warning restore
	}
}
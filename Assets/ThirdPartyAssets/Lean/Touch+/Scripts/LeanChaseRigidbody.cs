using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component causes the current Rigidbody to chase the specified position.</summary>
	[RequireComponent(typeof(Rigidbody))]
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanChaseRigidbody")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Chase Rigidbody")]
	public class LeanChaseRigidbody : LeanChase
	{
		/*
		public bool Rotation;

		[Tooltip("How sharp the position value changes update (-1 = instant)")]
		public float RotationDampening = -1.0f;
		*/

		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		[System.NonSerialized]
		protected bool fixedUpdateCalled;

		/// <summary>This method will override the Position value based on the specified value.</summary>
		public override void SetPosition(Vector3 newPosition)
		{
			base.SetPosition(newPosition);

			fixedUpdateCalled = false;
		}

		protected virtual void OnEnable()
		{
			cachedRigidbody = GetComponent<Rigidbody>();
		}

		protected virtual void FixedUpdate()
		{
			if (positionSet == true || Continuous == true)
			{
				if (Destination != null)
				{
					Position = Destination.TransformPoint(DestinationOffset);
				}

				var currentPosition = transform.position;
				var targetPosition  = Position + Offset;

				if (IgnoreZ == true)
				{
					targetPosition.z = currentPosition.z;
				}

				var direction = targetPosition - currentPosition;
				var velocity  = direction / Time.fixedDeltaTime;

				// Apply the velocity
				velocity *= LeanTouch.GetDampenFactor(Dampening, Time.fixedDeltaTime);

				cachedRigidbody.velocity = velocity;

				/*
				if (Rotation == true && direction != Vector3.zero)
				{
					var angle           = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
					var directionB      = (Vector2)transform.up;
					var angleB          = Mathf.Atan2(directionB.x, directionB.y) * Mathf.Rad2Deg;
					var delta           = Mathf.DeltaAngle(angle, angleB);
					var angularVelocity = delta / Time.fixedDeltaTime;

					angularVelocity *= LeanTouch.GetDampenFactor(RotationDampening, Time.fixedDeltaTime);

					//cachedRigidbody.angularVelocity = angularVelocity;
				}
				*/
				fixedUpdateCalled = true;
			}
		}

		protected override void Update()
		{
			if (fixedUpdateCalled == true)
			{
				positionSet       = false;
				fixedUpdateCalled = false;
			}
		}
	}
}
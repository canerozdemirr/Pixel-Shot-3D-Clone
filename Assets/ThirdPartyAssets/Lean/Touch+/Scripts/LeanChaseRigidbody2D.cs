using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This script allows you to drag this Rigidbody2D in a way that causes it to chase the specified position.</summary>
	[RequireComponent(typeof(Rigidbody2D))]
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanChaseRigidbody2D")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Chase Rigidbody2D")]
	public class LeanChaseRigidbody2D : LeanChase
	{
		[System.NonSerialized]
		private Rigidbody2D cachedRigidbody;

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
			cachedRigidbody = GetComponent<Rigidbody2D>();
		}

		protected virtual void FixedUpdate()
		{
			if (positionSet == true || Continuous == true)
			{
				if (Destination != null)
				{
					Position = Destination.TransformPoint(DestinationOffset);
				}

				var currentPosition = (Vector2)(transform.position);
				var targetPosition  = (Vector2)(Position + Offset);

				var direction = targetPosition - currentPosition;
				var velocity  = direction / Time.fixedDeltaTime;

				// Apply the velocity
				velocity *= LeanTouch.GetDampenFactor(Dampening, Time.fixedDeltaTime);

				cachedRigidbody.velocity = velocity;

				fixedUpdateCalled = true;
			}
		}

		protected virtual void LateUpdate()
		{
			if (fixedUpdateCalled == true)
			{
				positionSet       = false;
				fixedUpdateCalled = false;
			}
		}
	}
}
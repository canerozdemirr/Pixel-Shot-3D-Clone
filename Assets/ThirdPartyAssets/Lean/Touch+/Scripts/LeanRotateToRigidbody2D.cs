using UnityEngine;
using Lean.Common;

namespace Lean.Touch
{
	/// <summary>This component automatically rotates the current Rigidbody2D based on movement.</summary>
	[RequireComponent(typeof(Rigidbody2D))]
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanRotateToRigidbody2D")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Rotate To Rigidbody2D")]
	public class LeanRotateToRigidbody2D : MonoBehaviour
	{
		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = 10.0f;

		[HideInInspector]
		[SerializeField]
		private Vector3 previousPosition;

		[HideInInspector]
		[SerializeField]
		private Vector2 vector;

		[System.NonSerialized]
		private Rigidbody2D cachedRigidbody2D;

		protected virtual void OnEnable()
		{
			cachedRigidbody2D = GetComponent<Rigidbody2D>();
		}

		protected virtual void Start()
		{
			previousPosition = transform.position;
		}

		protected virtual void LateUpdate()
		{
			var currentPosition = transform.position;
			var newVector       = (Vector2)(currentPosition - previousPosition);

			if (newVector.sqrMagnitude > 0.0f)
			{
				vector = newVector;
			}

			var currentRotation = transform.localRotation;
			var factor          = LeanHelper.DampenFactor(Dampening, Time.deltaTime);

			if (vector.sqrMagnitude > 0.0f)
			{
				var angle           = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
				var directionB      = (Vector2)transform.up;
				var angleB          = Mathf.Atan2(directionB.x, directionB.y) * Mathf.Rad2Deg;
				var delta           = Mathf.DeltaAngle(angle, angleB);
				var angularVelocity = delta / Time.fixedDeltaTime;

				angularVelocity *= LeanTouch.GetDampenFactor(Dampening, Time.fixedDeltaTime);

				cachedRigidbody2D.angularVelocity = angularVelocity;
			}

			transform.localRotation = Quaternion.Slerp(currentRotation, transform.localRotation, factor);

			previousPosition = currentPosition;
		}
	}
}
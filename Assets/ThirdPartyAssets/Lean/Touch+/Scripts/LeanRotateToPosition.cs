using UnityEngine;
using Lean.Common;

namespace Lean.Touch
{
	/// <summary>This component automatically rotates the current GameObject based on movement.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanRotateToPosition")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Rotate To Position")]
	public class LeanRotateToPosition : MonoBehaviour
	{
		public enum PositionType
		{
			PreviousPosition,
			ManuallySetPosition
		}

		public enum RotateType
		{
			None,
			Forward,
			TopDown,
			Side2D
		}

		/// <summary>This allows you choose the method used to calculate the position we will rotate toward.
		/// PreviousPosition = This component will automatically calculate positions based on the <b>Transform.position</b>.
		/// ManuallySetPosition = You must manually call the <b>SetPosition</b> method to update the rotation.</summary>
		[Tooltip("This allows you choose the method used to calculate the position we will rotate toward.\n\nPreviousPosition = This component will automatically calculate positions based on the Transform.position.\n\nManuallySetPosition = You must manually call the SetPosition method to update the rotation.")]
		public PositionType Position;

		/// <summary>This allows you to set the minimum amount of movement required to trigger the rotation to update. This is useful to prevent tiny movements from causing the rotation to change unexpectedly.</summary>
		[Tooltip("This allows you to set the minimum amount of movement required to trigger the rotation to update. This is useful to prevent tiny movements from causing the rotation to change unexpectedly.")]
		public float Threshold = 0.1f;

		/// <summary>This allows you choose the method used to find the target rotation.</summary>
		[Tooltip("This allows you choose the method used to find the target rotation.")]
		public RotateType RotateTo;

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
		private Vector3 previousDelta;

		/// <summary>If <b>Position</b> is set to <b>ManuallySetPosition</b>, then this method allows you to set the position we will rotate to.</summary>
		public void SetPosition(Vector3 position)
		{
			var currentPosition = transform.position;

			if (Vector3.Distance(currentPosition, position) > Threshold)
			{
				SetDelta(position - currentPosition);
			}
		}

		/// <summary>This method allows you to override the position delta used to calculate the rotation.
		/// NOTE: This should be non-zero.</summary>
		public void SetDelta(Vector3 delta)
		{
			if (delta.sqrMagnitude > 0.0f)
			{
				previousDelta = delta;
			}
		}

		/// <summary>If your <b>Transform</b> has teleported, then call this to reset the cached position.</summary>
		public void ResetPosition()
		{
			previousPosition = transform.position;
		}

		protected virtual void Start()
		{
			ResetPosition();
		}

		protected virtual void OnEnable()
		{
			ResetPosition();
		}

		protected virtual void LateUpdate()
		{
			// Update position and delta
			var currentPosition = transform.position;

			if (Position == PositionType.PreviousPosition && Vector3.Distance(previousPosition, currentPosition) > Threshold)
			{
				SetDelta(currentPosition - previousPosition);

				previousPosition = currentPosition;
			}

			// Update rotation
			var currentRotation = transform.localRotation;
			var factor          = LeanHelper.DampenFactor(Dampening, Time.deltaTime);

			if (previousDelta.sqrMagnitude > 0.0f)
			{
				UpdateRotation(previousDelta);
			}

			transform.localRotation = Quaternion.Slerp(currentRotation, transform.localRotation, factor);
		}

		private void UpdateRotation(Vector3 vector)
		{
			switch (RotateTo)
			{
				case RotateType.Forward:
				{
					transform.forward = vector;
				}
				break;

				case RotateType.TopDown:
				{
					var yaw = Mathf.Atan2(vector.x, vector.z) * Mathf.Rad2Deg;

					transform.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
				}
				break;

				case RotateType.Side2D:
				{
					var roll = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;

					transform.rotation = Quaternion.Euler(0.0f, 0.0f, -roll);
				}
				break;
			}
		}
	}
}
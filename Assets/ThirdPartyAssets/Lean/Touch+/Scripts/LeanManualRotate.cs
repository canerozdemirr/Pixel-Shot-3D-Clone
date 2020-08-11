using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to rotate the current GameObject using events.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanManualRotate")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Manual Rotate")]
	public class LeanManualRotate : MonoBehaviour
	{
		/// <summary>If you want this component to work on a different GameObject, then specify it here. This can be used to improve organization if your GameObject already has many components.</summary>
		[Tooltip("If you want this component to work on a different GameObject, then specify it here. This can be used to improve organization if your GameObject already has many components.")]
		public GameObject Target;

		/// <summary>This allows you to set the coordinate space the rotation will use.</summary>
		[Tooltip("This allows you to set the coordinate space the rotation will use.")]
		public Space Space;

		/// <summary>The first rotation axis, used when calling RotateA or RotateAB.</summary>
		[Tooltip("The first rotation axis, used when calling RotateA or RotateAB.")]
		public Vector3 AxisA = Vector3.down;

		/// <summary>The second rotation axis, used when calling RotateB or RotateAB.</summary>
		[Tooltip("The second rotation axis, used when calling RotateB or RotateAB.")]
		public Vector3 AxisB = Vector3.right;

		/// <summary>The rotation angle is multiplied by this.
		/// 1 = Normal rotation.
		/// 2 = Double rotation.</summary>
		[Tooltip("The rotation angle is multiplied by this.\n\n1 = Normal rotation.\n\n2 = Double rotation.")]
		public float Multiplier = 1.0f;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = 10.0f;

		/// <summary>If you enable this then the rotation will be multiplied by Time.deltaTime. This allows you to maintain framerate independent rotation.</summary>
		[Tooltip("If you enable this then the rotation will be multiplied by Time.deltaTime. This allows you to maintain framerate independent rotation.")]
		public bool ScaleByTime;

		[Space]

		/// <summary>If you call the ResetRotation method, the rotation will be set to this Euler rotation.</summary>
		[Tooltip("If you call the ResetRotation method, the rotation will be set to this Euler rotation.")]
		public Vector3 DefaultRotation;

		[HideInInspector]
		[SerializeField]
		private Quaternion remainingDelta = Quaternion.identity;

		/// <summary>This method will reset the rotation to the specified DefaultRotation value.</summary>
		[ContextMenu("Reset Rotation")]
		public void ResetRotation()
		{
			var finalTransform = Target != null ? Target.transform : transform;
			var oldRotation    = finalTransform.localRotation;

			if (Space == Space.Self)
			{
				finalTransform.localRotation = Quaternion.Euler(DefaultRotation);
			}
			else
			{
				finalTransform.rotation = Quaternion.Euler(DefaultRotation);
			}

			remainingDelta *= Quaternion.Inverse(oldRotation) * finalTransform.localRotation;

			// Revert
			finalTransform.localRotation = oldRotation;
		}

		/// <summary>This method allows you to rotate around AxisA, with the specified angle in degrees.</summary>
		public void RotateA(float delta)
		{
			RotateAB(new Vector2(delta, 0.0f));
		}

		/// <summary>This method allows you to rotate around AxisB, with the specified angle in degrees.</summary>
		public void RotateB(float delta)
		{
			RotateAB(new Vector2(0.0f, delta));
		}

		/// <summary>This method allows you to rotate around AxisA and AxisB, with the specified angles in degrees.</summary>
		public void RotateAB(Vector2 delta)
		{
			var finalTransform = Target != null ? Target.transform : transform;
			var oldRotation    = finalTransform.localRotation;

			if (ScaleByTime == true)
			{
				delta *= Time.deltaTime;
			}

			finalTransform.Rotate(AxisA, delta.x * Multiplier, Space);
			finalTransform.Rotate(AxisB, delta.y * Multiplier, Space);

			remainingDelta *= Quaternion.Inverse(oldRotation) * finalTransform.localRotation;

			// Revert
			finalTransform.localRotation = oldRotation;
		}

		protected virtual void Update()
		{
			var finalTransform = Target != null ? Target.transform : transform;
			var factor         = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);
			var newDelta       = Quaternion.Slerp(remainingDelta, Quaternion.identity, factor);

			finalTransform.localRotation = finalTransform.localRotation * Quaternion.Inverse(newDelta) * remainingDelta;

			remainingDelta = newDelta;
		}
	}
}
using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component causes the current Transform to chase the specified position.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanChase")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Chase")]
	public class LeanChase : MonoBehaviour
	{
		/// <summary>The transform that will be chased.</summary>
		[Tooltip("The transform that will be chased.")]
		public Transform Destination;

		/// <summary>If Target is set, then this allows you to set the offset in local space.</summary>
		[Tooltip("If Target is set, then this allows you to set the offset in local space.")]
		public Vector3 DestinationOffset;

		[Space]

		/// <summary>The world space position that will be followed.
		/// NOTE: If Destination is set, then this value will be overridden.</summary>
		[Tooltip("The world space position that will be followed.\n\nNOTE: If Destination is set, then this value will be overridden.")]
		public Vector3 Position;

		/// <summary>The world space offset that will be followed.</summary>
		[Tooltip("The world space offset that will be followed.")]
		public Vector3 Offset;

		[Space]

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = -1.0f;

		/// <summary>Ignore Z for 2D?</summary>
		[Tooltip("Ignore Z for 2D?")]
		public bool IgnoreZ;

		/// <summary>Should the chase keep updating, even if you haven't called the SetPosition method this frame?</summary>
		[Tooltip("Should the chase keep updating, even if you haven't called the SetPosition method this frame?")]
		public bool Continuous;

		/// <summary>Automatically set the Position value to the transform.position in Start?</summary>
		[Tooltip("Automatically set the Position value to the transform.position in Start?")]
		public bool SetPositionOnStart = true;

		[System.NonSerialized]
		protected bool positionSet;

		/// <summary>This method will override the Position value based on the specified value.</summary>
		public virtual void SetPosition(Vector3 newPosition)
		{
			Destination = null;
			Position    = newPosition;
			positionSet = true;
		}

		/// <summary>This method will override the Position value based on the center point of all selected objects, if at least one exists.</summary>
		[ContextMenu("Set Position Selection")]
		public void SetPositionSelection()
		{
			var center = default(Vector3);
			var count  = 0;

			foreach (var selectable in LeanSelectable.Instances)
			{
				if (selectable.IsSelected == true)
				{
					center += selectable.transform.position;
					count  += 1;
				}
			}

			if (count > 0)
			{
				SetPosition(center / count);
			}
		}

		protected virtual void Start()
		{
			if (SetPositionOnStart == true)
			{
				Position = transform.position;
			}
		}

		protected virtual void Update()
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

				// Get t value
				var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

				// Lerp the current values to the target ones
				currentPosition = Vector3.Lerp(currentPosition, targetPosition, factor);

				// Apply new point
				transform.position = currentPosition;

				positionSet = false;
			}
		}
	}
}
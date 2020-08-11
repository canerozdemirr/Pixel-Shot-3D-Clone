using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Lean.Touch
{
	/// <summary>This script allows you to twist the selected object around like a dial or knob.</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanSelectableDial")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Selectable Dial")]
	public class LeanSelectableDial : LeanSelectableBehaviour
	{
		[System.Serializable]
		public class Trigger
		{
			[Tooltip("The central Angle of this trigger in degrees.")]
			public float Angle;

			[Tooltip("The angle range of this trigger in degrees.\n\n90 = Quarter circle.\n180 = Half circle.")]
			public float Arc;

			[HideInInspector]
			public bool Inside;

			public UnityEvent OnEnter { get { if (onEnter == null) onEnter = new UnityEvent(); return onEnter; } } [SerializeField] private UnityEvent onEnter;

			public UnityEvent OnExit { get { if (onExit == null) onExit = new UnityEvent(); return onExit; } } [SerializeField] private UnityEvent onExit;

			public bool IsInside(float angle, bool clamp)
			{
				var range = Arc * 0.5f;

				if (clamp == false)
				{
					var delta  = Mathf.Abs(Mathf.DeltaAngle(Angle, angle));

					return delta < range;
				}

				return angle >= Angle - range && angle <= Angle + range;
			}
		}

		/// <summary>The camera we will be used.
		/// None = MainCamera.</summary>
		[Tooltip("The camera we will be used.\n\nNone = MainCamera.")]
		public Camera Camera;

		/// <summary>The base rotation in local space.</summary>
		[Tooltip("The base rotation in local space.")]
		public Vector3 Tilt;

		/// <summary>The axis of the rotation in local space.</summary>
		[Tooltip("The axis of the rotation in local space.")]
		public Vector3 Axis = Vector3.up;

		/// <summary>The angle of the dial in degrees.</summary>
		[Tooltip("The angle of the dial in degrees.")]
		public float Angle;

		[Space]

		/// <summary>Should the Angle value be clamped?</summary>
		[Tooltip("Should the Angle value be clamped?")]
		public bool Clamp;

		/// <summary>The minimum Angle value.</summary>
		[Tooltip("The minimum Angle value.")]
		public float ClampMin = -45.0f;

		/// <summary>The maximum Angle value.</summary>
		[Tooltip("The maximum Angle value.")]
		public float ClampMax = 45.0f;

		[Space]

		/// <summary>This allows you to perform a custom event when the dial is within a specifid angle range.</summary>
		[Tooltip("This allows you to perform a custom event when the dial is within a specifid angle range.")]
		public List<Trigger> Triggers;

		private Vector2 oldPoint;

		private bool oldPointSet;
#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.DrawLine(transform.position, transform.TransformPoint(Axis));
		}
#endif
		protected virtual void Update()
		{
			// Reset rotation and get axis
			transform.localEulerAngles = Tilt;

			// Is this GameObject selected?
			if (Selectable != null && Selectable.IsSelected == true)
			{
				// Does it have a selected finger?
				var finger = Selectable.SelectingFinger;

				if (finger != null)
				{
					var newPoint = GetPoint(finger.ScreenPosition);

					if (oldPointSet == true)
					{
						Angle += Vector2.SignedAngle(newPoint, oldPoint);
					}

					oldPoint    = newPoint;
					oldPointSet = true;
				}
			}
			else
			{
				oldPointSet = false;
			}

			if (Clamp == true)
			{
				Angle = Mathf.Clamp(Angle, ClampMin, ClampMax);
			}

			transform.Rotate(Axis, Angle, Space.Self);

			if (Triggers != null)
			{
				for (var i = 0; i < Triggers.Count; i++)
				{
					var trigger = Triggers[i];

					if (trigger.IsInside(Angle, Clamp) == true)
					{
						if (trigger.Inside == false)
						{
							trigger.Inside = true;

							trigger.OnEnter.Invoke();
						}
					}
					else
					{
						if (trigger.Inside == true)
						{
							trigger.Inside = false;

							trigger.OnExit.Invoke();
						}
					}
				}
			}
		}

		private Vector2 GetPoint(Vector2 screenPoint)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				var rectTransform = transform as RectTransform;

				if (rectTransform != null)
				{
					/*
					var localPoint = default(Vector2);

					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out localPoint) == true)
					{
						return new Vector2(-localPoint.x, localPoint.y);
					}
					*/
					var worldPoint = default(Vector3);

					if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, camera, out worldPoint) == true)
					{
						var vector = Quaternion.LookRotation(Axis) * transform.InverseTransformPoint(worldPoint);

						vector.x = -vector.x;

						return vector;
					}
				}
				else
				{
					var ray      = camera.ScreenPointToRay(screenPoint);
					var plane    = new Plane(transform.TransformDirection(Axis), transform.position);
					var distance = default(float);

					if (plane.Raycast(ray, out distance) == true)
					{
						var vector = ray.GetPoint(distance) - transform.position;

						return Quaternion.LookRotation(Axis) * vector;
					}
				}
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}

			return oldPoint;
		}
	}
}
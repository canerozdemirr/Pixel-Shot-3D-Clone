using UnityEngine;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Touch
{
	/// <summary>This component allows you to zoom a camera in and out based on the pinch gesture
	/// This supports both perspective and orthographic cameras</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanPinchCamera")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Pinch Camera")]
	public class LeanPinchCamera : MonoBehaviour
	{
		/// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
		public LeanFingerFilter Use = new LeanFingerFilter(true);

		/// <summary>The camera that will be used during calculations.
		/// None = MainCamera.</summary>
		[Tooltip("The camera that will be used during calculations.\n\nNone = MainCamera.")]
		public Camera Camera;

		/// <summary>The current FOV/Size.</summary>
		[Tooltip("The current FOV/Size.")]
		public float Zoom = 50.0f;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = -1.0f;

		/// <summary>Limit the FOV/Size?</summary>
		[Tooltip("Limit the FOV/Size?")]
		[UnityEngine.Serialization.FormerlySerializedAs("ZoomClamp")]
		public bool Clamp;

		/// <summary>The minimum FOV/Size we want to zoom to.</summary>
		[Tooltip("The minimum FOV/Size we want to zoom to.")]
		[UnityEngine.Serialization.FormerlySerializedAs("ZoomMin")]
		public float ClampMin = 10.0f;

		/// <summary>The maximum FOV/Size we want to zoom to.</summary>
		[Tooltip("The maximum FOV/Size we want to zoom to.")]
		[UnityEngine.Serialization.FormerlySerializedAs("ZoomMax")]
		public float ClampMax = 60.0f;

		/// <summary>Should the zoom be performanced relative to the finger center?</summary>
		[Tooltip("Should the zoom be performanced relative to the finger center?")]
		public bool Relative;

		/// <summary>Ignore changes in Z translation for 2D?</summary>
		[Tooltip("Ignore changes in Z translation for 2D?")]
		public bool IgnoreZ;

		/// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
		public LeanScreenDepth ScreenDepth = new LeanScreenDepth(LeanScreenDepth.ConversionType.DepthIntercept);

		[HideInInspector]
		[SerializeField]
		private float currentZoom;

		[HideInInspector]
		[SerializeField]
		private Vector3 remainingTranslation;

		public void ContinuouslyZoom(float direction)
		{
			var factor = LeanTouch.GetDampenFactor(Mathf.Abs(direction), Time.deltaTime);

			if (direction > 0.0f)
			{
				Zoom = Mathf.Lerp(Zoom, ClampMax, factor);
			}
			else if (direction <= 0.0f)
			{
				Zoom = Mathf.Lerp(Zoom, ClampMin, factor);
			}
		}

		/// <summary>This method allows you to multiply the current <b>Zoom</b> value by the specified scale. This is useful for quickly changing the zoom from UI button clicks, or <b>LeanMouseWheel</b> scrolling.</summary>
		public void MultiplyZoom(float scale)
		{
			Zoom *= scale;

			if (Clamp == true)
			{
				Zoom = Mathf.Clamp(Zoom, ClampMin, ClampMax);
			}
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
		public void AddFinger(LeanFinger finger)
		{
			Use.AddFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
		public void RemoveFinger(LeanFinger finger)
		{
			Use.RemoveFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
		public void RemoveAllFingers()
		{
			Use.RemoveAllFingers();
		}
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}
#endif
		protected virtual void Awake()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}

		protected virtual void Start()
		{
			currentZoom = Zoom;
		}

		protected virtual void LateUpdate()
		{
			// Get the fingers we want to use
			var fingers = Use.GetFingers();

			// Get the pinch ratio of these fingers
			var pinchRatio = LeanGesture.GetPinchRatio(fingers);

			// Store
			var oldPosition = transform.localPosition;

			// Make sure the zoom value is valid
			Zoom = TryClamp(Zoom);

			if (pinchRatio != 1.0f)
			{
				// Store old zoom value and then modify zoom
				var oldZoom = Zoom;

				Zoom = TryClamp(Zoom * pinchRatio);

				// Zoom relative to a point on screen?
				if (Relative == true)
				{
					var screenPoint = default(Vector2);

					if (LeanGesture.TryGetScreenCenter(fingers, ref screenPoint) == true)
					{
						// Derive actual pinchRatio from the zoom delta (it may differ with clamping)
						pinchRatio = Zoom / oldZoom;

						var worldPoint = ScreenDepth.Convert(screenPoint);

						transform.position = worldPoint + (transform.position - worldPoint) * pinchRatio;

						// Increment
						remainingTranslation += transform.localPosition - oldPosition;

						if (IgnoreZ == true)
						{
							remainingTranslation.z = 0.0f;
						}
					}
				}
			}

			// Get t value
			var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

			// Lerp the current value to the target one
			currentZoom = Mathf.Lerp(currentZoom, Zoom, factor);

			// Set the new zoom
			SetZoom(currentZoom);

			// Dampen remainingDelta
			var newRemainingTranslation = Vector3.Lerp(remainingTranslation, Vector3.zero, factor);

			// Shift this transform by the change in delta
			transform.localPosition = oldPosition + remainingTranslation - newRemainingTranslation;

			// Update remainingDelta with the dampened value
			remainingTranslation = newRemainingTranslation;
		}

		protected void SetZoom(float current)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				if (camera.orthographic == true)
				{
					camera.orthographicSize = current;
				}
				else
				{
					camera.fieldOfView = current;
				}
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}

		private float TryClamp(float z)
		{
			if (Clamp == true)
			{
				z = Mathf.Clamp(z, ClampMin, ClampMax);
			}

			return z;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanPinchCamera))]
	public class LeanPinchCamera_Inspector : LeanInspector<LeanPinchCamera>
	{
		protected override void DrawInspector()
		{
			Draw("Use");
			Draw("Camera");
			Draw("Zoom");
			Draw("Dampening");
			Draw("Clamp");
			if (Any(t => t.Clamp == true))
			{
				EditorGUI.indentLevel++;
					Draw("ClampMin", null, "Min");
					Draw("ClampMax", null, "Max");
				EditorGUI.indentLevel--;
			}

			Draw("Relative");

			if (Any(t => t.Relative == true))
			{
				EditorGUI.indentLevel++;
					Draw("IgnoreZ");
					Draw("ScreenDepth");
				EditorGUI.indentLevel--;
			}
		}
	}
}
#endif
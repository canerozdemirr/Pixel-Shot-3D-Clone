using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component controls the current GameObject's rotation, based on the specified Pitch and Yaw values.</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanPitchYaw")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Pitch Yaw")]
	public class LeanPitchYaw : MonoBehaviour
	{
		/// <summary>If you want the rotation to be scaled by the camera FOV, then set the camera here.</summary>
		[Tooltip("If you want the rotation to be scaled by the camera FOV, then set the camera here.")]
		public Camera Camera;

		/// <summary>This allows you to set the Pitch andYaw rotation value when calling the ResetRotation method.</summary>
		[Tooltip("This allows you to set the Pitch and Yaw rotation value when calling the ResetRotation method.")]
		public Vector2 DefaultRotation;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = -1.0f;

		[Space]

		/// <summary>Pitch of the rotation in degrees.</summary>
		[Tooltip("Pitch of the rotation in degrees.")]
		public float Pitch;

		/// <summary>The strength of the pitch changes with vertical finger movement.</summary>
		[Tooltip("The strength of the pitch changes with vertical finger movement.")]
		public float PitchSensitivity = 0.25f;

		/// <summary>Limit the pitch to min/max?</summary>
		[Tooltip("Limit the pitch to min/max?")]
		public bool PitchClamp = true;

		/// <summary>The minimum pitch angle in degrees.</summary>
		[Tooltip("The minimum pitch angle in degrees.")]
		public float PitchMin = -90.0f;

		/// <summary>The maximum pitch angle in degrees.</summary>
		[Tooltip("The maximum pitch angle in degrees.")]
		public float PitchMax = 90.0f;

		[Space]

		/// <summary>Yaw of the rotation in degrees.</summary>
		[Tooltip("Yaw of the rotation in degrees.")]
		public float Yaw;

		/// <summary>The strength of the yaw changes with horizontal finger movement.</summary>
		[Tooltip("The strength of the yaw changes with horizontal finger movement.")]
		public float YawSensitivity = 0.25f;

		/// <summary>Limit the yaw to min/max?</summary>
		[Tooltip("Limit the yaw to min/max?")]
		public bool YawClamp;

		/// <summary>The minimum yaw angle in degrees.</summary>
		[Tooltip("The minimum yaw angle in degrees.")]
		public float YawMin = -45.0f;

		/// <summary>The maximum yaw angle in degrees.</summary>
		[Tooltip("The maximum yaw angle in degrees.")]
		public float YawMax = 45.0f;

		[HideInInspector]
		[SerializeField]
		private float currentPitch;

		[HideInInspector]
		[SerializeField]
		private float currentYaw;

		/// <summary>This method resets the Pitch and Yaw values to the DefaultRotation value.</summary>
		[ContextMenu("Reset Rotation")]
		public virtual void ResetRotation()
		{
			Pitch = DefaultRotation.x;
			Yaw   = DefaultRotation.y;
		}

		/// <summary>This allows you to click the screen when using a perspective camera, and have the Pitch and Yaw values update to that point.</summary>
		public void RotateToFinger(LeanFinger finger)
		{
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				var xyz       = finger.GetRay(camera).direction;
				var longitude = Mathf.Atan2(xyz.x, xyz.z);
				var latitude  = Mathf.Asin(xyz.y / xyz.magnitude);
				var newPitch  = latitude  * -Mathf.Rad2Deg;
				var newYaw    = longitude *  Mathf.Rad2Deg;
				var delta     = Mathf.DeltaAngle(Yaw, newYaw);

				Pitch = newPitch;
				Yaw  += delta;
			}
		}

		public void Rotate(Vector2 delta)
		{
			var sensitivity = GetSensitivity();

			Yaw   += delta.x *   YawSensitivity * sensitivity;
			Pitch -= delta.y * PitchSensitivity * sensitivity;
		}

		public void RotatePitch(float delta)
		{
			Pitch -= delta * PitchSensitivity * GetSensitivity();
		}

		public void RotateYaw(float delta)
		{
			Yaw += delta * YawSensitivity * GetSensitivity();
		}

		protected virtual void Start()
		{
			currentPitch = Pitch;
			currentYaw   = Yaw;
		}

		protected virtual void LateUpdate()
		{
			if (PitchClamp == true)
			{
				Pitch = Mathf.Clamp(Pitch, PitchMin, PitchMax);
			}

			if (YawClamp == true)
			{
				Yaw = Mathf.Clamp(Yaw, YawMin, YawMax);
			}

			// Get t value
			var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

			// Lerp the current values to the target ones
			currentPitch = Mathf.Lerp(currentPitch, Pitch, factor);
			currentYaw   = Mathf.Lerp(currentYaw  , Yaw  , factor);

			// Rotate to pitch and yaw values
			transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0.0f);
		}

		private float GetSensitivity()
		{
			// Has a camera been set?
			if (Camera != null)
			{
				// Adjust sensitivity by FOV?
				if (Camera.orthographic == false)
				{
					return Camera.fieldOfView / 90.0f;
				}
			}

			return 1.0f;
		}
	}
}
using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component will constrain the current <b>transform.position</b> to the specified <b>LeanPlane</b> shape.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanConstrainToOrthographic")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Constrain To Orthographic")]
	public class LeanConstrainToOrthographic : MonoBehaviour
	{
		[Tooltip("The camera whose orthographic size will be used.")]
		public Camera Camera;

		[Tooltip("The plane this transform will be constrained to")]
		public LeanPlane Plane;

		protected virtual void LateUpdate()
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				if (Plane != null)
				{
					var ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
					var hit = default(Vector3);

					if (Plane.TryRaycast(ray, ref hit, 0.0f, false) == true)
					{
						var delta   = transform.position - hit;
						var local   = Plane.transform.InverseTransformPoint(hit);
						var snapped = local;
						var size    = new Vector2(Camera.orthographicSize * Camera.aspect, Camera.orthographicSize);

						if (Plane.ClampX == true)
						{
							var min = Plane.MinX + size.x;
							var max = Plane.MaxX - size.x;

							if (min > max)
							{
								snapped.x = (min + max) * 0.5f;
							}
							else
							{
								snapped.x = Mathf.Clamp(local.x, min, max);
							}
						}

						if (Plane.ClampY == true)
						{
							var min = Plane.MinY + size.y;
							var max = Plane.MaxY - size.y;

							if (min > max)
							{
								snapped.y = (min + max) * 0.5f;
							}
							else
							{
								snapped.y = Mathf.Clamp(local.y, min, max);
							}
						}

						if (local != snapped)
						{
							transform.position = Plane.transform.TransformPoint(snapped) + delta;
						}
					}
				}
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}
	}
}
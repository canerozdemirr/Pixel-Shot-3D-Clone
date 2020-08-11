using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component will constrain the current <b>transform.position</b> to the specified <b>BoxCollider</b> shape.
	/// NOTE: Unlike <b>LeanConstrainToCollider</b>, this component doesn't use the physics system, so it can avoid certain issues if your constrain shape moves.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanConstrainToBox")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Constrain To Box")]
	public class LeanConstrainToBox : MonoBehaviour
	{
		[Tooltip("The box collider this transform will be constrained to")]
		public BoxCollider Collider;

		protected virtual void LateUpdate()
		{
			if (Collider != null)
			{
				var local = Collider.transform.InverseTransformPoint(transform.position);
				var min   = Collider.center - Collider.size * 0.5f;
				var max   = Collider.center + Collider.size * 0.5f;
				var set   = false;

				if (local.x < min.x) { local.x = min.x; set = true; }
				if (local.y < min.y) { local.y = min.y; set = true; }
				if (local.z < min.z) { local.z = min.z; set = true; }
				if (local.x > max.x) { local.x = max.x; set = true; }
				if (local.y > max.y) { local.y = max.y; set = true; }
				if (local.z > max.z) { local.z = max.z; set = true; }

				if (set == true)
				{
					transform.position = Collider.transform.TransformPoint(local);
				}
			}
		}
	}
}
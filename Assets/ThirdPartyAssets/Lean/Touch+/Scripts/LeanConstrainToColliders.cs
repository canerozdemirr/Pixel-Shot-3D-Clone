using UnityEngine;
using System.Collections.Generic;

namespace Lean.Touch
{
	/// <summary>This script will constrain the current transform.position to the specified colliders.
	/// NOTE: If you're using a MeshCollider then it must be marked as <b>convex</b>.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanConstrainToColliders")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Constrain To Colliders")]
	public class LeanConstrainToColliders : MonoBehaviour
	{
		/// <summary>The colliders this transform will be constrained to.</summary>
		[Tooltip("The colliders this transform will be constrained to.")]
		public List<Collider> Colliders;

		protected virtual void LateUpdate()
		{
			if (Colliders != null)
			{
				var position = transform.position;
				var closest  = default(Vector3);
				var distance = float.PositiveInfinity;
				var count    = 0;
				var moved    = 0;

				for (var i = Colliders.Count - 1; i >= 0; i--)
				{
					var collider = Colliders[i];

					if (collider != null)
					{
						var newPosition = collider.ClosestPoint(position);

						if (newPosition != position)
						{
							moved++;

							var newDistance = Vector3.SqrMagnitude(newPosition - position);
							
							if (newDistance < distance)
							{
								distance = newDistance;
								closest  = newPosition;
							}
						}

						count++;
					}
				}

				if (count > 0 && count == moved)
				{
					transform.position = closest;
				}
			}
		}
	}
}
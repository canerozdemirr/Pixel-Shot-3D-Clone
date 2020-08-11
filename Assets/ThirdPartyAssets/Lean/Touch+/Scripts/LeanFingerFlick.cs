using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Touch
{
	/// <summary>This component detects swipes while the finger is touching the screen.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanFingerFlick")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Finger Flick")]
	public class LeanFingerFlick : LeanSwipeBase
	{
		public float Interval = -1.0f;

		/// <summary>Ignore fingers with StartedOverGui?</summary>
		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreStartedOverGui = true;

		/// <summary>Ignore fingers with IsOverGui?</summary>
		[Tooltip("Ignore fingers with IsOverGui?")]
		public bool IgnoreIsOverGui;

		/// <summary>Do nothing if this LeanSelectable isn't selected?</summary>
		[Tooltip("Do nothing if this LeanSelectable isn't selected?")]
		public LeanSelectable RequiredSelectable;

		private List<LeanFinger> fingers = new List<LeanFinger>();
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			RequiredSelectable = GetComponentInParent<LeanSelectable>();
		}
#endif
		protected virtual void Awake()
		{
			if (RequiredSelectable == null)
			{
				RequiredSelectable = GetComponentInParent<LeanSelectable>();
			}
		}

		protected virtual void OnEnable()
		{
			LeanTouch.OnFingerDown += HandleFingerDown;
			LeanTouch.OnFingerUp   += HandleFingerUp;
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerDown -= HandleFingerDown;
			LeanTouch.OnFingerUp   -= HandleFingerUp;

			fingers.Clear();
		}

		protected virtual void Update()
		{
			for (var i = fingers.Count - 1; i >= 0; i--)
			{
				var finger     = fingers[i];
				var screenFrom = default(Vector2);
				var screenTo   = default(Vector2);

				if (TestFinger(finger, ref screenFrom, ref screenTo) == true)
				{
					fingers.RemoveAt(i);

					HandleFingerSwipe(finger, screenFrom, screenTo);
				}
			}
		}

		private void HandleFingerDown(LeanFinger finger)
		{
			fingers.Add(finger);
		}

		private void HandleFingerUp(LeanFinger finger)
		{
			fingers.Remove(finger);
		}

		private bool TestFinger(LeanFinger finger, ref Vector2 screenFrom, ref Vector2 screenTo)
		{
			if (IgnoreStartedOverGui == true && finger.StartedOverGui == true)
			{
				return false;
			}

			if (IgnoreIsOverGui == true && finger.IsOverGui == true)
			{
				return false;
			}

			if (RequiredSelectable != null && RequiredSelectable.IsSelectedBy(finger) == false)
			{
				return false;
			}

			if (finger.Age >= LeanTouch.CurrentTapThreshold)
			{
				return false;
			}

			var scalingFactor = LeanTouch.ScalingFactor;
			var sqrThreshold  = LeanTouch.CurrentSwipeThreshold / scalingFactor; sqrThreshold *= sqrThreshold;

			screenTo = finger.ScreenPosition;

			for (var i = finger.Snapshots.Count - 1; i >= 0; i--)
			{
				screenFrom = finger.Snapshots[i].ScreenPosition;

				var screenDelta = screenTo - screenFrom;

				// Valid distance and angle?
				if (screenDelta.sqrMagnitude >= sqrThreshold && AngleIsValid(screenDelta) == true)
				{
					return true;
				}
			}

			return false;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanFingerFlick))]
	public class LeanFingerFlick_Inspector : LeanSwipeBase_Inspector<LeanFingerFlick>
	{
		protected override void DrawInspector()
		{
			Draw("IgnoreStartedOverGui");
			Draw("IgnoreIsOverGui");
			Draw("RequiredSelectable");

			base.DrawInspector();
		}
	}
}
#endif
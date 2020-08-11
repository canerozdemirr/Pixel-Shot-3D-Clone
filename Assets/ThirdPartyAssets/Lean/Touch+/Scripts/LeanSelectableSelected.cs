using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Touch
{
	/// <summary>This component fires events when the selectable has been selected for a certain amount of time.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanSelectableSelected")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Selectable Selected")]
	public class LeanSelectableSelected : LeanSelectableBehaviour
	{
		public enum ResetType
		{
			None,
			OnSelect,
			OnDeselect
		}

		// Event signature
		[System.Serializable] public class SelectableEvent : UnityEvent<LeanSelectable> {}

		[Tooltip("The finger must be held for this many seconds")]
		public float Threshold = 1.0f;

		[Tooltip("When should Seconds be reset to 0?")]
		public ResetType Reset = ResetType.OnDeselect;

		[Tooltip("Bypass LeanSelectable.HideWithFinger?")]
		public bool RawSelection;

		[Tooltip("If the selecting finger went up, cancel timer?")]
		public bool RequireFinger;

		/// <summary>Called on the first frame the conditions are met.</summary>
		public SelectableEvent OnSelectableDown { get { if (onSelectableDown == null) onSelectableDown = new SelectableEvent(); return onSelectableDown; } } [FormerlySerializedAs("onDown")] [FormerlySerializedAs("OnDown")] [SerializeField] private SelectableEvent onSelectableDown;

		/// <summary>Called on every frame the conditions are met.</summary>
		public SelectableEvent OnSelectableUpdate { get { if (onSelectableUpdate == null) onSelectableUpdate = new SelectableEvent(); return onSelectableUpdate; } } [FormerlySerializedAs("onSelectableSet")] [FormerlySerializedAs("onSet")] [FormerlySerializedAs("OnSet")] [SerializeField] private SelectableEvent onSelectableUpdate;

		/// <summary>Called on the last frame the conditions are met.</summary>
		public SelectableEvent OnSelectableUp { get { if (onSelectableUp == null) onSelectableUp = new SelectableEvent(); return onSelectableUp; } } [FormerlySerializedAs("onUp")] [FormerlySerializedAs("OnUp")] [SerializeField] private SelectableEvent onSelectableUp;

		[HideInInspector]
		[SerializeField]
		private bool lastSet;

		[HideInInspector]
		[SerializeField]
		private float seconds;

		protected virtual void Update()
		{
			// See if the timer can be incremented
			var set = false;

			if (Selectable != null && Selectable.GetIsSelected(RawSelection) == true)
			{
				if (RequireFinger == false || Selectable.SelectingFinger != null)
				{
					seconds += Time.deltaTime;

					if (seconds >= Threshold)
					{
						set = true;
					}
				}
			}

			// If this is the first frame of set, call down
			if (set == true && lastSet == false)
			{
				if (onSelectableDown != null)
				{
					onSelectableDown.Invoke(Selectable);
				}
			}

			// Call set every time if set
			if (set == true)
			{
				if (onSelectableUpdate != null)
				{
					onSelectableUpdate.Invoke(Selectable);
				}
			}

			// Store last value
			lastSet = set;
		}

		protected override void OnSelect(LeanFinger finger)
		{
			if (Reset == ResetType.OnSelect)
			{
				seconds = 0.0f;
			}

			// Reset value
			lastSet = false;
		}

		protected override void OnDeselect()
		{
			if (Reset == ResetType.OnDeselect)
			{
				seconds = 0.0f;
			}

			if (lastSet == true)
			{
				if (onSelectableUp != null)
				{
					onSelectableUp.Invoke(Selectable);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanSelectableSelected))]
	public class LeanSelectableSelected_Inspector : LeanInspector<LeanSelectableSelected>
	{
		private bool showUnusedEvents;

		protected override void DrawInspector()
		{
			Draw("Threshold");
			Draw("Reset");
			Draw("RawSelection");
			Draw("RequireFinger");

			EditorGUILayout.Separator();

			var usedA = Any(t => t.OnSelectableDown.GetPersistentEventCount() > 0);
			var usedB = Any(t => t.OnSelectableUpdate.GetPersistentEventCount() > 0);
			var usedC = Any(t => t.OnSelectableUp.GetPersistentEventCount() > 0);

			EditorGUI.BeginDisabledGroup(usedA && usedB && usedC);
				showUnusedEvents = EditorGUILayout.Foldout(showUnusedEvents, "Show Unused Events");
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Separator();

			if (usedA == true || showUnusedEvents == true)
			{
				Draw("onSelectableDown");
			}

			if (usedB == true || showUnusedEvents == true)
			{
				Draw("onSelectableUpdate");
			}

			if (usedC == true || showUnusedEvents == true)
			{
				Draw("onSelectableUp");
			}
		}
	}
}
#endif
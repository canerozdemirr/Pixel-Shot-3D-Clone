using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
	/// <summary>This script will trigger events based on the selection of LeanSelectables.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanAnySelected")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Any Selected")]
	public class LeanAnySelected : MonoBehaviour
	{
		/// <summary>Set to true when at least one LeanSelectable is selected.</summary>
		[Tooltip("Set to true when at least one LeanSelectable is selected.")]
		public bool AnySelected;

		/// <summary>Called when at least one LeanSelectable is selected.</summary>
		public UnityEvent OnFirstSelected { get { if (onFirstSelected == null) onFirstSelected = new UnityEvent(); return onFirstSelected; } } [UnityEngine.Serialization.FormerlySerializedAs("OnSelectedStart")] [SerializeField] private UnityEvent onFirstSelected;

		/// <summary>Called when all LeanSelectables are deselected.</summary>
		public UnityEvent OnLastDeselected { get { if (onLastDeselected == null) onLastDeselected = new UnityEvent(); return onLastDeselected; } } [UnityEngine.Serialization.FormerlySerializedAs("OnSelectedEnd")] [SerializeField] private UnityEvent onLastDeselected;

		protected virtual void Update()
		{
			if (AnySelected == true)
			{
				if (LeanSelectable.IsSelectedCount == 0)
				{
					AnySelected = false;

					if (onLastDeselected != null)
					{
						onLastDeselected.Invoke();
					}
				}
			}
			else
			{
				if (LeanSelectable.IsSelectedCount > 0)
				{
					AnySelected = true;

					if (onFirstSelected != null)
					{
						onFirstSelected.Invoke();
					}
				}
			}
		}
	}
}
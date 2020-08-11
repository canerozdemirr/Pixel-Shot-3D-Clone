using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to translate the specified GameObject when you call methods like <b>TranslateA</b>, which can be done from events.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanManualTranslate")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Manual Translate")]
	public class LeanManualTranslate : MonoBehaviour
	{
		/// <summary>If you want this component to work on a different GameObject, then specify it here. This can be used to improve organization if your GameObject already has many components.</summary>
		[Tooltip("If you want this component to work on a different GameObject, then specify it here. This can be used to improve organization if your GameObject already has many components.")]
		public GameObject Target;

		/// <summary>This allows you to set the coordinate space the translation will use.</summary>
		[Tooltip("This allows you to set the coordinate space the translation will use.")]
		public Space Space;

		/// <summary>The first translation direction, used when calling TranslateA or TranslateAB.</summary>
		[Tooltip("The first translation direction, used when calling TranslateA or TranslateAB.")]
		public Vector3 DirectionA = Vector3.right;

		/// <summary>The first second direction, used when calling TranslateB or TranslateAB.</summary>
		[Tooltip("The first second direction, used when calling TranslateB or TranslateAB.")]
		public Vector3 DirectionB = Vector3.up;

		[Space]

		/// <summary>The translation distance is multiplied by this.
		/// 1 = Normal distance.
		/// 2 = Double distance.</summary>
		[Tooltip("The translation distance is multiplied by this.\n\n1 = Normal distance.\n\n2 = Double distance.")]
		public float Multiplier = 1.0f;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = 10.0f;

		/// <summary>If you enable this then the translation will be multiplied by Time.deltaTime. This allows you to maintain framerate independent movement.</summary>
		[Tooltip("If you enable this then the translation will be multiplied by Time.deltaTime. This allows you to maintain framerate independent movement.")]
		public bool ScaleByTime;

		[HideInInspector]
		[SerializeField]
		private Vector3 remainingDelta;

		/// <summary>This method allows you to set this value from inspector events.</summary>
		public void SetTarget(GameObject newTarget)
		{
			Target = newTarget;
		}

		/// <summary>This method allows you to set this value from inspector events.</summary>
		public void SetSpace(Space newSpace)
		{
			Space = newSpace;
		}

		/// <summary>This method allows you to set this value from inspector events.</summary>
		public void SetDirectionA(Vector3 newDirectionA)
		{
			DirectionA = newDirectionA;
		}

		/// <summary>This method allows you to set this value from inspector events.</summary>
		public void SetDirectionB(Vector3 newDirectionB)
		{
			DirectionB = newDirectionB;
		}

		/// <summary>This method allows you to set this value from inspector events.</summary>
		public void SetMultiplier(float newMultiplier)
		{
			Multiplier = newMultiplier;
		}

		/// <summary>This method allows you to set this value from inspector events.</summary>
		public void SetDampening(float newDampening)
		{
			Dampening = newDampening;
		}

		/// <summary>This method allows you to set this value from inspector events.</summary>
		public void SetScaleByTime(bool newScaleByTime)
		{
			ScaleByTime = newScaleByTime;
		}

		/// <summary>This method allows you to translate along DirectionA, with the specified multiplier.</summary>
		public void TranslateA(float magnitude)
		{
			Translate(DirectionA * magnitude);
		}

		/// <summary>This method allows you to translate along DirectionB, with the specified multiplier.</summary>
		public void TranslateB(float magnitude)
		{
			Translate(DirectionB * magnitude);
		}

		/// <summary>This method allows you to translate along DirectionA and DirectionB, with the specified multipliers.</summary>
		public void TranslateAB(Vector2 magnitude)
		{
			Translate(DirectionA * magnitude.x + DirectionB * magnitude.y);
		}

		/// <summary>This method allows you to translate along the specified vector in local space.</summary>
		public void Translate(Vector3 vector)
		{
			if (Space == Space.Self)
			{
				var finalTransform = Target != null ? Target.transform : transform;

				vector = finalTransform.TransformVector(vector);
			}

			TranslateWorld(vector);
		}

		/// <summary>This method allows you to translate along the specified vector in world space.</summary>
		public void TranslateWorld(Vector3 vector)
		{
			if (ScaleByTime == true)
			{
				vector *= Time.deltaTime;
			}

			remainingDelta += vector * Multiplier;
		}

		protected virtual void Update()
		{
			var finalTransform = Target != null ? Target.transform : transform;
			var factor         = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);
			var newDelta       = Vector3.Lerp(remainingDelta, Vector3.zero, factor);

			finalTransform.position += remainingDelta - newDelta;

			remainingDelta = newDelta;
		}
	}
}
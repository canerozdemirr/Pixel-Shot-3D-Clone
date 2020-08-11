using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Lean.Touch
{
	/// <summary>This component allows you to detect </summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanShapeDetector")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Shape Detector")]
	public class LeanShapeDetector : MonoBehaviour
	{
		[System.Serializable] public class FingerDataEvent : UnityEvent<FingerData> {}

		/// <summary>This stores data about a finger that's currently tracing the shape.</summary>
		[System.Serializable]
		public class FingerData : LeanFingerData
		{
			public int           Index; // This is the Shape.Points index that this finger is starting from.
			public bool          Reverse; // Is this finger drawing the shape in reverse?
			public List<Vector2> Points = new List<Vector2>(); // This stores the current shape this finger has drawn.
			public List<Vector2> Buffer = new List<Vector2>(); // This stores the currently buffered points that could be used to define the next edge if you continue drawing enough.

			public Vector2 EndPoint
			{
				get
				{
					return Points[Points.Count - 1];
				}
			}

			public Vector2 EndBuffer
			{
				get
				{
					return Buffer[Buffer.Count - 1];
				}
			}

			public Vector2 EndVector
			{
				get
				{
					return Points[Points.Count - 1] - Points[Points.Count - 2];
				}
			}
		}

		public enum DirectionType
		{
			Forward,
			Backward,
			ForwardAndBackward
		}

		/// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
		public LeanFingerFilter Use = new LeanFingerFilter(true);

		[Space]

		[Tooltip("The shape we want to detect.")]
		public LeanShape Shape;

		[Tooltip("If the finger moves this many scaled pixels, the detector will update.")]
		public float StepThreshold = 1.0f;

		[Tooltip("This allows you to specify the minimum length of each edge in the detected shape in scaled pixels.")]
		public float MinimumEdgeLength = 5.0f;

		//[Tooltip("If the finger traces an edge that exceeds this length scale, then the finger will be discarded.")]
		//[Range(1.01f, 10.0f)]
		//public float LengthThreshold = 1.5f;

		[Tooltip("If the finger strays in a direction this far from the expected heading, then the finger will be discarded.")]
		[Range(0.5f, 0.99f)]
		public float DirectionPrecision = 0.85f;

		[Tooltip("If you want to allow partial shape matches, then specify the minimum amount of edges that must be matched in the shape.")]
		public int MinimumPoints = -1;

		[Tooltip("Which direction should the shape be checked using?")]
		public DirectionType Direction = DirectionType.ForwardAndBackward;

		/// <summary>If the finger goes up and it has traced the specified shape, this event will be invoked with the finger data.</summary>
		public FingerDataEvent OnDetected { get { if (onDetected == null) onDetected = new FingerDataEvent(); return onDetected; } } [SerializeField] private FingerDataEvent onDetected;

		// This stores the currently active finger data.
		private List<FingerData> fingerDatas;

		// Pool the FingerData so we reduce GC alloc!
		private static Stack<FingerData> fingerDataPool = new Stack<FingerData>();

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
		public void AddFinger(LeanFinger finger)
		{
			if (Shape != null && Shape.Points != null && Shape.Points.Count > 1)
			{
				// Discard any existing FingerData for this finger
				LeanFingerData.Remove(fingerDatas, finger, fingerDataPool);

				if (Shape.ConnectEnds == true)
				{
					// If the shape is connected then begin detection from every point, because we don't know where the user has begun drawing from yet.
					for (var i = Shape.Points.Count - 1; i >= 0; i--)
					{
						AddFinger(finger, i);
					}
				}
				else
				{
					AddFinger(finger, 0);
				}
			}
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
		public void RemoveFinger(LeanFinger finger)
		{
			LeanFingerData.Remove(fingerDatas, finger, fingerDataPool);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
		public void RemoveAllFingers()
		{
			LeanFingerData.RemoveAll(fingerDatas, fingerDataPool);
		}

		private void AddFinger(LeanFinger finger, int index)
		{
			if (Direction == DirectionType.ForwardAndBackward || Direction == DirectionType.Forward)
			{
				AddFinger(finger, index, false);
			}

			if (Direction == DirectionType.ForwardAndBackward || Direction == DirectionType.Backward)
			{
				AddFinger(finger, index, true);
			}
		}

		private void AddFinger(LeanFinger finger, int index, bool reverse)
		{
			var fingerData = LeanFingerData.FindOrCreate(ref fingerDatas, null); // We want to be able to store multiple links per finger, so pass null initially

			fingerData.Finger  = finger;
			fingerData.Index   = index;
			fingerData.Reverse = reverse;

			fingerData.Buffer.Clear();
			fingerData.Points.Clear();

			fingerData.Buffer.Add(finger.ScreenPosition);
			fingerData.Points.Add(finger.ScreenPosition);
			fingerData.Points.Add(finger.ScreenPosition);
		}

		protected virtual void OnEnable()
		{
			LeanTouch.OnFingerUp += HandleFingerUp;
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerUp -= HandleFingerUp;
		}

		protected virtual void Update()
		{
			var fingers = Use.GetFingers(true);

			for (var i = fingers.Count - 1; i >= 0; i--)
			{
				var finger = fingers[i];

				if (finger.Down == true)
				{
					AddFinger(finger);
				}
			}

			if (Shape != null && fingerDatas != null)
			{
				for (var i = fingerDatas.Count - 1; i >= 0; i--)
				{
					var fingerData = fingerDatas[i];
					var fingerTip  = fingerData.Points[fingerData.Points.Count - 1];

					if (TryExtend(fingerData, fingerData.Finger.ScreenPosition) == false)
					{
						fingerDatas.RemoveAt(i);

						fingerDataPool.Push(fingerData);
					}
				}
			}
		}

		private bool TryExtend(FingerData fingerData, Vector2 newPoint)
		{
			fingerData.Points[fingerData.Points.Count - 1] = newPoint;

			if (Vector2.Distance(fingerData.EndBuffer, newPoint) >= StepThreshold)
			{
				fingerData.Buffer.Add(newPoint);

				var lineA = fingerData.Points[fingerData.Points.Count - 2];
				var lineB = fingerData.Points[fingerData.Points.Count - 1];

				if (BufferOutOfBounds(lineA, lineB, fingerData, 0, fingerData.Buffer.Count) == true)
				{
					return false;
				}
				else
				{
					TryPivot(fingerData);
				}
			}

			return true;
		}

		private bool BufferOutOfBounds(Vector2 lineA, Vector2 lineB, FingerData fingerData, int indexA, int indexB)
		{
			for (var i = indexA; i < indexB; i++)
			{
				if (Distance(lineA, lineB, fingerData.Buffer[i]) > 20.0f)
				{
					return true;
				}
			}

			return false;
		}

		private bool TryPivot(FingerData fingerData)
		{
			if (fingerData.Buffer.Count > 2)
			{
				var first           = fingerData.Buffer[0];
				var last            = fingerData.Buffer[fingerData.Buffer.Count - 1];
				var bestScore       = -1.0f;
				//var bestScoreA      = -1.0f;
				//var bestScoreB      = -1.0f;
				var bestIndex       = -1;
				var bestMiddle      = default(Vector2);
				var shapePointA     = Shape.GetPoint(fingerData.Index + fingerData.Points.Count - 2, fingerData.Reverse);
				var shapePointB     = Shape.GetPoint(fingerData.Index + fingerData.Points.Count - 1, fingerData.Reverse);
				var shapePointC     = Shape.GetPoint(fingerData.Index + fingerData.Points.Count - 0, fingerData.Reverse);
				var shapeDirectionA = (shapePointB - shapePointA).normalized;
				var shapeDirectionB = (shapePointC - shapePointB).normalized;

				for (var i = fingerData.Buffer.Count - 2; i >= 1; i--)
				{
					var middle     = fingerData.Buffer[i];

					if (Vector2.Distance(first, middle) >= MinimumEdgeLength && Vector2.Distance(middle, last) >= MinimumEdgeLength)
					{
						var directionA = (middle - first).normalized;
						var directionB = (last - middle).normalized;
						var scoreA     = Mathf.Max(0.0f, Vector2.Dot(directionA, shapeDirectionA));
						var scoreB     = Mathf.Max(0.0f, Vector2.Dot(directionB, shapeDirectionB));
						var score      = scoreA * scoreB;

						if (scoreA > DirectionPrecision && scoreB > DirectionPrecision && score > bestScore)
						{
							//bestScoreA = scoreA;
							//bestScoreB = scoreB;
							bestScore  = score;
							bestIndex  = i;
							bestMiddle = middle;
						}
					}
				}

				if (bestIndex > 0)
				{
					if (BufferOutOfBounds(first, bestMiddle, fingerData, 0, bestIndex) == false)
					{
						if (BufferOutOfBounds(bestMiddle, last, fingerData, bestIndex, fingerData.Buffer.Count) == false)
						{
							fingerData.Points.Insert(fingerData.Points.Count - 1, bestMiddle);

							fingerData.Buffer.Clear();

							fingerData.Buffer.Add(last);

							return true;
						}
					}
				}
			}

			return false;
		}

		private float Distance(Vector2 lineA, Vector2 lineB, Vector2 point)
		{
			var v  = lineB - lineA;
			var w  = point - lineA;
			var c1 = Vector2.Dot(w,v); if (c1 <= 0.0f) return Vector2.Distance(point, lineA);
			var c2 = Vector2.Dot(v,v); if (c2 <= c1) return Vector2.Distance(point, lineB);
			var b  = c1 / c2;
			var Pb = lineA + b * v;

			return Vector2.Distance(point, Pb);
		}

		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			if (fingerDatas != null)
			{
				foreach (var fingerData in fingerDatas)
				{
					Gizmos.color = Color.black;

					for (var i = 0; i < fingerData.Buffer.Count - 1; i++)
					{
						Gizmos.DrawLine(fingerData.Buffer[i], fingerData.Buffer[i + 1]);
					}

					Gizmos.color = Color.white;

					for (var i = 0; i < fingerData.Points.Count - 1; i++)
					{
						Gizmos.DrawLine(fingerData.Points[i], fingerData.Points[i + 1]);
					}
				}
			}
		}

		private void HandleFingerUp(LeanFinger finger)
		{
			var fingerData = LeanFingerData.Find(fingerDatas, finger);

			if (fingerData != null && Shape != null && Shape.Points != null)
			{
				//Debug.Log("count " + fingerData.Points.Count);
				var minimum = Shape.ConnectEnds == true ? Shape.Points.Count + 1 : Shape.Points.Count;

				if (MinimumPoints > 0 && MinimumPoints < minimum)
				{
					minimum = MinimumPoints;
				}

				if (fingerData.Points.Count >= minimum)
				{
					if (OnDetected != null)
					{
						OnDetected.Invoke(fingerData);
					}
				}
			}

			LeanFingerData.Remove(fingerDatas, finger, fingerDataPool);
		}

		private float VectorsAlignment(Vector2 fingerVector, Vector2 shapeVector)
		{
			if (fingerVector != Vector2.zero && shapeVector != Vector2.zero)
			{
				var dot = Vector2.Dot(fingerVector.normalized, shapeVector.normalized);

				if (dot > 0.0f && dot >= DirectionPrecision)
				{
					return dot;
				}
			}

			return 0.0f;
		}

		private float CalculateScale(float a, float b)
		{
			return b != 0.0f ? a / b : 0.0f;
		}
	}
}
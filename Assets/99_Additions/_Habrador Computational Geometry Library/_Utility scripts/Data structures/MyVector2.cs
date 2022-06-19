using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
namespace Habrador_Computational_Geometry
{
	//Unity loves to automatically cast beween Vector2 and Vector3
	//Because theres no way to stop it, its better to use a custom struct 
	[System.Serializable]
	public struct Vector2
	{
		public float x;
		public float y;

		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}



		//
		// To make vector operations easier
		//

		//Test if this vector is approximately the same as another vector
		public bool Equals(Vector2 other)
		{
			//Using Mathf.Approximately() is not accurate enough
			//Using Mathf.Abs is slow because Abs involves a root

			float xDiff = this.x - other.x;
			float yDiff = this.y - other.y;

			float e = MathUtility.EPSILON;

			//If all of the differences are around 0
			if (
				xDiff < e && xDiff > -e && 
				yDiff < e && yDiff > -e)
			{
				return true;
			}
			else
			{
				return false;
			}
		}


		//Vector operations
		public static float Dot(Vector2 a, Vector2 b)
		{
			float dotProduct = (a.x * b.x) + (a.y * b.y);

			return dotProduct;
		}

		// Length of vector a: ||a||
		public static float Magnitude(Vector2 a)
		{
			float magnitude = Mathf.Sqrt(SqrMagnitude(a));

			return magnitude;
		}

		public static float SqrMagnitude(Vector2 a)
		{
			float sqrMagnitude = (a.x * a.x) + (a.y * a.y);

			return sqrMagnitude;
		}

		public static float Distance(Vector2 a, Vector2 b)
		{
			float distance = Magnitude(a - b);

			return distance;
		}

		public static float SqrDistance(Vector2 a, Vector2 b)
		{
			float distance = SqrMagnitude(a - b);

			return distance;
		}

		public static Vector2 Normalize(Vector2 v)
		{
			float v_magnitude = Magnitude(v);

			Vector2 v_normalized = new Vector2(v.x / v_magnitude, v.y / v_magnitude);

			return v_normalized;
		}


		//Operator overloads
		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x + b.x, a.y + b.y);
		}

		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x - b.x, a.y - b.y);
		}

		public static Vector2 operator *(Vector2 a, float b)
		{
			return new Vector2(a.x * b, a.y * b);
		}

		public static Vector2 operator *(float b, Vector2 a)
		{
			return new Vector2(a.x * b, a.y * b);
		}

		public static Vector2 operator -(Vector2 a)
		{
			return a * -1f;
		}
	}
}
*/
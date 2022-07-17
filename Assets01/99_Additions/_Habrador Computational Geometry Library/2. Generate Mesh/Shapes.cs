using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry.MeshAlgorithms
{
	//Generates geometric shapes, such as arrows, circles, lines, etc
	public static class Shapes
	{
		//
		// Circle
		//

		//Filled circle
		public static HashSet<Triangle2> Circle(Vector2 center, float radius, int resolution)
		{
			if (resolution < 3)
			{
				Debug.Log("You cant make a circle with less than 3 points! FailFish");
				
				return null;
			}

			//Generate the point on the circle edge
			List<Vector2> pointsOnCircleEdge = GenerateCirclePoints(center, radius, resolution);

			//Triangulate
			HashSet<Triangle2> circleTriangles = _TriangulatePoints.PointsOnConvexHull(pointsOnCircleEdge, center);

			return circleTriangles;
		}


		//Circle with a hole in it
		public static HashSet<Triangle2> CircleHollow(Vector2 center, float innerRadius, int resolution, float width)
		{
			if (resolution < 3)
			{
				Debug.Log("You cant make a circle with less than 3 points! FailFish");

				return null;
			}

			//We will later triangulate the circle by using the connected-lines algorithm
			//where width is measured has half-width from the middle of the edge
			//so the inner radius has to be modified with the half-width
			float middleRadius = innerRadius + width * 0.5f;

			//Generate the point on the circles edge between the inner radius and the outer radius
			List<Vector2> pointsOnCircleEdge = GenerateCirclePoints(center, middleRadius, resolution);

			//Triangulate
			HashSet<Triangle2> circleTriangles = ConnectedLineSegments(pointsOnCircleEdge, width, isConnected: true);

			return circleTriangles;
		}


		//Help method to generate points on a circle edge
		private static List<Vector2> GenerateCirclePoints(Vector2 center, float radius, int resolution)
		{
			//Generate the point on the circle edge
			//then we just triangulate with one of our convex polygon algorithm
			List<Vector2> pointsOnCircleEdge = new List<Vector2>();

			//The ange between each point on the edge
			float angleBetween = 360f / (float)(resolution);

			float angle = 0f;

			for (int i = 0; i < resolution; i++)
			{
				float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
				float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

				Vector2 p = center + new Vector2(x, y);

				pointsOnCircleEdge.Add(p);

				//Minus to get correct orientation when displaying the mesh
				angle -= angleBetween;
			}


			return pointsOnCircleEdge;
		}



		//
		// Line segment
		//
		public static HashSet<Triangle2> LineSegment(Vector2 p1, Vector2 p2, float width)
		{
			Vector2 lineDir = p2 - p1;

			Vector2 lineNormal = (new Vector2(lineDir.y, -lineDir.x)).normalized;

			//Bake in the width in the normal
			lineNormal *= width * 0.5f;

			Vector2 p1_T = p1 + lineNormal;
			Vector2 p2_T = p2 + lineNormal;

			Vector2 p1_B = p1 - lineNormal;
			Vector2 p2_B = p2 - lineNormal;

			HashSet<Triangle2> lineTriangles = LineSegment(p1_T, p1_B, p2_T, p2_B);

			return lineTriangles;
		}


		//Generate two triangles if we know the corners of the rectangle
		public static HashSet<Triangle2> LineSegment(Vector2 p1_T, Vector2 p1_B, Vector2 p2_T, Vector2 p2_B)
		{
			HashSet<Triangle2> lineTriangles = new HashSet<Triangle2>();

			//Create the triangles
			Triangle2 t1 = new Triangle2(p1_T, p1_B, p2_T);
			Triangle2 t2 = new Triangle2(p1_B, p2_B, p2_T);

			lineTriangles.Add(t1);
			lineTriangles.Add(t2);

			return lineTriangles;
		}



		//
		// Connected line segments
		//
		//isConnected means if the end points are connected to form a loop
		public static HashSet<Triangle2> ConnectedLineSegments(List<Vector2> points, float width, bool isConnected)
		{
			if (points != null && points.Count < 2)
			{
				Debug.Log("Cant form a line with fewer than two points");

				return null;
			}



			//Generate the triangles
			HashSet<Triangle2> lineTriangles = new HashSet<Triangle2>();

			//If the lines are connected we need to do plane-plane intersection to find the 
			//coordinate where the lines meet at each point, or the line segments will
			//not get the same size
			//(There might be a better way to do it than with plane-plane intersection)
			List<Vector2> topCoordinate = new List<Vector2>();
			List<Vector2> bottomCoordinate = new List<Vector2>();

			float halfWidth = width * 0.5f;

			for (int i = 0; i < points.Count; i++)
			{
				Vector2 p = points[i];
			
				//First point = special case if the lines are not connected
				if (i == 0 && !isConnected)
				{
					Vector2 lineDir = points[1] - points[0];

					Vector2 lineNormal = (new Vector2(lineDir.y, -lineDir.x)).normalized;

					topCoordinate.Add(p + lineNormal * halfWidth);

					bottomCoordinate.Add(p - lineNormal * halfWidth);
				}
				//Last point = special case if the lines are not connected
				else if (i == points.Count - 1 && !isConnected)
				{
					Vector2 lineDir = p - points[points.Count - 2];

					Vector2 lineNormal = (new Vector2(lineDir.y, -lineDir.x)).normalized;

					topCoordinate.Add(p + lineNormal * halfWidth);

					bottomCoordinate.Add(p - lineNormal * halfWidth);
				}
				else
				{
					//Now we need to find the intersection points between the top line and the bottom line
					Vector2 p_before = points[MathUtility.ClampListIndex(i - 1, points.Count)];

					Vector2 p_after = points[MathUtility.ClampListIndex(i + 1, points.Count)];

					Vector2 pTop = GetIntersectionPoint(p_before, p, p_after, halfWidth, isTopPoint: true);
					
					Vector2 pBottom = GetIntersectionPoint(p_before, p, p_after, halfWidth, isTopPoint: false);



					topCoordinate.Add(pTop);

					bottomCoordinate.Add(pBottom);
				}
			}

			//Debug.Log();

			for (int i = 0; i < points.Count; i++)
			{
				//Skip the first point if it is not connected to the last point
				if (i == 0 && !isConnected)
				{
					continue;
				}

				int i_minus_one = MathUtility.ClampListIndex(i - 1, points.Count);

				Vector2 p1_T = topCoordinate[i_minus_one];
				Vector2 p1_B = bottomCoordinate[i_minus_one];

				Vector2 p2_T = topCoordinate[i];
				Vector2 p2_B = bottomCoordinate[i];

				HashSet<Triangle2> triangles = LineSegment(p1_T, p1_B, p2_T, p2_B);

				foreach (Triangle2 t in triangles)
				{
					lineTriangles.Add(t);
				}
			}

			//Debug.Log(lineTriangles.Count);

			return lineTriangles;
		}


		//Help method to calculate the intersection point between two planes offset in normal direction by a width
		private static Vector2 GetIntersectionPoint(Vector2 a, Vector2 b, Vector2 c, float halfWidth, bool isTopPoint)
		{
			//Direction of the lines going to and from point b
			Vector2 beforeDir = (b - a).normalized;

			Vector2 afterDir = (c - b).normalized;

			Vector2 beforeNormal = GetNormal(a, b);

			Vector2 afterNormal = GetNormal(b, c);

			//Compare the normals!

			//normalDirFactor is used to determine if we want to top point (same direction as normal)
			float normalDirFactor = isTopPoint ? 1f : -1f;

			//If they are the same it means we have a straight line and thus we cant do plane-plane intersection
			//if (beforeNormal.Equals(afterNormal))
			//When comparing the normals, we cant use the regular small value because then
			//the line width goes to infinity when doing plane-plane intersection
			float dot = Vector2.Dot(beforeNormal, afterNormal);

			//Dot is 1 if the point in the same dir and -1 if the point in the opposite dir
			float one = 1f - 0.01f;

			if (dot > one || dot < -one)
			{
				Vector2 averageNormal = ((afterNormal + beforeNormal) * 0.5f).normalized;

				Vector2 intersectionPoint = b + averageNormal * halfWidth * normalDirFactor;

				return intersectionPoint;
			}
			else 
			{
				//Now we can calculate where the plane starts
				Vector2 beforePlanePos = b + beforeNormal * halfWidth * normalDirFactor;

				Vector2 afterPlanePos = b + afterNormal * halfWidth * normalDirFactor;

				Plane2 planeBefore = new Plane2(beforePlanePos, beforeNormal);

				Plane2 planeAfter = new Plane2(afterPlanePos, afterNormal);

				//Calculate the intersection point
				//We know they are intersecting, so we don't need to test that
				Vector2 intersectionPoint = _Intersections.GetPlanePlaneIntersectionPoint(planeBefore, planeAfter);

				return intersectionPoint;
			}
		}


		//Help method to calculate the normal from two points
		private static Vector2 GetNormal(Vector2 a, Vector2 b)
		{
			Vector2 lineDir = b - a;

			//Flip x with y and set one to negative to get the normal
			Vector2 normal = (new Vector2(lineDir.y, -lineDir.x)).normalized;

			return normal;
		}


		//Help method to calculate the average normal of two line segments
		private static Vector2 GetAverageNormal(Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 normal_1 = GetNormal(a, b);
			Vector2 normal_2 = GetNormal(b, c);

			Vector2 averageNormal = (normal_1 + normal_2) * 0.5f;

			averageNormal = averageNormal.normalized;

			return averageNormal;
		}



		//
		// Arrow
		//
		public static HashSet<Triangle2> Arrow(Vector2 p1, Vector2 p2, float lineWidth, float arrowSize)
		{
			HashSet<Triangle2> arrowTriangles = new HashSet<Triangle2>();

			//An arrow consists of two parts: the pointy part and the rectangular part

			//First we have to see if we can fit the parts
			Vector2 lineDir = p2 - p1;

			float lineLength = lineDir.magnitude;

			if (lineLength < arrowSize)
			{
				Debug.Log("Cant make arrow because line is too short");

				return null;
			}


			//Make the arrow tip
			Vector2 lineDirNormalized = lineDir.normalized;

			Vector2 arrowBottom = p2 - lineDirNormalized * arrowSize;

			Vector2 lineNormal = (new Vector2(lineDirNormalized.y, -lineDirNormalized.x)).normalized;

			Vector2 arrowBottom_R = arrowBottom + lineNormal * arrowSize * 0.5f;
			Vector2 arrowBottom_L = arrowBottom - lineNormal * arrowSize * 0.5f;

			Triangle2 arrowTipTriangle = new Triangle2(p2 , arrowBottom_R, arrowBottom_L);

			arrowTriangles.Add(arrowTipTriangle);


			//Make the arrow rectangle
			float halfWidth = lineWidth * 0.5f;

			Vector2 p1_T = p1 + lineNormal * halfWidth;
			Vector2 p1_B = p1 - lineNormal * halfWidth;

			Vector2 p2_T = arrowBottom + lineNormal * halfWidth;
			Vector2 p2_B = arrowBottom - lineNormal * halfWidth;

			HashSet<Triangle2> rectangle = LineSegment(p1_T, p1_B, p2_T, p2_B);

			foreach (Triangle2 t in rectangle)
			{
				arrowTriangles.Add(t);
			}

			return arrowTriangles;
		}
	}
}

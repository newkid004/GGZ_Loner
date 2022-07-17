using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
	//Calculate the intersection of two polygons
	//https://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman_algorithm
	///Requires that the clipping polygon (the polygon we want to remove from the other polygon) is convex
	public static class SutherlandHodgman
	{
		//Sometimes its more efficient to calculate the planes once before we call the method
		//if we want to cut several polygons with the same planes
		public static List<Vector2> ClipPolygon(List<Vector2> poly, List<Vector2> clipPoly)
		{
			//Calculate the clipping planes
			List<Plane2> clippingPlanes = GetClippingPlanes(clipPoly);

			List<Vector2> vertices = ClipPolygon(poly, clippingPlanes);

			return vertices;
		}



		//Assumes the polygons are oriented counter clockwise
		//poly is the polygon we want to cut
		//Assumes the polygon we want to remove from the other polygon is convex, so clipPolygon has to be convex
		//We will end up with the intersection of the polygons
		public static List<Vector2> ClipPolygon(List<Vector2> poly, List<Plane2> clippingPlanes)
		{
			//Clone the vertices because we will remove vertices from this list
			List<Vector2> vertices = new List<Vector2>(poly);

			//Save the new vertices temporarily in this list before transfering them to vertices
			List<Vector2> vertices_tmp = new List<Vector2>();

			//Clip the polygon
			for (int i = 0; i < clippingPlanes.Count; i++)
			{
				Plane2 plane = clippingPlanes[i];

				for (int j = 0; j < vertices.Count; j++)
				{
					int jPlusOne = MathUtility.ClampListIndex(j + 1, vertices.Count);

					Vector2 v1 = vertices[j];
					Vector2 v2 = vertices[jPlusOne];

					//Calculate the distance to the plane from each vertex
					//This is how we will know if they are inside or outside
					//If they are inside, the distance is positive, which is why the planes normals have to be oriented to the inside
					float dist_to_v1 = _Geometry.GetSignedDistanceFromPointToPlane(v1, plane);
					float dist_to_v2 = _Geometry.GetSignedDistanceFromPointToPlane(v2, plane);

					//TODO: What will happen if they are exactly 0? Should maybe use a tolerance of 0.001

					//Case 1. Both are outside (= to the right), do nothing 

					//Case 2. Both are inside (= to the left), save v2
					if (dist_to_v1 >= 0f && dist_to_v2 >= 0f)
					{
						vertices_tmp.Add(v2);
					}
					//Case 3. Outside -> Inside, save intersection point and v2
					else if (dist_to_v1 < 0f && dist_to_v2 >= 0f)
					{
						Vector2 rayDir = (v2 - v1).normalized;

						Ray2 ray = new Ray2(v1, rayDir);

						Vector2 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

						vertices_tmp.Add(intersectionPoint);

						vertices_tmp.Add(v2);
					}
					//Case 4. Inside -> Outside, save intersection point
					else if (dist_to_v1 >= 0f && dist_to_v2 < 0f)
					{
						Vector2 rayDir = (v2 - v1).normalized;

						Ray2 ray = new Ray2(v1, rayDir);

						Vector2 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

						vertices_tmp.Add(intersectionPoint);
					}
				}

				//Add the new vertices to the list of vertices
				vertices.Clear();

				vertices.AddRange(vertices_tmp);

				vertices_tmp.Clear();
			}

			return vertices;
		}



		//Get the clipping planes
		public static List<Plane2> GetClippingPlanes(List<Vector2> clipPoly)
		{
			//Calculate the clipping planes
			List<Plane2> clippingPlanes = new List<Plane2>();

			for (int i = 0; i < clipPoly.Count; i++)
			{
				int iPlusOne = MathUtility.ClampListIndex(i + 1, clipPoly.Count);

				Vector2 v1 = clipPoly[i];
				Vector2 v2 = clipPoly[iPlusOne];

				//Doesnt have to be center but easier to debug
				Vector2 planePos = (v1 + v2) * 0.5f;

				Vector2 planeDir = v2 - v1;

				//Should point inwards - do we need to normalize???
				Vector2 planeNormal = new Vector2(-planeDir.y, planeDir.x);

				//Gizmos.DrawRay(planePos, planeNormal.normalized * 0.1f);

				clippingPlanes.Add(new Plane2(planePos, planeNormal));
			}

			return clippingPlanes;
		}

		//Sometimes its more efficient to calculate the planes once before we call the method
		//if we want to cut several polygons with the same planes
		public static List<Vector3> ClipPolygon(List<Vector3> poly, List<Vector3> clipPoly)
		{
			//Calculate the clipping planes
			List<Plane2> clippingPlanes = GetClippingPlanes(clipPoly);

			List<Vector3> vertices = ClipPolygon(poly, clippingPlanes);

			return vertices;
		}



		//Assumes the polygons are oriented counter clockwise
		//poly is the polygon we want to cut
		//Assumes the polygon we want to remove from the other polygon is convex, so clipPolygon has to be convex
		//We will end up with the intersection of the polygons
		public static List<Vector3> ClipPolygon(List<Vector3> poly, List<Plane2> clippingPlanes)
		{
			//Clone the vertices because we will remove vertices from this list
			List<Vector3> vertices = new List<Vector3>(poly);

			//Save the new vertices temporarily in this list before transfering them to vertices
			List<Vector3> vertices_tmp = new List<Vector3>();

			//Clip the polygon
			for (int i = 0; i < clippingPlanes.Count; i++)
			{
				Plane2 plane = clippingPlanes[i];

				for (int j = 0; j < vertices.Count; j++)
				{
					int jPlusOne = MathUtility.ClampListIndex(j + 1, vertices.Count);

					Vector3 v1 = vertices[j];
					Vector3 v2 = vertices[jPlusOne];

					//Calculate the distance to the plane from each vertex
					//This is how we will know if they are inside or outside
					//If they are inside, the distance is positive, which is why the planes normals have to be oriented to the inside
					float dist_to_v1 = _Geometry.GetSignedDistanceFromPointToPlane(v1, plane);
					float dist_to_v2 = _Geometry.GetSignedDistanceFromPointToPlane(v2, plane);

					//TODO: What will happen if they are exactly 0? Should maybe use a tolerance of 0.001

					//Case 1. Both are outside (= to the right), do nothing 

					//Case 2. Both are inside (= to the left), save v2
					if (dist_to_v1 >= 0f && dist_to_v2 >= 0f)
					{
						vertices_tmp.Add(v2);
					}
					//Case 3. Outside -> Inside, save intersection point and v2
					else if (dist_to_v1 < 0f && dist_to_v2 >= 0f)
					{
						Vector3 rayDir = (v2 - v1).normalized;

						Ray2 ray = new Ray2(v1, rayDir);

						Vector3 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

						vertices_tmp.Add(intersectionPoint);

						vertices_tmp.Add(v2);
					}
					//Case 4. Inside -> Outside, save intersection point
					else if (dist_to_v1 >= 0f && dist_to_v2 < 0f)
					{
						Vector3 rayDir = (v2 - v1).normalized;

						Ray2 ray = new Ray2(v1, rayDir);

						Vector3 intersectionPoint = _Intersections.GetRayPlaneIntersectionPoint(plane, ray);

						vertices_tmp.Add(intersectionPoint);
					}
				}

				//Add the new vertices to the list of vertices
				vertices.Clear();

				vertices.AddRange(vertices_tmp);

				vertices_tmp.Clear();
			}

			return vertices;
		}



		//Get the clipping planes
		public static List<Plane2> GetClippingPlanes(List<Vector3> clipPoly)
		{
			//Calculate the clipping planes
			List<Plane2> clippingPlanes = new List<Plane2>();

			for (int i = 0; i < clipPoly.Count; i++)
			{
				int iPlusOne = MathUtility.ClampListIndex(i + 1, clipPoly.Count);

				Vector3 v1 = clipPoly[i];
				Vector3 v2 = clipPoly[iPlusOne];

				//Doesnt have to be center but easier to debug
				Vector3 planePos = (v1 + v2) * 0.5f;

				Vector3 planeDir = v2 - v1;

				//Should point inwards - do we need to normalize???
				Vector3 planeNormal = new Vector3(-planeDir.y, planeDir.x);

				//Gizmos.DrawRay(planePos, planeNormal.normalized * 0.1f);

				clippingPlanes.Add(new Plane2(planePos, planeNormal));
			}

			return clippingPlanes;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
	//3D
	public struct Plane3
	{
		public Vector3 pos;

		public Vector3 normal;


		public Plane3(Vector3 pos, Vector3 normal)
		{
			this.pos = pos;

			this.normal = normal;
		}


		//p1-p2-p3 should be ordered clock-wise
		public Plane3(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			this.pos = p1;

			Vector3 normal = _Geometry.CalculateTriangleNormal(p1, p2, p3);

			this.normal = normal;
		}
	}



	//Oriented plane which is needed if we want to transform between coordinate systems
	public struct OrientedPlane3
	{
		public Transform planeTrans;

		public OrientedPlane3(Transform planeTrans)
		{
			this.planeTrans = planeTrans;
		}

		public Plane3 Plane3 => new Plane3(Position, Normal);

		public Vector3 Position => planeTrans.position;

		public Vector3 Normal => planeTrans.up;
	}



	//2D
	public struct Plane2
	{
		public Vector2 pos;

		public Vector2 normal;


		public Plane2(Vector2 pos, Vector2 normal)
		{
			this.pos = pos;

			this.normal = normal;
		}
	}
}

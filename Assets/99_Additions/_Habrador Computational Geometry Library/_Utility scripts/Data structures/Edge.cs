using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
	//And edge between two vertices in 2d space
	public struct Edge2
	{
		public Vector2 p1;
		public Vector2 p2;

		//Is this edge intersecting with another edge?
		//public bool isIntersecting = false;

		public Edge2(Vector2 p1, Vector2 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}
	}



	//And edge between two vertices in 3d space
	public struct Edge3
	{
		public Vector3 p1;
		public Vector3 p2;

		//Is this edge intersecting with another edge?
		//public bool isIntersecting = false;

		public Edge3(Vector3 p1, Vector3 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}
	}
}

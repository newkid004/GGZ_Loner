using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
	//3D
	public struct Ray3
	{
		public Vector3 origin;

		public Vector3 dir;


		public Ray3(Vector3 origin, Vector3 dir)
		{
			this.origin = origin;

			this.dir = dir;
		}
	}



	//2D
	public struct Ray2
	{
		public Vector2 origin;

		public Vector2 dir;


		public Ray2(Vector2 origin, Vector2 dir)
		{
			this.origin = origin;

			this.dir = dir;
		}
	}
}

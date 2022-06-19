using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
	public class LinkedVertex
	{
		public Vector2 pos;

		public LinkedVertex prevLinkedVertex;
		public LinkedVertex nextLinkedVertex;

		public LinkedVertex(Vector2 pos)
		{
			this.pos = pos;
		}
	}
}

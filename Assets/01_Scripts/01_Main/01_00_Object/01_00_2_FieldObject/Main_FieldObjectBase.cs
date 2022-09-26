using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class Main_FieldObjectBase : ProtoPooledObject
	{
		[Header("[Main_FieldObjectBase]")]
		public SpriteRenderer sRenderer;

		public CircleCollider2D colCircle;
		public BoxCollider2D colBox;

		public Collider2D colCurrent { get; protected set; } = null;

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			sRenderer = GetComponent<SpriteRenderer>();

			colCircle = GetComponent<CircleCollider2D>();
			colBox = GetComponent<BoxCollider2D>();
		}
	}
}
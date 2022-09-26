using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Main_FieldCharacterPrePooled : Main_FieldCharacterBase
	{
		[Header("[Main_FieldCharacterPrePooled]")]
		public PolygonCollider2D colPoly;

		protected virtual void Start()
		{
			base.Init();

			Main_FieldObjectManager.Single.ProcessObjectPreRegist(this);
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			colPoly = GetComponent<PolygonCollider2D>();
		}
	}
}
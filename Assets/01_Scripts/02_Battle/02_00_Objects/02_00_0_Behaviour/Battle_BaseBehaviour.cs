using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GGZ
{
	public class Battle_BaseBehaviour : PooledObject
	{
		[Header("----- Base -----")]
		[Header("Own Component")]
		[SerializeField]
		public Battle_BaseCharacter characterOwn;

		public virtual void Init(Battle_BaseCharacter initChar)
		{
			characterOwn = initChar;
		}

		protected virtual void FixedUpdate()
		{
			characterOwn?.ProcessUpdateMove();
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.gameObject.layer == GlobalDefine.CollideLayer.Field_Total)
			{
				OnExitField();
			}
		}

		public virtual void OnChangeDirection(int iBefore, int iAfter, int iBeforeDirect) { }

		public virtual void OnChangePosition(Vector2 vec2Pos) { }

		public virtual void OnExitField()
		{
			characterOwn.ProcessLossLife();
		}
	}
}
using GGZ.GlobalDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Battle_BehaviourPlayer : Battle_BaseBehaviour
	{
		// Char Ref
		public new Battle_CharacterPlayer characterOwn { get => (Battle_CharacterPlayer)base.characterOwn; }

		public Battle_BhvPlayerHuntline bhvHuntline { get; protected set; }
		public Battle_BhvPlayerBullet bhvBullet { get; protected set; }

		public Battle_BehaviourPlayer()
		{
			bhvHuntline = new Battle_BhvPlayerHuntline(this);
			bhvBullet = new Battle_BhvPlayerBullet(this);
		}

		private void OnCollisionEnter2D(Collision2D collision) => bhvHuntline.OnCollisionEnter2D(collision);
		private void OnCollisionExit2D(Collision2D collision) => bhvHuntline.OnCollisionExit2D(collision);
		private void OnTriggerEnter2D(Collider2D collision) => bhvHuntline.OnTriggerEnter2D(collision);
		private void OnTriggerExit2D(Collider2D collision) => bhvHuntline.OnTriggerExit2D(collision);

		public override void OnChangeDirection(int iBefore, int iAfter, int iBeforeDirect) => bhvHuntline.OnChangeDirection(iBefore, iAfter, iBeforeDirect);
		public override void OnChangePosition(Vector2 vec2Pos) => bhvHuntline.OnChangePosition(vec2Pos);

		private void Update()
		{
			bhvHuntline.Update();
		}
	}
}
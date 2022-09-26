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
		public new Battle_CharacterPlayer characterOwn 
		{
			get => (Battle_CharacterPlayer)base.characterOwn; 
			set => base.characterOwn = value;
		}

		public Battle_BhvModulePlayerHuntline bhvHuntline { get; protected set; }
		public Battle_BhvModulePlayerAttack bhvBullet { get; protected set; }

		public override void Init(Battle_BaseCharacter initChar)
		{
			base.Init(initChar);

			bhvHuntline = new Battle_BhvModulePlayerHuntline(this);

			Game.Item.Equipment.EClass eWeaponClass = MainManager.Single.player.eClass;

			switch (eWeaponClass)
			{
				case Game.Item.Equipment.EClass.Summon: break;
				case Game.Item.Equipment.EClass.Worrier: break;
				case Game.Item.Equipment.EClass.Caster: break;
				case Game.Item.Equipment.EClass.Explorer: break;
				case Game.Item.Equipment.EClass.Gunner: bhvBullet = new Battle_BhvModulePAGunner(this); break;
			}

			bhvBullet.isFire = true;
		}

		private void OnCollisionEnter2D(Collision2D collision) => bhvHuntline.OnCollisionEnter2D(collision);
		private void OnCollisionExit2D(Collision2D collision) => bhvHuntline.OnCollisionExit2D(collision);
		private void OnTriggerEnter2D(Collider2D collision) => bhvHuntline.OnTriggerEnter2D(collision);
		private void OnTriggerExit2D(Collider2D collision) => bhvHuntline.OnTriggerExit2D(collision);

		public override void OnChangeDirection(int iBefore, int iAfter, int iBeforeDirect) => bhvHuntline.OnChangeDirection(iBefore, iAfter, iBeforeDirect);
		public override void OnChangePosition(Vector2 vec2Pos) => bhvHuntline.OnChangePosition(vec2Pos);

		private void Update()
		{
			float fTime = Time.deltaTime;

			bhvHuntline.Update();
			bhvBullet.Update(fTime);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public abstract class Battle_BhvModulePlayerAttack
	{
		protected class FireSystem
		{
			public int iBulletInMag;	// 탄창 단 탄환
			public int iBulletLeft;		// 현재 탄환

			public float fFireTime;		// 공격 간격
			public float fFireLeft;		// 공격 간격 : 남은 시간

			public float fCoolTime;		// 재장전 간격
			public float fCoolLeft;		// 재장전 간격 : 남은 시간

			// 이동이 멈춘 후 탄환 발사에 텀이 들어갈 수 있도록 ( 공격속도 영향 ㅇ )

			public bool isReloading;	// 재장전 여부
			public bool isPressFire;	// 발사 동작 여부

			public void Init(int iBulletCount, float fFireTime)
			{
				Init(iBulletCount, fFireTime, 1.0f);
			}

			public virtual void Init(int iBulletCount, float fFireTime, float fCoolTime)
			{
				this.iBulletInMag = iBulletCount;

				this.fFireTime = fFireTime;
				this.fCoolTime = fCoolTime;

				this.iBulletLeft = iBulletCount;
				this.fFireLeft = fFireTime;
				this.fCoolLeft = fCoolTime;
			}

			public virtual int Update(float fTime)
			{
				int iFireCount = 0;

				if (isReloading)
				{
					fCoolLeft -= fTime;

					if (fCoolLeft <= 0.0f)
					{
						isReloading = false;

						iBulletLeft = iBulletInMag;
						fFireLeft = fCoolLeft;			// 남은 간격 보정
					}
				}
				else
				{
					fFireLeft -= fTime;

					if (isPressFire)
					{
						iFireCount = ProcessFire(false, true);
					}
					else
					{
						if (fFireLeft <= 0.0f)
						{
							fFireLeft = 0;
						}
					}
				}

				return iFireCount;
			}

			public virtual bool IsPossibleFire()
			{
				if (isReloading)
					return false;

				if (0.0f < fFireLeft)
					return false;

				if (iBulletLeft == 0)
					return false;

				return true;
			}

			public virtual int ProcessFire(bool isCheckPossible, bool isApplyFire)
			{
				if (isCheckPossible && IsPossibleFire() == false)
					return 0;

				int iFireCount = 0;
				float fFireLeftTemp = fFireLeft;

				while (fFireLeftTemp <= 0.0f && iFireCount < iBulletLeft)
				{
					fFireLeftTemp += fFireTime;
					++iFireCount;
				}

				if (isApplyFire)
				{
					fFireLeft = fFireLeftTemp;
					iBulletLeft -= iFireCount;

					if (iBulletLeft == 0)
					{
						ProcessReload();
					}
				}

				return iFireCount;
			}

			public virtual bool ProcessReload()
			{
				if (isReloading)
					return false;

				isReloading = true;

				fCoolLeft = fCoolTime;

				iBulletLeft = 0;
				fFireLeft = fFireTime;

				return true;
			}
		}

		protected Battle_BehaviourPlayer bhvPlayer;
		protected Battle_CharacterPlayer charPlayer;
		protected Battle_ItemEquipment itWeapon;

		protected FireSystem fireSystem;
		protected Battle_BaseMonster monTarget;

		public bool isFire
		{
			get => fireSystem == null ? false : fireSystem.isPressFire;
			set
			{
				if (fireSystem == null)
					return;

				fireSystem.isPressFire = value;
			}
		}

		public int iBulletLeftCount { get => fireSystem == null ? -1 : fireSystem.iBulletLeft; }

		private Battle_BhvModulePlayerAttack() { }
		public Battle_BhvModulePlayerAttack(Battle_BehaviourPlayer bhvPlayer)
		{
			this.bhvPlayer = bhvPlayer;
			this.charPlayer = bhvPlayer.characterOwn;
			this.itWeapon = MainManager.Single.player.GetEquipment(Game.Item.Equipment.ESlot.Weapon);

			fireSystem = null;
		}

		public virtual void Update(float fTime)
		{
			if (fireSystem == null)
				return;

			int iFireCount = fireSystem.Update(fTime);
			if (iFireCount == 0)
				return;

			if (isFire)
			{
				Attack(iFireCount);
			}
		}

		public abstract void Attack(int iCount);
	}

	public class Battle_BhvModulePAGunner : Battle_BhvModulePlayerAttack
	{
		public Battle_BhvModulePAGunner(Battle_BehaviourPlayer bhvPlayer) : base(bhvPlayer) 
		{
			fireSystem = new FireSystem();

			fireSystem.Init(1, 1.0f / charPlayer.csStatBasic.fAttackSpeed);
		}

		public override void Attack(int iCount)
		{
			var monClosest = Battle_MonsterManager.Single.GetClosestMonster();

			if (monClosest == null)
				return;

			int iBulletID;
			AnimationModule.Group aniGroupBullet;

			if (itWeapon == null)
			{
				iBulletID = -(int)Game.Item.Equipment.EClass.Gunner;
				aniGroupBullet = AnimationManager.Single.GetGroupItem(AnimationManager.EAniType.Bullet, iBulletID, "idle");
			}
			else
			{
				iBulletID = -(int)itWeapon.eTypeClass;
				aniGroupBullet = AnimationManager.Single.GetGroupItem(AnimationManager.EAniType.Bullet, itWeapon.iID, "idle");

				if (aniGroupBullet == null)
				{
					aniGroupBullet = AnimationManager.Single.GetGroupItem(AnimationManager.EAniType.Bullet, iBulletID, "idle");
				}
			}

			for (int i = 0; i < iCount; ++i)
			{
				var objBullet = Battle_BulletManager.Single.Create(iBulletID, charPlayer);
				
				if (objBullet != null)
				{
					objBullet.AniModule.SetGroup(aniGroupBullet);
					objBullet.AniModule.Play(0);

					Battle_BulletManager.Single.Fire(objBullet, monClosest);
				}
			}
		}
	}
}

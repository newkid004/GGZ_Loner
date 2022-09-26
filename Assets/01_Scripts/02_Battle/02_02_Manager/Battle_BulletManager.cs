using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalUtility;
	using GGZ.GlobalDefine;
	using static GGZ.Battle_SkillManager;

	[System.Serializable]
	public class Battle_BulletManager
	{
		public static Battle_BulletManager Single { get => SceneMain_Battle.Single.mcsBullet; }

		[SerializeField] private ObjectPool<Battle_BaseBullet> oPool = new ObjectPool<Battle_BaseBullet>();

		public void Init()
		{
			oPool.Init();
		}

		public Battle_BaseBullet GetBullet(int iSequenceID) => oPool.GetObject(iSequenceID);

		private Battle_BaseBullet PopToID(int iID, Transform trParent = null)
		{
			Battle_BaseBullet obj;

			switch (iID)
			{
				case -(int)Game.Item.Equipment.EClass.Summon:	obj = oPool.Pop<Battle_PlayerBulletGunner>(trParent); break;
				case -(int)Game.Item.Equipment.EClass.Worrier:	obj = oPool.Pop<Battle_PlayerBulletGunner>(trParent); break;
				case -(int)Game.Item.Equipment.EClass.Caster:	obj = oPool.Pop<Battle_PlayerBulletGunner>(trParent); break;
				case -(int)Game.Item.Equipment.EClass.Explorer:	obj = oPool.Pop<Battle_PlayerBulletGunner>(trParent); break;
				case -(int)Game.Item.Equipment.EClass.Gunner:	obj = oPool.Pop<Battle_PlayerBulletGunner>(trParent); break;
				default: obj = oPool.Pop<Battle_BulletActiveSkill>(trParent); break;
			}

			obj.ReconnectRefSelf();

			return obj;
		} // Push는 관리되며 구현할 필요 없음

		public Battle_BaseBullet Create(int iID, Battle_BaseCharacter charOwner)
		{
			Transform trGround = SceneMain_Battle.Single.mcsField.trCharacterGround;
			Battle_BaseBullet obj = PopToID(iID, SceneMain_Battle.Single.mcsField.trCharacterGround);

			InitBulletToCSV(obj, iID);
			InitBulletToChar(obj, charOwner);

			return obj;
		}

		private void InitBulletToCSV(Battle_BaseBullet obj, int iID)
		{
			var csvBullet = CSVData.Battle.Skill.BulletInfo.Manager.Get(iID);

			obj.csvInfo = csvBullet;

			obj.fDeadTime = Time.time + csvBullet.LifeTime;
			obj.colCircle.radius = csvBullet.Radius;

			switch (csvBullet.MoveType)
			{
				case 0:
				{
					obj.fSpeed = csvBullet.Speed;
					break;
				}
			}
		}

		private void InitBulletToChar(Battle_BaseBullet obj, Battle_BaseCharacter charOwner)
		{
			obj.charOwner = charOwner;
			obj.isAlly = 0 < (charOwner.iObjectType & ObjectData.ObjectType.ciAlly);
		}

		public void Fire(Battle_BaseBullet obj, Battle_BaseCharacter charTarget)
		{
			if (obj.transform.position.Vec2() == Vector2.zero)
			{
				if (Digit.Include(obj.charOwner.iObjectType, ObjectData.ObjectType.ciCharacter))
				{
					obj.transform.position = GetBulletPosition(obj.charOwner, obj);
				}
				else
				{
					obj.transform.position = obj.charOwner.transform.position;
				}
			}

			if (obj.vec2Direction == Vector2.zero)
			{
				obj.TargetTo(charTarget.transform.position.Vec2());
			}
		}

		public void Fire(Battle_BaseBullet obj, Battle_BaseCharacter charTarget, ref stSkillProcessInfo info)
		{
			if (info.vec2Pos == Vector2.zero)
			{
				if (Digit.Include(info.objOwner.iObjectType, ObjectData.ObjectType.ciCharacter))
				{
					obj.transform.position = GetBulletPosition((Battle_BaseCharacter)info.objOwner, obj);
				}
				else
				{
					obj.transform.position = info.objOwner.transform.position;
				}

			}
			else
			{
				obj.transform.position = info.vec2Pos;
			}

			if (obj.vec2Direction == Vector2.zero)
			{
				obj.TargetTo(charTarget.transform.position.Vec2());
			}
			else
			{
				obj.vec2Direction = info.vec2Dir;
			}
		}

		public Vector2 GetBulletPosition(Battle_BaseCharacter charOwner, Battle_BaseBullet blt)
		{
			var colOwner = (CircleCollider2D)charOwner.colliderOwn;

			return charOwner.transform.position.Vec2() +
				(blt.vec2Direction * blt.colCircle.radius) +
				(blt.vec2Direction * colOwner.radius * blt.csvInfo.Interval);
		}

		public Vector2 GetBulletDirection(Battle_BaseObject charOwner, Battle_BaseObject charTarget)
		{
			return Vector3.Normalize(charTarget.transform.position - charOwner.transform.position);
		}
	}
}
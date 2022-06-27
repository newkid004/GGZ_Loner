using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalUtility;
	using GGZ.GlobalDefine;

	[System.Serializable]
	public class Battle_BulletManager
	{
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
				case 4: obj = oPool.Pop<Battle_BulletActiveSkill>(trParent); break;
				default: obj = oPool.Pop<Battle_BulletTest>(trParent); break;
			}

			obj.ReconnectRefSelf();

			return obj;
		} // Push는 관리되며 구현할 필요 없음

		public Battle_BaseBullet Create(int iID, Battle_BaseCharacter charOwner)
		{
			Transform trGround = SceneMain_Battle.Single.mcsField.trCharacterGround;
			Battle_BaseBullet obj = PopToID(iID, SceneMain_Battle.Single.mcsField.trCharacterGround);

			InitBulletToCSV(obj, iID, charOwner);

			return obj;
		}

		private void InitBulletToCSV(Battle_BaseBullet obj, int iID, Battle_BaseCharacter charOwner)
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

			obj.charOwner = charOwner;
			obj.isAlly = 0 < (charOwner.iObjectType & ObjectData.ObjectType.ciAlly);
		}
	}
}
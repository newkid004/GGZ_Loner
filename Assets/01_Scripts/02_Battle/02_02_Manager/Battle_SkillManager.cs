using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;
	using GlobalUtility;

	[System.Serializable]
	public class Battle_SkillManager
	{
		public static Battle_SkillManager Single { get => SceneMain_Battle.Single.mcsSkill; }

		public struct stHitTypeInfo
		{
			public CSVData.Battle.Skill.TargetingPreset csvTargetingPreset;

			public bool isActiveAlly;
			public Battle_BaseObject objOwner;
			public Battle_BaseObject objTarget;

			public int dgCalcTargetingFlag;		// output
		}

		public void CheckHitType(ref stHitTypeInfo info)
		{
			int dgType = 0;

			int dgCompareFlag;

			dgCompareFlag = 1 << 0;		// 0 : �ڱ� �ڽ�
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.objOwner == info.objTarget ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 1;		// 1 : �Ʊ� (�����)
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciAlly, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 2;		// 2 : ���� (�����)
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Declude(ObjectData.ObjectType.ciAlly, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 3;		// 3 : �÷��̾�
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciPlayer, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 4;		// 4 : ����
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Declude(ObjectData.ObjectType.ciPlayer, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 5;		// 5 : ����
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciBoss, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 6;		// 6 : ���۵� �����
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciHuntZone | ObjectData.ObjectType.ciHuntZoneOutline, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 7;		// 7 : �������� ��ɼ�
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciHuntLine, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 8;		// 8 : źȯ
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciBullet, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			info.dgCalcTargetingFlag = dgType;
		}

		public struct stSkillProcessInfo
		{
			public CSVData.Battle.Skill.SkillActive csvSkillActive;

			public Vector2 vec2Pos;
			public Vector2 vec2Dir;

			public Battle_BaseObject objOwner;
			public Battle_BaseObject objTarget;

			public Battle_BaseBullet bltHit;

			// �� ���� ( ���� )
			public int		iIParamAddition;
			public float	fFParamAddition;

			// �� ���� ( ���� )
			public float	fIParamPercent;
			public float	fFParamPercent;

			public stSkillProcessInfo(CSVData.Battle.Skill.SkillActive arg_csvSkillActive)
			{
				csvSkillActive = arg_csvSkillActive;

				vec2Pos = Vector2.zero;
				vec2Dir = Vector2.zero;

				objOwner = null;
				objTarget = null;

				bltHit = null;

				iIParamAddition = 0;
				fFParamAddition = 0;

				fIParamPercent = 1;
				fFParamPercent = 1;
			}

			public stSkillProcessInfo(int iCsvSkillActiveID) : this(CSVData.Battle.Skill.SkillActive.Manager.Get(iCsvSkillActiveID)) { }
		}

		public void ProcessSkill(ref stSkillProcessInfo info)
		{
			if (info.csvSkillActive == null)
				return;

			switch (info.csvSkillActive.ActiveType)
			{
				case 0:		// Bullet ����			- [BulletID / ]
				{
					int iBulletID = info.csvSkillActive.ParamInts[0];
					var blt = Battle_BulletManager.Single.Create(iBulletID, (Battle_BaseCharacter)info.objOwner);

					if (info.vec2Dir == Vector2.zero)
					{
						blt.vec2Direction = Battle_BulletManager.Single.GetBulletDirection(info.objOwner, info.objTarget);
					}
					else
					{
						blt.vec2Direction = info.vec2Dir;
					}

					if (info.vec2Pos == Vector2.zero)
					{
						if (Digit.Include(info.objOwner.iObjectType, ObjectData.ObjectType.ciCharacter))
						{
							blt.transform.position = Battle_BulletManager.Single.GetBulletPosition((Battle_BaseCharacter)info.objOwner, info.objTarget, blt);
						}
						else
						{
							blt.transform.position = info.objOwner.transform.position;
						}

					}
					else
					{
						blt.transform.position = info.vec2Pos;
					}
				}
				break;

				case 1:		// Buff ����			- [BuffID / Time, Tick, Value]
				{
					int iBuffID = -1;

					if (0 < info.csvSkillActive.ParamInts.Length)
						iBuffID = info.csvSkillActive.ParamInts[0];

					var stBuffInfo = new Battle_BuffManager.stBuffProcessInfo();

					stBuffInfo.csvBuffInfo = CSVData.Battle.Skill.BuffInfo.Manager.Get(iBuffID);

					if (stBuffInfo.csvBuffInfo != null)
					{
						stBuffInfo.objOwner = info.objOwner;
						stBuffInfo.objTarget = info.objTarget;
						stBuffInfo.arrfEffection = info.csvSkillActive.ParamFloats;

						Battle_BuffManager.Single.ProcessBuff(ref stBuffInfo);
					}
				}
				break;

				case 2:		// �Ǵٸ� Skill �ߵ�	- [SkillID, .. / ]
				{
					var stSkillInfo = info;

					foreach (int iSkillID in info.csvSkillActive.ParamInts)
					{
						stSkillInfo.csvSkillActive = CSVData.Battle.Skill.SkillActive.Manager.Get(iSkillID);
						ProcessSkill(ref stSkillInfo);
					}
				}
				break;
			}
		}
	}
}
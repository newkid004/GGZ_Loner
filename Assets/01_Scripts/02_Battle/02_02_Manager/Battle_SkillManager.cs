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

			dgCompareFlag = 1 << 0;		// 0 : 자기 자신
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.objOwner == info.objTarget ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 1;		// 1 : 아군 (상대적)
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciAlly, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 2;		// 2 : 적군 (상대적)
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Declude(ObjectData.ObjectType.ciAlly, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 3;		// 3 : 플레이어
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciPlayer, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 4;		// 4 : 몬스터
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Declude(ObjectData.ObjectType.ciPlayer, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 5;		// 5 : 보스
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciBoss, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 6;		// 6 : 제작된 사냥터
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciHuntZone | ObjectData.ObjectType.ciHuntZoneOutline, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 7;		// 7 : 제작중인 사냥선
			if (Digit.Include(dgCompareFlag, info.csvTargetingPreset.Flag))
			{
				dgType = Digit.OR(dgType, info.isActiveAlly == Digit.Include(ObjectData.ObjectType.ciHuntLine, info.objTarget.iObjectType) ? (dgCompareFlag) : 0);
			}

			dgCompareFlag = 1 << 8;		// 8 : 탄환
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

			// 값 보정 ( 정수 )
			public int		iIParamAddition;
			public float	fFParamAddition;

			// 값 보정 ( 비율 )
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
				case 0:		// Bullet 생성			- [BulletID / ]
				{
					int iBulletID = info.csvSkillActive.ParamInts[0];
					var blt = Battle_BulletManager.Single.Create(iBulletID, (Battle_BaseCharacter)info.objOwner);

					Battle_BulletManager.Single.Fire(blt, (Battle_BaseCharacter)info.objTarget, ref info);
				}
				break;

				case 1:		// Buff 적용			- [BuffID / Time, Tick, Value]
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

				case 2:		// 또다른 Skill 발동	- [SkillID, .. / ]
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
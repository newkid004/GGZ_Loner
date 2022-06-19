using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class Battle_BaseBuff : PooledMemory
	{
		public CSVData.Battle.Skill.BuffInfo csvBuffInfo;

		public float fActiveValue;
		public float fActiveTick;

		public float fTimeLeft;
		public float fNextTickLeft;

		public Battle_BaseObject objOwner;
		public Battle_BaseObject objTarget;

		public virtual float fCalcedValue => csvBuffInfo.ActValue;
		public virtual float fCalcedTime => csvBuffInfo.ActTime;
		public virtual float fCalcedTick => csvBuffInfo.ActTick;

		public virtual bool IsCompleted { get => fTimeLeft< 0; }

		public virtual void Init(ref Battle_BuffManager.stBuffProcessInfo info)
		{
			csvBuffInfo = info.csvBuffInfo;
			objOwner = info.objOwner;
			objTarget = info.objTarget;

			fActiveValue = fCalcedValue;
			fActiveTick = fCalcedTick;

			fTimeLeft = fCalcedTime;
			fNextTickLeft = fCalcedTick;

			fTimeLeft *= 0 < info.arrfEffection.Length ? info.arrfEffection[0] : 1;
			fActiveTick *= 1 < info.arrfEffection.Length ? info.arrfEffection[1] : 1;
			fActiveValue *= 2 < info.arrfEffection.Length ? info.arrfEffection[2] : 1;
		}

		public virtual void FixedUpdate(float fFixedDeltaTime)
		{
			if (IsCompleted)
				return;

			fNextTickLeft -= fFixedDeltaTime;

			float fTickAddition = fActiveTick;
			while (fNextTickLeft < 0)
			{
				var OwnSkillInfo = CreateOwnSkillProcess(csvBuffInfo.ActiveSkillIDTick);
				TriggeredByTickBuff(ref OwnSkillInfo);

				fNextTickLeft += fTickAddition;
			}

			fTimeLeft -= fFixedDeltaTime;
		}

		public Battle_SkillManager.stSkillProcessInfo CreateOwnSkillProcess(int iSkillID)
		{
			var stSkillInfo = new Battle_SkillManager.stSkillProcessInfo(CSVData.Battle.Skill.SkillActive.Manager.Get(iSkillID));

			stSkillInfo.objOwner = this.objOwner;
			stSkillInfo.objTarget = this.objTarget;
			stSkillInfo.vec2Pos = this.objTarget.transform.position;

			if (GlobalUtility.Digit.Include(this.objTarget.iObjectType, GlobalDefine.ObjectData.ObjectType.ciCharacter))
			{
				Battle_BaseCharacter objChar = (Battle_BaseCharacter)this.objTarget;
				stSkillInfo.vec2Dir = GlobalDefine.Direction8.GetNormalByDirection(objChar.iDirection);
			}

			stSkillInfo.bltHit = null;

			stSkillInfo.fFParamPercent *= fActiveValue;

			return stSkillInfo;
		}

		// 트리거 : Target 오브젝트에게 버프가 적용
		public virtual void TriggeredByBeginBuff(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo) => Battle_SkillManager.Single.ProcessSkill(ref stSkillInfo);
		public virtual void TriggeredByTickBuff(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo) => Battle_SkillManager.Single.ProcessSkill(ref stSkillInfo);
		public virtual void TriggeredByEndBuff(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo) => Battle_SkillManager.Single.ProcessSkill(ref stSkillInfo);
	}
}
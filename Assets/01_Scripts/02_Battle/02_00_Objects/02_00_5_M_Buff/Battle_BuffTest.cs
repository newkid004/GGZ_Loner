using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GGZ
{
	public class Battle_BuffTest5 : Battle_BaseBuff
	{
		public override void TriggeredByBeginBuff(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo)
		{
			base.TriggeredByBeginBuff(ref stSkillInfo);

			var objTargetChar = (Battle_BaseCharacter)stSkillInfo.objTarget;

			objTargetChar.AniModule.spriteRenderer.color = Color.red;
			objTargetChar.AniModule.spriteRenderer.DOColor(Color.white, 0.5f);
		}
	}

	public class Battle_BuffTest6 : Battle_BaseBuff
	{
		public override void TriggeredByBeginBuff(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo)
		{
			base.TriggeredByBeginBuff(ref stSkillInfo);

			var objTargetChar = (Battle_BaseCharacter)stSkillInfo.objTarget;

			objTargetChar.AniModule.spriteRenderer.color = Color.blue;
			objTargetChar.AniModule.spriteRenderer.DOColor(Color.white, 0.5f);
		}
	}

	public class Battle_BuffTest7 : Battle_BaseBuff
	{
		public override void TriggeredByBeginBuff(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo)
		{
			base.TriggeredByBeginBuff(ref stSkillInfo);

			var objTargetChar = (Battle_BaseCharacter)stSkillInfo.objTarget;

			objTargetChar.AniModule.spriteRenderer.color = Color.green;
			objTargetChar.AniModule.spriteRenderer.DOColor(Color.white, 0.5f);
		}
	}
}

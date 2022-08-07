using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Battle_BuffProcessHit : Battle_BaseBuff
	{
		public override void TriggeredByBeginBuff(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo)
		{
			base.TriggeredByBeginBuff(ref stSkillInfo);
			BattleManager.Single.ProcessCharacterHit(ref stSkillInfo);
		}
	}
}
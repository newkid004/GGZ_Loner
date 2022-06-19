using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Battle_BulletActiveSkill : Battle_BaseBullet
	{
		public override void TriggerByHit_TargetHit(Battle_BaseObject target, int dgType)
		{
			var stSkillInfo = new Battle_SkillManager.stSkillProcessInfo(CSVData.Battle.Skill.SkillActive.Manager.Get(csvInfo.SkillActiveID));
			
			stSkillInfo.vec2Pos = this.transform.position;
			stSkillInfo.vec2Dir = this.vec2Direction;

			stSkillInfo.objOwner = this.charOwner;
			stSkillInfo.objTarget = target;

			stSkillInfo.bltHit = this;

			SceneMain_Battle.Single.mcsSkill.ProcessSkill(ref stSkillInfo);
		}
	}
}

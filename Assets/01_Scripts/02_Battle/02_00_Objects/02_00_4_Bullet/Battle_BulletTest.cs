using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace GGZ
{
	public class Battle_BulletTest : Battle_BaseBullet
	{
		public override void TriggerByHit_TargetHit(Battle_BaseObject target, int dgType)
		{
			// 타격 연출
			Battle_BaseCharacter charTarget = (Battle_BaseCharacter)target;
			charTarget.AniModule.spriteRenderer.material.color = Color.red;
			charTarget.AniModule.spriteRenderer.material.DOColor(Color.white, 1.0f);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Proto_00_N
{
	[System.Serializable]
	public class Battle_MonsterManager
	{
		[SerializeField]
		private ObjectPool<Battle_BaseMonster> oPool = new ObjectPool<Battle_BaseMonster>();

		public RaycastHit2D[] arrRch2CharMoveResult = new RaycastHit2D[8];

		public void Init()
		{
			oPool.Init();
		}

		public Battle_BaseMonster GetObject(int iSequenceID) => oPool.GetObject(iSequenceID);

		public T PopObj<T>(Transform trParent = null) where T : Battle_BaseMonster
		{
			Battle_BaseMonster charMonster = oPool.Pop<T>(trParent);
			charMonster.ReconnectRefSelf();

			return (T)charMonster;
		} // Push는 관리되며 구현할 필요 없음

		public void AnimateKnockbackByZone(Battle_BaseMonster csMonster, Vector2 vec2TargetPos)
		{
			Vector2 vec2SourcePos = csMonster.transform.position;

			float fFloatingHeight = 1f / csMonster.csStatEffect.fWeight;
			float fFloatingTime = csMonster.csStatEffect.fAirHold;

			CustomRoutine.CallInTime(fFloatingTime,
				(fAlpha) =>
				{
					Vector2 vec2CurrentPos = Vector2.Lerp(vec2SourcePos, vec2TargetPos, fAlpha);
					vec2CurrentPos.y += fAlpha < 0.5f ?
						Easing.EaseOutSine(0, fFloatingHeight, fAlpha * 2f) :
						Easing.EaseOutBounce(fFloatingHeight, 0, (fAlpha - 0.5f) * 2f);

					csMonster.transform.position = vec2CurrentPos;
				},
				() => csMonster.transform.position = vec2TargetPos);
		}
	}
}

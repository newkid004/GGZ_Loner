using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GGZ
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
		public Battle_BaseMonster GetRandom() => oPool.GetRandomObjectInActive<Battle_BaseMonster>();

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

		public Battle_BaseMonster CreateMonster(int iID)
		{
			Battle_BaseMonster tResult = PopObj<Battle_BaseMonster>(SceneMain_Battle.Single.mcsField.trCharacterGround);

			InitMonsterStatus(tResult, iID);
			InitMonsterPosition(tResult);

			return tResult;
		}

		private void InitMonsterStatus(Battle_BaseMonster monObject, int iID)
		{
			var csvMonster = CSVData.Battle.Status.Unit.Manager.Get(iID);

			monObject.csStatBasic.fHealthMax = csvMonster.Health;
			monObject.csStatBasic.fHealthNow = csvMonster.Health;

			monObject.csStatBasic.fAttackPower = csvMonster.Attack;
			monObject.csStatBasic.fDefendPower = csvMonster.Defend;

			monObject.csStatBasic.fAttackSpeed = csvMonster.AttackSpeed;
			monObject.csStatBasic.fMoveSpeed = csvMonster.MoveSpeed;

			monObject.csStatEffect.fWeight = csvMonster.Weight;
			monObject.csStatEffect.fAirHold = csvMonster.AirHold;
		}

		private void InitMonsterPosition(Battle_BaseMonster monObject)
		{

		}
	}
}

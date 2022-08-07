using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GGZ
{
	[System.Serializable]
	public class Battle_MonsterManager
	{
		public static Battle_MonsterManager Single { get => SceneMain_Battle.Single.mcsMonster; }

		[SerializeField]
		public ObjectPool<Battle_BaseMonster> oPool = new ObjectPool<Battle_BaseMonster>();

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
			Battle_BaseMonster monResult;

			switch (iID)
			{
				default: monResult = PopObj<Battle_BaseMonster>(SceneMain_Battle.Single.mcsField.trCharacterGround); break;
			}

			if (monResult != null)
			{
				monResult.iCharacterID = iID;

				InitMonsterStatus(monResult, iID);
				InitMonsterPosition(monResult);
				InitMonsterAnimation(monResult);
			}

			return monResult;
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

		private void InitMonsterAnimation(Battle_BaseMonster monObject)
		{
			var aniGroup = AnimationManager.Single.GetGroupItem(monObject.EAniType, monObject.iCharacterID, "idle");

			monObject.AniModule.SetGroup(aniGroup);
			monObject.AniModule.Play(0);
		}

#if _debug
		public bool IsDebug;
		private const float GIZMO_DISK_THICKNESS = 0.01f;
		public void OnDrawGizmo()
		{

			oPool.LoopOnActiveTotal(mon =>
			{
				// Alert
				Gizmos.color = new Color(1, 0.2f, 0.2f, 0.25f);
				float corners = 19; // How many corners the circle should have
				float size = mon.csvMonster.AlertRadius; // How wide the circle should be
				Vector3 origin = mon.transform.position; // Where the circle will be drawn around
				Vector3 startRotation = mon.transform.right * size; // Where the first point of the circle starts
				Vector3 lastPosition = origin + startRotation;
				float angle = 0;
				while (angle <= 360)
				{
					angle += 360 / corners;
					Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
					Gizmos.DrawLine(lastPosition, nextPosition);
					// Gizmos.DrawSphere(nextPosition, 1);

					lastPosition = nextPosition;
				}

				// Attack
				Gizmos.color = new Color(1, 0.2f, 0.2f, 0.5f);
				size = mon.csvMonster.AttackRadius;
				startRotation = mon.transform.right * size;
				lastPosition = origin + startRotation;
				angle = 0;
				while (angle <= 360)
				{
					angle += 360 / corners;
					Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
					Gizmos.DrawLine(lastPosition, nextPosition);
					// Gizmos.DrawSphere(nextPosition, 1);

					lastPosition = nextPosition;
				}
			});
		}
#endif
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_BehaviourMonster : Battle_BaseBehaviour
	{
		public enum EMoveState
		{
			Wait,
			Move,
			Aggressive,
			MAX
		}

		public enum ESubState
		{
			None,
			Attack,
			Skill,
			MAX
		}

		public enum EState
		{
			Move, Wait,
			MAX
		}

		[Header("----- Wander -----")]

		public CSVData.Battle.UnitBehaviour.PatternGroup csvPatternGroup = null;

		[Tooltip("최대 이동 시간")]
		public float fMoveTime = 0.5f;
		[Range(0.0f, 1.0f)] public float fMoveTimeRange = 1.0f;

		[Tooltip("이동 후 대기시간")]
		public float fWaitAfterMove = 0.5f;
		[Range(0.0f, 1.0f)] public float fWaitAfterMoveRange = 1.0f;

		private float fNextChangeStateTime = 0;

		public EMoveState eMoveState = EMoveState.Wait;
		public ESubState eSubState = ESubState.None;
		public EState eNowState = EState.Wait;

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			UpdateChangeState();
		}

		private void UpdateChangeState()
		{
			if (Time.time < fNextChangeStateTime)
				return;

			eNowState = (EState)(((int)eNowState + 1) % (int)EState.MAX);

			// 시간 입력
			float fMinNextTimeRange;
			float fMinNextTimeInterval;
			float fMaxNextTimeInterval;

			switch (eNowState)
			{
				case EState.Move:
				fMinNextTimeRange = fMoveTimeRange;
				fMaxNextTimeInterval = fMoveTime;
				break;
				case EState.Wait:
				fMinNextTimeRange = fWaitAfterMoveRange;
				fMaxNextTimeInterval = fWaitAfterMove;
				break;

				default:
				fMinNextTimeRange = 0;
				fMaxNextTimeInterval = 0;
				break;
			}

			fMinNextTimeInterval = fMaxNextTimeInterval * fMinNextTimeRange;
			fNextChangeStateTime = Time.time + Random.Range(fMinNextTimeInterval, fMaxNextTimeInterval);

			OnChangeState(eNowState);
		}

		protected virtual void OnChangeState(EState eState)
		{
			switch (eState)
			{
				case EState.Move:
				characterOwn.ProcessEnterMove(Direction8.GetRandomDirection());

				break;
				case EState.Wait:
				characterOwn.ProcessExitMove();

				break;
			}
		}
	}
}


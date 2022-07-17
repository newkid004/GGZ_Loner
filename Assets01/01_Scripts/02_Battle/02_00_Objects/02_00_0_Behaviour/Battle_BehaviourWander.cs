using Proto_00_N.GlobalDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	// ������ 8�������� �̵�
	public class Battle_BehaviourWander : Battle_BaseBehaviour
	{
		public enum EState
		{
			Move, Wait,
			MAX
		}

		[Header("----- Wander -----")]

		[Tooltip("�ִ� �̵� �ð�")]
		public float fMoveTime = 0.5f;
		[Range(0.0f, 1.0f)] public float fMoveTimeRange = 1.0f;

		[Tooltip("�̵� �� ���ð�")]
		public float fWaitAfterMove = 0.5f;
		[Range(0.0f, 1.0f)] public float fWaitAfterMoveRange = 1.0f;

		private float fNextChangeStateTime = 0;

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

			// �ð� �Է�
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

		private void OnChangeState(EState eState)
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
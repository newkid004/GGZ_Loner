using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;
	using System;

	public class Battle_CharacterPlayer : Battle_BaseCharacter
	{
		[SerializeField]
		private ObjectData.StatusPlayer _csPlayerStatus = new ObjectData.StatusPlayer();
		public ObjectData.StatusPlayer csPlayerStatus { get => _csPlayerStatus; }

		public new Battle_BehaviourPlayer behaviorOwn { get => (Battle_BehaviourPlayer)base.behaviorOwn; }

		public int iLastInputDirection;
		public override int iDirection 
		{
			get => base.iDirection;
			set
			{
				// 방향 전환 트리거
				if (value != _iDirection)
				{
					// 사냥선을 그릴 때, 반대 방향이면 전환하지 않음
					if (behaviorOwn.isCharDrawingHuntLine)
					{
						Battle_HPoint hlpDrawing = SceneMain_Battle.Single.mcsHLine.nowDrawingPoint;
						if (hlpDrawing.iDirectionDraw == Direction8.GetInverseDirection(value))
							return;
					}

					iLastMovedDirection = _iDirection;
					_iDirection = value;
					behaviorOwn.OnChangeDirection(_iDirection, value, iLastMovedDirection);
				}
			}
		}

		protected override void Init()
		{
			base.Init();

			base.iObjectType |= 
				ObjectData.ObjectType.ciPlayer |
				ObjectData.ObjectType.ciAlly;
			base.iAttribute |= ObjectData.Attribute.ciBasic_Player;

			lCharacterID = 0;
		}

		public override void ProcessUpdateMove()
		{
			Battle_BehaviourPlayer bhvPlayer = behaviorOwn;

			if (bhvPlayer.isCharInHuntZone)
			{
				if (isMove)
				{
					// 사냥터 내부 이동
					MoveDirectional();
				}
				else
				{
					MoveStand();
				}
			}
			else
			{
				if (isMove)
				{
					// 사냥선 작성 이동
					MoveDirectional();
				}
				else if (false == bhvPlayer.isDownHuntLineBtn && false == bhvPlayer.isCharInHuntZone)
				{
					// 사냥선 되돌아오기
					MoveReturnToHuntZone();
				}
				else
				{
					MoveStand();
				}
			}
		}

		protected void MoveDirectional()
		{
			float fMoveDistance = csStatBasic.fMoveSpeed;
			Vector2 vec2NormalDirection = Direction8.GetNormalByDirection(_iDirection);

			RigidbodyOwn.velocity = fMoveDistance * vec2NormalDirection;

			behaviorOwn.OnChangePosition(transform.position);
		}

		protected void MoveReturnToHuntZone()
		{
			float fMoveSpeed = csStatBasic.fMoveSpeed;
			float fMoveReturnSpeed = fMoveSpeed * csPlayerStatus.fHuntlineDrawReturnSpeed;
			float fMoveReturnSpeedInTime = fMoveReturnSpeed * Time.fixedDeltaTime;

			Battle_HLine hlLast = SceneMain_Battle.Single.mcsHLine.nowDrawingLine;
			Battle_HPoint hpLast = hlLast?.listPoint.Back();

			Vector2 vec2PrevPoint;
			Vector2 vec2PrevDirection;
			float fPrevDistance;

			if (hpLast == null || hpLast.PointPrev == null)
			{
				vec2PrevPoint = hlLast.transform.position;
				vec2PrevDirection = Direction8.GetNormalToInterval(transform.position, vec2PrevPoint);
				fPrevDistance = Vector2.Distance(transform.position, SceneMain_Battle.Single.mcsHLine.nowDrawingLine.transform.position);
			}
			else
			{
				vec2PrevPoint = hpLast.PointPrev.transform.position;
				vec2PrevDirection = Direction8.GetNormalToInterval(hpLast.transform.position, vec2PrevPoint);
				fPrevDistance = Vector2.Distance(transform.position, vec2PrevPoint);
			}

			if (fPrevDistance < fMoveReturnSpeedInTime)
			{
				// 거리 보정
				fMoveReturnSpeedInTime -= fPrevDistance;

				if (hpLast == null || hpLast.PointPrev == null)
				{
					vec2PrevDirection = Direction8.GetNormalToInterval(transform.position, vec2PrevPoint);
				}
				else
				{
				}

					vec2PrevPoint += fMoveReturnSpeedInTime * vec2PrevDirection;

				transform.position = vec2PrevPoint;
				RigidbodyOwn.velocity = vec2PrevDirection;

				// 보정 사냥점 제거
				SceneMain_Battle.Single.mcsHLine.CancelDrawPoint();

				// if (hlpLast == SceneMain_Battle.Single.mcsHLine.nowDrawingPoint)
				// {
				// 	// 보정 사냥선 제거
				// 	SceneMain_Battle.Single.mcsHLine.CancelDrawLine();
				// }
			}
			else
			{
				// 일반 되돌아가기
				Vector2 vec2NormalDirection = -Direction8.GetNormalByDirection(hpLast.iDirectionDraw);
				RigidbodyOwn.velocity = fMoveReturnSpeed * vec2NormalDirection;
			}

			behaviorOwn.OnChangePosition(transform.position);
		}

		protected void MoveStand()
		{
			RigidbodyOwn.velocity = Vector2.zero;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
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
						Battle_HuntLinePoint hlpDrawing = SceneMain_Battle.Single.mcsHuntLine.hlpDrawing;
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
			Vector2 vec2NormalDirection = Direction8.GetNormalByDirection(iDirection);

			RigidbodyOwn.velocity = fMoveDistance * vec2NormalDirection;
			behaviorOwn.OnChangePosition(transform.position);
		}

		protected void MoveReturnToHuntZone()
		{
			Battle_HuntLinePoint hlpLast = SceneMain_Battle.Single.mcsHuntLine.hlpDrawing;
			Battle_HuntLinePoint hlpLastPrev = hlpLast.PointPrev;

			float fMoveSpeed = csStatBasic.fMoveSpeed;
			float fMoveReturnSpeed = csPlayerStatus.fHuntlineDrawReturnSpeed;

			float fMoveDistance = fMoveSpeed * fMoveReturnSpeed;
			float fPrevDistance = Vector2.Distance(transform.position, hlpLastPrev.transform.position);

			float fScaledMoveDistance = fMoveDistance * Time.fixedDeltaTime;

			if (fPrevDistance < fScaledMoveDistance)
			{
				// 거리 보정
				fScaledMoveDistance -= fPrevDistance;

				Vector2 vec2PrevDirection = Direction8.GetNormalByDirection(hlpLastPrev.iDirection8);
				Vector2 vec2PrevPos = hlpLastPrev.transform.position;
				vec2PrevPos -= fScaledMoveDistance * vec2PrevDirection;

				transform.position = vec2PrevPos;
				RigidbodyOwn.velocity = -vec2PrevDirection;

				// 보정 사냥점 제거
				SceneMain_Battle.Single.mcsHuntLine.CancelDrawHuntLine();
			}
			else
			{
				// 일반 되돌아가기
				Vector2 vec2NormalDirection = -Direction8.GetNormalByDirection(hlpLast.iDirectionDraw);
				RigidbodyOwn.velocity = fMoveDistance * vec2NormalDirection;
			}

			behaviorOwn.OnChangePosition(transform.position);
		}

		protected void MoveStand()
		{
			RigidbodyOwn.velocity = Vector2.zero;
		}
	}
}
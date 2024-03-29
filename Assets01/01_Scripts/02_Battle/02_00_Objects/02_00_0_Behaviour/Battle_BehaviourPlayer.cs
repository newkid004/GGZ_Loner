using Proto_00_N.GlobalDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Proto_00_N
{
	public class Battle_BehaviourPlayer : Battle_BaseBehaviour
	{
		// Char Ref
		public new Battle_CharacterPlayer characterOwn { get => (Battle_CharacterPlayer)base.characterOwn; }

		// Zone Ref
		public Battle_HuntZone hzPlaced { get; protected set; }									// 위치하고 있는 사냥터
		public HashSet<Battle_HuntLinePoint> hsHlpPlaced { get; protected set; }				// 위치하고 있는 사냥터 외곽
			= new HashSet<Battle_HuntLinePoint>();

		public Battle_HuntZone hzLastPlaced { get; protected set; }								// 마지막으로 위치했던 사냥터
		public Battle_HuntLinePoint hlpLastPlaced { get; protected set; }					   // 마지막으로 위치했던 사냥터 외곽

		// 사냥터 작성 중 터치했던 사냥터, 사냥선 정보 <사냥터, <터치했던 사냥터 중복 저장>>
		public Dictionary<Battle_HuntZone, List<Battle_HuntLinePoint>> dictTouchedZoneInfo { get; protected set; }
			= new Dictionary<Battle_HuntZone, List<Battle_HuntLinePoint>>();

		// Draw Status
		public bool isCharInHuntZone { get; protected set; }				// 사냥터 내부 위치 중
		public bool isCharInHuntZoneOutLine { get; protected set; }			// 사냥터 외곽 위치 중
		public bool isDownHuntLineBtn { get; protected set; }				// 사냥선 버튼 입력 중
		public bool isCharDrawingHuntLine { get; set; }						// 사냥선 그리는 중

		public override void Init()
		{
			base.Init();
		}

		public void OnToggleHuntLine(Toggle tgLine)
		{
			bool isOn = tgLine != null ? tgLine.isOn : !isDownHuntLineBtn;

			isDownHuntLineBtn = isOn;

			bool isTrigger = isDownHuntLineBtn || isCharDrawingHuntLine;

			// 사냥터 내 사냥선 충돌 전환
			hzPlaced.hlcEdge.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger);
			hzPlaced.hsHlcHoles.ForEach(hlcHole => hlcHole.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger));
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawed:
					{
						Battle_HuntLinePoint hlpCollide = collision.collider.GetComponent<Battle_HuntLinePoint>();

						if (hlpCollide == null)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline))
						{
							// 사냥터 외곽
							if (isCharDrawingHuntLine)
							{
								// 사냥터 외부 ( 터치 )
								Battle_HuntZone hzTouched = hlpCollide.hlContainer.hZone;
								List<Battle_HuntLinePoint> listTouchPoint = dictTouchedZoneInfo.GetSafe(hzTouched);

								if (isDownHuntLineBtn)
								{
									listTouchPoint.Add(hlpCollide);

									// 선 추가 : 방향 보정
									// OnChangeDirection();
								}
								else
								{
									if (listTouchPoint.Remove(hlpCollide) && listTouchPoint.Count == 0)
									{
										dictTouchedZoneInfo.Remove(hzTouched);
									}
								}
							}
							else
							{
								// 사냥터 내부
								hsHlpPlaced.Add(hlpCollide);
								hlpLastPlaced = hlpCollide;
							}
						}
						else if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
						{
							// 사냥터 구멍
							if (false == isCharDrawingHuntLine)
							{
								// 사냥터 내부
								hsHlpPlaced.Add(hlpCollide);
								hlpLastPlaced = hlpCollide;
							}
						}
					}
					break;
			}
		}

		private void OnCollisionExit2D(Collision2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawed:
					{
						Battle_HuntLinePoint hlpCollide = collision.collider.GetComponent<Battle_HuntLinePoint>();

						if (hlpCollide == null || hlpCollide.iObjectID == 0)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline))
						{
							if (isCharDrawingHuntLine)
							{
								// 사냥터 외부 ( 터치 )
								Battle_HuntZone hzTouched = hlpCollide.hlContainer.hZone;
								List<Battle_HuntLinePoint> listTouchPoint = dictTouchedZoneInfo.GetSafe(hzTouched);

								if (isDownHuntLineBtn)
								{
									listTouchPoint.Add(hlpCollide);

								}
								else
								{
									if (listTouchPoint.Remove(hlpCollide) && listTouchPoint.Count == 0)
									{
										dictTouchedZoneInfo.Remove(hzTouched);
									}
								}
							}
							else
							{
								// 사냥터 내부에서 사냥선과 떨어짐
								OnExitHuntZoneOutline(hlpCollide);
							}
						}
						else if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
						{
							if (false == isCharDrawingHuntLine)
							{
								// 사냥터 내부에서 사냥선과 떨어짐
								OnExitHuntZoneOutline(hlpCollide);
							}
						}
					}
					break;
			}
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawings:
					{
						Battle_HuntLinePoint hlpCollide = collision.GetComponent<Battle_HuntLinePoint>();

						if (hlpCollide == null)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntLine))
						{
							OnEnterHuntline(hlpCollide);
						}
					}
					break;

				case CollideLayer.HuntLineDrawed:
					{
						Battle_HuntLinePoint hlpCollide = collision.GetComponent<Battle_HuntLinePoint>();

						if (hlpCollide == null || hlpCollide.iObjectID == 0)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, 
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline |
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
						{
							OnEnterHuntZoneOutline(collision.ClosestPoint(transform.position), hlpCollide);

							if (hlpCollide.hlContainer != null && hlpCollide.hlContainer.hZone != null && hlpCollide.hlContainer.hZone != hzLastPlaced)
							{
								// 예외처리....
							}
							else
							{
								if (hlpCollide.iObjectID != 0)
								{
									hsHlpPlaced.Add(hlpCollide);
								}
							}

							hlpLastPlaced = hlpCollide;
							isCharInHuntZoneOutLine = true;
						}
					}
					break;
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawed:
					{
						Battle_HuntLinePoint hlpCollide = collision.GetComponent<Battle_HuntLinePoint>();

						if (hlpCollide == null || hlpCollide.iObjectID == 0)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType,
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline |
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
						{
							OnExitHuntZoneOutline(hlpCollide);

							if (false == isCharInHuntZoneOutLine)
							{
								// 방향(noraml) 비교 후 사냥선 작성 시작
								Direction8.JoinState eJoinState = Direction8.GetJoinState(characterOwn.iDirection, hlpCollide.iDirectionNormal);

								switch (eJoinState)
								{
									case Direction8.JoinState.Own:
									case Direction8.JoinState.Diagonal:
										// case Direction8.JoinState.Perpendicular:
										{
											// 외부로 이동
											if (isDownHuntLineBtn && false == isCharDrawingHuntLine)
											{
												// 사냥선 작성 위치 계산
												Vector2 vec2PlayerPos = transform.position;
												Vector2 vec2PlayerBackPos = vec2PlayerPos - (Direction8.GetNormalByDirection(characterOwn.iDirection) * characterOwn.csStatBasic.fMoveSpeed);

												if (GlobalUtility.PPhysics.CollideLineLine(vec2PlayerPos, vec2PlayerBackPos, hlpCollide.transform.position, hlpCollide.PointPrev.transform.position, out Vector2 vec2CreatePosition))
												{
													// 사냥선 작성 시작
													SceneMain_Battle.Single.mcsHuntLine.StartDrawHuntLine(vec2CreatePosition);

													// 퇴장 처리
													OnExitHuntZone();
													isCharDrawingHuntLine = true;
												}
											}
										}
										break;

									default:
										{
											// 내부로 이동
										}
										break;
								}
							}
						}
					} 
					break;
			}
		}

		public void OnEnterHuntZone(Battle_HuntZone hZone)
		{
			// 이전 사냥터 내 사냥선 트리거 비활성화
			if (hzPlaced != null && hzPlaced != hZone)
			{
				hzPlaced.hlcEdge.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = false);
				hzPlaced.hsHlcHoles.ForEach(hlcHole => hlcHole.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = false));
			}

			// 입장 처리
			Battle_HuntZone hzPrev = hzPlaced;
			hzPlaced = hZone;
			hzLastPlaced = hZone;

			isCharInHuntZone = true;

			// 현재 사냥터 내 사냥선 트리거 설정
			bool isTrigger = isDownHuntLineBtn;
			hzPlaced.hlcEdge.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger);
			hzPlaced.hsHlcHoles.ForEach(hlcHole => hlcHole.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger));
		}

		public void OnExitHuntZone()
		{
			isCharInHuntZone = false;

			if (hlpLastPlaced == null || hlpLastPlaced.iOwnSequenceID == 0)
			{
				characterOwn.ProcessLossLife();
			}
		}

		public void OnEnterHuntline(Battle_HuntLinePoint hlpCollide)
		{
			Battle_HuntLinePoint hlpDrawing = SceneMain_Battle.Single.mcsHuntLine.hlpDrawing;

			// 플레이어가 그리는 중인 사냥 끝 선과 동일할 경우 무시
			if (hlpCollide == hlpDrawing)
				return;

			if (isDownHuntLineBtn)
			{
				// 사냥선 작성 중
				if (hlpDrawing.PointPrev == null || hlpDrawing.PointPrev == hlpCollide)
					return;

				// 생성 : 사냥선을 따라 사냥터 생성
				if (CollidePlayer2Huntline(hlpCollide, false, out Vector2 vec2CreatePosition))
				{
					Battle_HuntZone hzCreated = SceneMain_Battle.Single.mcsHuntZone.ContactHuntLine(vec2CreatePosition, hlpCollide);

					hsHlpPlaced.Add(hlpCollide);
					hlpLastPlaced = hlpCollide;

					OnEnterHuntZone(hzCreated);

					// 방향(noraml) 비교
					Direction8.JoinState eJoinState = Direction8.GetJoinState(characterOwn.iDirection, hlpCollide.iDirectionNormal);

					switch (eJoinState)
					{
						case Direction8.JoinState.Obtuse:
						case Direction8.JoinState.Inverse:
							{
								// 입장 처리
								isCharDrawingHuntLine = false;
							}
							break;

						case Direction8.JoinState.Own:
						case Direction8.JoinState.Diagonal:
							{
								// 이어그리기 처리
								OnExitHuntZone();

								// 사냥선 작성 시작
								SceneMain_Battle.Single.mcsHuntLine.StartDrawHuntLine(vec2CreatePosition);
							}
							break;
					}
#if _debug
					Debug.Log(
						$"Battle_BehaviourPlayer : OnEnterHuntline\n" +
						$"ContactHuntLine { hlpCollide.iOwnSequenceID }");
#endif
				}
			}
			else
			{
				// 사냥선 되돌아가기 중
				// int i = 0;
			}
		}

		public void OnEnterHuntZoneOutline(Vector2 vec2ContectPoint, Battle_HuntLinePoint hlpCollide)
		{
			Battle_HuntLinePoint hlpDrawing = SceneMain_Battle.Single.mcsHuntLine.hlpDrawing;

			// 플레이어가 그리는 중인 사냥 끝 선과 동일할 경우 무시
			if (hlpCollide == hlpDrawing)
				return;

			if (isCharDrawingHuntLine)
			{
				if (false == isDownHuntLineBtn)
				{
					// 사냥터 되돌아오기 : 작성중인 사냥터가 있었다면 제거
					Battle_HuntLinePoint hlpLastDrawingPoint = null;

					while (SceneMain_Battle.Single.mcsHuntLine.hlpDrawing != null)
					{
						hlpLastDrawingPoint = SceneMain_Battle.Single.mcsHuntLine.hlpDrawing;
						SceneMain_Battle.Single.mcsHuntLine.CancelDrawHuntLine();
					}

					// 입장 처리
					OnEnterHuntZone(hzLastPlaced);
					isCharDrawingHuntLine = false;

					// 입장 보정
					Vector2 vec2PlayerDirection = Direction8.GetNormalByDirection(characterOwn.iDirection);
					CollidePlayer2Huntline(hlpCollide, true, out Vector2 vec2CreatePosition);
					transform.position = vec2CreatePosition + vec2PlayerDirection * (-0.025f);
				}
				else
				{
					if (hlpCollide.hlContainer.hZone == hzLastPlaced)
					{
						if (hlpCollide.hlContainer == hzLastPlaced.hlcEdge)
						{
							// 확장 : 사냥선에 인접한 사냥터에 의해 사냥터 생성
							SceneMain_Battle.Single.mcsHuntZone.ContactHuntZone(vec2ContectPoint, hlpCollide);

							// 입장 처리
							OnEnterHuntZone(hzLastPlaced);
							isCharDrawingHuntLine = false;
						}
						else
						{
							// 확장 : 사냥터 내 구멍 메우기

						}
					}
					else
					{
						// 터치 처리
					}
				}
			}
			else
			{
				// 사냥터 내부에서 사냥선 외곽 충돌
			}
#if _debug
			Debug.Log(
				$"Battle_BehaviourPlayer : OnEnterHuntZoneOutline\n" +
				$"ContactHuntZone { hlpCollide.iOwnSequenceID }");
#endif
		}

		public void OnExitHuntZoneOutline(Battle_HuntLinePoint hlpOutline)
		{
			hsHlpPlaced.Remove(hlpOutline);
			isCharInHuntZoneOutLine = 0 < hsHlpPlaced.Count;
#if _debug
			Debug.Log($"ExitHuntZone { hlpOutline.iOwnSequenceID }");
#endif
		}

		public override void OnChangeDirection(int iBefore, int iAfter, int iBeforeDirect)
		{
			if (isCharDrawingHuntLine)
			{
				// 사냥선 그리는 중 : 선 추가
				SceneMain_Battle.Single.mcsHuntLine.ChangeHuntLineDirection();
			}
		}

		public override void OnChangePosition(Vector2 vec2Pos)
		{
			if (isCharDrawingHuntLine)
			{
				// 사냥선 그리는 중
				SceneMain_Battle.Single.mcsHuntLine.hlpDrawing.vec2ToPoint =  vec2Pos;
			}
		}

		private void Update()
		{
#if _debug
			if (Input.GetKeyDown(KeyCode.A))
			{
				string str = "Place Point : ";

				foreach (var item in hsHlpPlaced)
				{
					str += $"{ item.iObjectID } ";
				}
				Debug.Log(str);
			}
#endif
		}

		private bool CollidePlayer2Huntline(Battle_HuntLinePoint hlpCollide, bool isReturn, out Vector2 vec2Collide)
		{
			Vector2 vec2PlayerDirection = Direction8.GetNormalByDirection(characterOwn.iDirection);
			if (isReturn)
				vec2PlayerDirection *= -1f;

			Vector2 vec2PlayerPos = transform.position;
			vec2PlayerPos -= vec2PlayerDirection * characterOwn.csStatBasic.fMoveSpeed;
			Vector2 vec2PlayerNextPos = vec2PlayerPos + (vec2PlayerDirection * characterOwn.csStatBasic.fMoveSpeed * 2f);

			bool isCollide = GlobalUtility.PPhysics.CollideLineLine(vec2PlayerPos, vec2PlayerNextPos, hlpCollide.transform.position, hlpCollide.PointPrev.transform.position, out vec2Collide);

			return isCollide;
		}
	}
}
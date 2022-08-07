using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_BhvPlayerHuntline
	{
		private Battle_BehaviourPlayer bhvPlayer;

		public Battle_BhvPlayerHuntline(Battle_BehaviourPlayer bhvPlayer)
		{
			this.bhvPlayer = bhvPlayer;
		}

		// Zone Ref
		public Battle_HZone hzPlaced { get; protected set; }							// ��ġ�ϰ� �ִ� �����
		public HashSet<Battle_HPoint> hsHlpPlaced { get; protected set; }				// ��ġ�ϰ� �ִ� ����� �ܰ�
			= new HashSet<Battle_HPoint>();

		public Battle_HZone hzLastPlaced { get; protected set; }								// ���������� ��ġ�ߴ� �����
		public Battle_HPoint hlpLastPlaced { get; protected set; }					   // ���������� ��ġ�ߴ� ����� �ܰ�

		// ����� �ۼ� �� ��ġ�ߴ� �����, ��ɼ� ���� <�����, <��ġ�ߴ� ����� �ߺ� ����>>
		public Dictionary<Battle_HZone, List<Battle_HPoint>> dictTouchedZoneInfo { get; protected set; }
			= new Dictionary<Battle_HZone, List<Battle_HPoint>>();

		// Draw Status
		public bool isCharInHuntZone { get; protected set; }				// ����� ���� ��ġ ��
		public bool isCharInHuntZoneOutLine { get; protected set; }			// ����� �ܰ� ��ġ ��
		public bool isDownHuntLineBtn { get; protected set; }				// ��ɼ� ��ư �Է� ��
		public bool isCharDrawingHuntLine { get; set; }						// ��ɼ� �׸��� ��

		public void OnToggleHuntLine(Toggle tgLine)
		{
			bool isOn = tgLine != null ? tgLine.isOn : !isDownHuntLineBtn;

			isDownHuntLineBtn = isOn;

			bool isTrigger = isDownHuntLineBtn || isCharDrawingHuntLine;

			// ����� �� ��ɼ� �浹 ��ȯ
			hzPlaced.lineEdge.listPoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger);
			hzPlaced.hsHlcHoles.ForEach(hlcHole => hlcHole.listPoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger));
		}

		public void OnCollisionEnter2D(Collision2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawed:
				{
					Battle_HPoint hlpCollide = collision.collider.GetComponent<Battle_HPoint>();

					if (hlpCollide == null)
						return;

					if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, ObjectData.ObjectType.ciHuntZoneOutline))
					{
						// ����� �ܰ�
						if (isCharDrawingHuntLine)
						{
							// ����� �ܺ� ( ��ġ )
							Battle_HZone hzTouched = hlpCollide.hLine.hZone;
							List<Battle_HPoint> listTouchPoint = dictTouchedZoneInfo.GetSafe(hzTouched);

							if (isDownHuntLineBtn)
							{
								listTouchPoint.Add(hlpCollide);

								// �� �߰� : ���� ����
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
							// ����� ����
							hsHlpPlaced.Add(hlpCollide);
							hlpLastPlaced = hlpCollide;
						}
					}
					else if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
					{
						// ����� ����
						if (false == isCharDrawingHuntLine)
						{
							// ����� ����
							hsHlpPlaced.Add(hlpCollide);
							hlpLastPlaced = hlpCollide;
						}
					}
				}
				break;
			}
		}

		public void OnCollisionExit2D(Collision2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawed:
					{
						Battle_HPoint hlpCollide = collision.collider.GetComponent<Battle_HPoint>();

						if (hlpCollide == null || hlpCollide.iObjectID == 0)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline))
						{
							if (isCharDrawingHuntLine)
							{
								// ����� �ܺ� ( ��ġ )
								Battle_HZone hzTouched = hlpCollide.hLine.hZone;
								List<Battle_HPoint> listTouchPoint = dictTouchedZoneInfo.GetSafe(hzTouched);

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
								// ����� ���ο��� ��ɼ��� ������
								OnExitHuntZoneOutline(hlpCollide);
							}
						}
						else if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, ObjectData.ObjectType.ciHuntZoneHole))
						{
							if (false == isCharDrawingHuntLine)
							{
								// ����� ���ο��� ��ɼ��� ������
								OnExitHuntZoneOutline(hlpCollide);
							}
						}
					}
					break;
			}
		}

		public void OnTriggerEnter2D(Collider2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawings:
					{
						Battle_HPoint hlpCollide = collision.GetComponent<Battle_HPoint>();

						if (hlpCollide == null)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, ObjectData.ObjectType.ciHuntLine))
						{
							OnEnterHuntline(hlpCollide);
						}
					}
					break;

				case CollideLayer.HuntLineDrawed:
					{
						Battle_HPoint hlpCollide = collision.GetComponent<Battle_HPoint>();

						if (hlpCollide == null || hlpCollide.iObjectID == 0)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, 
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline |
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
						{
							OnEnterHuntZoneOutline(collision.ClosestPoint(bhvPlayer.characterOwn.transform.position), hlpCollide);

							if (hlpCollide.hLine != null && hlpCollide.hLine.hZone != null && hlpCollide.hLine.hZone != hzLastPlaced)
							{
								// ����ó��....
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

		public void OnTriggerExit2D(Collider2D collision)
		{
			switch (collision.gameObject.layer)
			{
				case CollideLayer.HuntLineDrawed:
					{
						Battle_HPoint hlpCollide = collision.GetComponent<Battle_HPoint>();

						if (hlpCollide == null || hlpCollide.iObjectID == 0)
							return;

						if (GlobalUtility.Digit.Include(hlpCollide.iObjectType,
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline |
								GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
						{
							OnExitHuntZoneOutline(hlpCollide);

							if (false == isCharInHuntZoneOutLine)
							{
								// ����(noraml) �� �� ��ɼ� �ۼ� ����
								Direction8.EJoinState eJoinState = Direction8.GetJoinState(bhvPlayer.characterOwn.iDirection, hlpCollide.iDirectionNormal);

								switch (eJoinState)
								{
									case Direction8.EJoinState.Own:
									case Direction8.EJoinState.Diagonal:
										// case Direction8.JoinState.Perpendicular:
										{
											// �ܺη� �̵�
											if (isDownHuntLineBtn && false == isCharDrawingHuntLine)
											{
												// ��ɼ� �ۼ� ��ġ ���
												Vector2 vec2PlayerPos = bhvPlayer.characterOwn.transform.position;
												Vector2 vec2PlayerBackPos = vec2PlayerPos - (Direction8.GetNormalByDirection(bhvPlayer.characterOwn.iDirection) * bhvPlayer.characterOwn.csStatBasic.fMoveSpeed);

												if (GlobalUtility.PPhysics.CollideLineLine(vec2PlayerPos, vec2PlayerBackPos, hlpCollide.transform.position, hlpCollide.PointPrevCircle.transform.position, out Vector2 vec2CreatePosition))
												{
													// ��ɼ� �ۼ� ����
													SceneMain_Battle.Single.mcsHLine.StartDrawLine(vec2CreatePosition, bhvPlayer.characterOwn.iDirection);

													// ���� ó��
													OnExitHuntZone();
													isCharDrawingHuntLine = true;
												}
											}
										}
										break;

									default:
										{
											// ���η� �̵�
										}
										break;
								}
							}
						}
					} 
					break;
			}
		}

		public void OnEnterHuntZone(Battle_HZone hZone)
		{
			// ���� ����� �� ��ɼ� Ʈ���� ��Ȱ��ȭ
			if (hzPlaced != null && hzPlaced != hZone)
			{
				hzPlaced.lineEdge.listPoint.ForEach(hlp => hlp.colEdge.isTrigger = false);
				hzPlaced.hsHlcHoles.ForEach(hlcHole => hlcHole.listPoint.ForEach(hlp => hlp.colEdge.isTrigger = false));
			}

			// ���� ó��
			Battle_HZone hzPrev = hzPlaced;
			hzPlaced = hZone;
			hzLastPlaced = hZone;

			isCharInHuntZone = true;

			// ���� ����� �� ��ɼ� Ʈ���� ����
			bool isTrigger = isDownHuntLineBtn;
			hzPlaced.lineEdge.listPoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger);
			hzPlaced.hsHlcHoles.ForEach(hlcHole => hlcHole.listPoint.ForEach(hlp => hlp.colEdge.isTrigger = isTrigger));
		}

		public void OnExitHuntZone()
		{
			isCharInHuntZone = false;

			if (hlpLastPlaced == null || hlpLastPlaced.iOwnSequenceID == 0)
			{
				bhvPlayer.characterOwn.ProcessLossLife();
			}
		}

		public void OnEnterHuntline(Battle_HPoint hlpCollide)
		{
			Battle_HPoint hlpDrawing = SceneMain_Battle.Single.mcsHLine.nowDrawingPoint;

			// �÷��̾ �׸��� ���� ��� �� ���� ������ ��� ����
			if (hlpCollide == hlpDrawing)
				return;

			if (isDownHuntLineBtn)
			{
				// ��ɼ� �ۼ� ��
				if (hlpDrawing.PointPrev == null || hlpDrawing.PointPrev == hlpCollide)
					return;

				// ���� : ��ɼ��� ���� ����� ����
				if (CollidePlayer2Huntline(hlpCollide, false, out Vector2 vec2CreatePosition))
				{
					Battle_HZone hzCreated = SceneMain_Battle.Single.mcsHZone.ContactLine(vec2CreatePosition, hlpCollide);

					hsHlpPlaced.Add(hlpCollide);
					hlpLastPlaced = hlpCollide;

					OnEnterHuntZone(hzCreated);

					// ����(noraml) ��
					Direction8.EJoinState eJoinState = Direction8.GetJoinState(bhvPlayer.characterOwn.iDirection, hlpCollide.iDirectionNormal);

					switch (eJoinState)
					{
						case Direction8.EJoinState.Obtuse:
						case Direction8.EJoinState.Inverse:
							{
								// ���� ó��
								isCharDrawingHuntLine = false;
							}
							break;

						case Direction8.EJoinState.Own:
						case Direction8.EJoinState.Diagonal:
							{
								// �̾�׸��� ó��
								hsHlpPlaced.Remove(hlpCollide);
								OnExitHuntZone();

								// ��ɼ� �ۼ� ����
								SceneMain_Battle.Single.mcsHLine.StartDrawLine(vec2CreatePosition, bhvPlayer.characterOwn.iDirection);
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
				// ��ɼ� �ǵ��ư��� ��
				// int i = 0;
			}
		}

		public void OnEnterHuntZoneOutline(Vector2 vec2ContectPoint, Battle_HPoint hlpCollide)
		{
			Battle_HPoint hlpDrawing = SceneMain_Battle.Single.mcsHLine.nowDrawingPoint;

			// �÷��̾ �׸��� ���� ��� �� ���� ������ ��� ����
			if (hlpCollide == hlpDrawing)
				return;

			if (isCharDrawingHuntLine)
			{
				if (false == isDownHuntLineBtn)
				{
					// ����� �ǵ��ƿ��� : �ۼ����� ����Ͱ� �־��ٸ� ����
					Battle_HPoint hlpLastDrawingPoint = null;

					while (SceneMain_Battle.Single.mcsHLine.nowDrawingPoint != null)
					{
						hlpLastDrawingPoint = SceneMain_Battle.Single.mcsHLine.nowDrawingPoint;
						SceneMain_Battle.Single.mcsHLine.CancelDrawPoint();
					}

					// ���� ó��
					OnEnterHuntZone(hzLastPlaced);
					isCharDrawingHuntLine = false;

					// ���� ����
					Vector2 vec2PlayerDirection = Direction8.GetNormalByDirection(bhvPlayer.characterOwn.iDirection);
					CollidePlayer2Huntline(hlpCollide, true, out Vector2 vec2CreatePosition);
					bhvPlayer.characterOwn.transform.position = vec2CreatePosition + vec2PlayerDirection * (-0.025f);
				}
				else
				{
					if (false == isCharInHuntZone)
					{
						if (hlpCollide.hLine.hZone == hzLastPlaced)
						{
							if (hlpCollide.hLine == hzLastPlaced.lineEdge)
							{
								// Ȯ�� : ��ɼ��� ������ ����Ϳ� ���� ����� ����
								SceneMain_Battle.Single.mcsHZone.ContactZone(vec2ContectPoint, hlpCollide);

								// ���� ó��
								OnEnterHuntZone(hzLastPlaced);
								isCharDrawingHuntLine = false;
							}
							else
							{
								// Ȯ�� : ����� �� ���� �޿��

							}
						}
						else
						{
							// ��ġ ó��
						}
					}
				}
			}
			else
			{
				// ����� ���ο��� ��ɼ� �ܰ� �浹
			}
#if _debug
			Debug.Log(
				$"Battle_BehaviourPlayer : OnEnterHuntZoneOutline\n" +
				$"ContactHuntZone { hlpCollide.iOwnSequenceID }");
#endif
		}

		public void OnExitHuntZoneOutline(Battle_HPoint hlpOutline)
		{
			if (hsHlpPlaced.Remove(hlpOutline))
			{
				isCharInHuntZoneOutLine = 0 < hsHlpPlaced.Count;
#if _debug
				Debug.Log($"ExitHuntZone { hlpOutline.iOwnSequenceID }");
#endif
			}
		}

		public void OnChangeDirection(int iBefore, int iAfter, int iBeforeDirect)
		{
			if (isCharDrawingHuntLine)
			{
				// ��ɼ� �׸��� �� : �� �߰�
				SceneMain_Battle.Single.mcsHLine.ChangeLineDirection();
			}
		}

		public void OnChangePosition(Vector2 vec2Pos)
		{
			if (isCharDrawingHuntLine)
			{
				// ��ɼ� �׸��� ��
				SceneMain_Battle.Single.mcsHLine.nowDrawingPoint.PosWorld = vec2Pos;
			}
		}

		public void Update()
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

		public bool CollidePlayer2Huntline(Battle_HPoint hlpCollide, bool isReturn, out Vector2 vec2Collide)
		{
			Vector2 vec2PlayerDirection = Direction8.GetNormalByDirection(bhvPlayer.characterOwn.iDirection);
			if (isReturn)
				vec2PlayerDirection *= -1f;

			Vector2 vec2PlayerPos = bhvPlayer.characterOwn.transform.position;
			vec2PlayerPos -= vec2PlayerDirection * bhvPlayer.characterOwn.csStatBasic.fMoveSpeed;
			Vector2 vec2PlayerNextPos = vec2PlayerPos + (vec2PlayerDirection * bhvPlayer.characterOwn.csStatBasic.fMoveSpeed * 2f);

			bool isCollide = GlobalUtility.PPhysics.CollideLineLine(vec2PlayerPos, vec2PlayerNextPos, hlpCollide.transform.position, hlpCollide.PointPrev.transform.position, out vec2Collide);

			return isCollide;
		}
	}
}
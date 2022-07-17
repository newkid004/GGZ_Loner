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
		public Battle_HuntZone hzPlaced { get; protected set; }									// ��ġ�ϰ� �ִ� �����
		public HashSet<Battle_HuntLinePoint> hsHlpPlaced { get; protected set; }				// ��ġ�ϰ� �ִ� ����� �ܰ�
			= new HashSet<Battle_HuntLinePoint>();

		public Battle_HuntZone hzLastPlaced { get; protected set; }								// ���������� ��ġ�ߴ� �����
		public Battle_HuntLinePoint hlpLastPlaced { get; protected set; }					   // ���������� ��ġ�ߴ� ����� �ܰ�

		// ����� �ۼ� �� ��ġ�ߴ� �����, ��ɼ� ���� <�����, <��ġ�ߴ� ����� �ߺ� ����>>
		public Dictionary<Battle_HuntZone, List<Battle_HuntLinePoint>> dictTouchedZoneInfo { get; protected set; }
			= new Dictionary<Battle_HuntZone, List<Battle_HuntLinePoint>>();

		// Draw Status
		public bool isCharInHuntZone { get; protected set; }				// ����� ���� ��ġ ��
		public bool isCharInHuntZoneOutLine { get; protected set; }			// ����� �ܰ� ��ġ ��
		public bool isDownHuntLineBtn { get; protected set; }				// ��ɼ� ��ư �Է� ��
		public bool isCharDrawingHuntLine { get; set; }						// ��ɼ� �׸��� ��

		public override void Init()
		{
			base.Init();
		}

		public void OnToggleHuntLine(Toggle tgLine)
		{
			bool isOn = tgLine != null ? tgLine.isOn : !isDownHuntLineBtn;

			isDownHuntLineBtn = isOn;

			bool isTrigger = isDownHuntLineBtn || isCharDrawingHuntLine;

			// ����� �� ��ɼ� �浹 ��ȯ
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
							// ����� �ܰ�
							if (isCharDrawingHuntLine)
							{
								// ����� �ܺ� ( ��ġ )
								Battle_HuntZone hzTouched = hlpCollide.hlContainer.hZone;
								List<Battle_HuntLinePoint> listTouchPoint = dictTouchedZoneInfo.GetSafe(hzTouched);

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
								// ����� �ܺ� ( ��ġ )
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
								// ����� ���ο��� ��ɼ��� ������
								OnExitHuntZoneOutline(hlpCollide);
							}
						}
						else if (GlobalUtility.Digit.Include(hlpCollide.iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole))
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
								// ����(noraml) �� �� ��ɼ� �ۼ� ����
								Direction8.JoinState eJoinState = Direction8.GetJoinState(characterOwn.iDirection, hlpCollide.iDirectionNormal);

								switch (eJoinState)
								{
									case Direction8.JoinState.Own:
									case Direction8.JoinState.Diagonal:
										// case Direction8.JoinState.Perpendicular:
										{
											// �ܺη� �̵�
											if (isDownHuntLineBtn && false == isCharDrawingHuntLine)
											{
												// ��ɼ� �ۼ� ��ġ ���
												Vector2 vec2PlayerPos = transform.position;
												Vector2 vec2PlayerBackPos = vec2PlayerPos - (Direction8.GetNormalByDirection(characterOwn.iDirection) * characterOwn.csStatBasic.fMoveSpeed);

												if (GlobalUtility.PPhysics.CollideLineLine(vec2PlayerPos, vec2PlayerBackPos, hlpCollide.transform.position, hlpCollide.PointPrev.transform.position, out Vector2 vec2CreatePosition))
												{
													// ��ɼ� �ۼ� ����
													SceneMain_Battle.Single.mcsHuntLine.StartDrawHuntLine(vec2CreatePosition);

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

		public void OnEnterHuntZone(Battle_HuntZone hZone)
		{
			// ���� ����� �� ��ɼ� Ʈ���� ��Ȱ��ȭ
			if (hzPlaced != null && hzPlaced != hZone)
			{
				hzPlaced.hlcEdge.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = false);
				hzPlaced.hsHlcHoles.ForEach(hlcHole => hlcHole.listLinePoint.ForEach(hlp => hlp.colEdge.isTrigger = false));
			}

			// ���� ó��
			Battle_HuntZone hzPrev = hzPlaced;
			hzPlaced = hZone;
			hzLastPlaced = hZone;

			isCharInHuntZone = true;

			// ���� ����� �� ��ɼ� Ʈ���� ����
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
					Battle_HuntZone hzCreated = SceneMain_Battle.Single.mcsHuntZone.ContactHuntLine(vec2CreatePosition, hlpCollide);

					hsHlpPlaced.Add(hlpCollide);
					hlpLastPlaced = hlpCollide;

					OnEnterHuntZone(hzCreated);

					// ����(noraml) ��
					Direction8.JoinState eJoinState = Direction8.GetJoinState(characterOwn.iDirection, hlpCollide.iDirectionNormal);

					switch (eJoinState)
					{
						case Direction8.JoinState.Obtuse:
						case Direction8.JoinState.Inverse:
							{
								// ���� ó��
								isCharDrawingHuntLine = false;
							}
							break;

						case Direction8.JoinState.Own:
						case Direction8.JoinState.Diagonal:
							{
								// �̾�׸��� ó��
								OnExitHuntZone();

								// ��ɼ� �ۼ� ����
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
				// ��ɼ� �ǵ��ư��� ��
				// int i = 0;
			}
		}

		public void OnEnterHuntZoneOutline(Vector2 vec2ContectPoint, Battle_HuntLinePoint hlpCollide)
		{
			Battle_HuntLinePoint hlpDrawing = SceneMain_Battle.Single.mcsHuntLine.hlpDrawing;

			// �÷��̾ �׸��� ���� ��� �� ���� ������ ��� ����
			if (hlpCollide == hlpDrawing)
				return;

			if (isCharDrawingHuntLine)
			{
				if (false == isDownHuntLineBtn)
				{
					// ����� �ǵ��ƿ��� : �ۼ����� ����Ͱ� �־��ٸ� ����
					Battle_HuntLinePoint hlpLastDrawingPoint = null;

					while (SceneMain_Battle.Single.mcsHuntLine.hlpDrawing != null)
					{
						hlpLastDrawingPoint = SceneMain_Battle.Single.mcsHuntLine.hlpDrawing;
						SceneMain_Battle.Single.mcsHuntLine.CancelDrawHuntLine();
					}

					// ���� ó��
					OnEnterHuntZone(hzLastPlaced);
					isCharDrawingHuntLine = false;

					// ���� ����
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
							// Ȯ�� : ��ɼ��� ������ ����Ϳ� ���� ����� ����
							SceneMain_Battle.Single.mcsHuntZone.ContactHuntZone(vec2ContectPoint, hlpCollide);

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
				// ��ɼ� �׸��� �� : �� �߰�
				SceneMain_Battle.Single.mcsHuntLine.ChangeHuntLineDirection();
			}
		}

		public override void OnChangePosition(Vector2 vec2Pos)
		{
			if (isCharDrawingHuntLine)
			{
				// ��ɼ� �׸��� ��
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
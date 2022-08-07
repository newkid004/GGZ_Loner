using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace GGZ
{
	using GlobalDefine;

	[System.Serializable]
	public class Battle_HZoneManager
	{
		[SerializeField] private ObjectPool<Battle_HZone> oPoolZone = new ObjectPool<Battle_HZone>();

		public Battle_HZone zoneStart { get; private set; }

		[SerializeField] public bool isDebug;

		[Serializable]
		private class DestroyTool
		{
			public PolygonCollider2D colDestroyCaster;
			[HideInInspector] public Collider2D[] colCastResult = new Collider2D[64];
			[HideInInspector] public ContactFilter2D contactFilter = new ContactFilter2D();
			[HideInInspector] public Battle_HLine[] hlResult = new Battle_HLine[64];

			[HideInInspector] public Clipper clipper = new Clipper();
		}
		[SerializeField] private DestroyTool destroyTool;

		public void Init()
		{
			oPoolZone.Init();

			destroyTool.contactFilter.useTriggers = true;
			destroyTool.contactFilter.useLayerMask = true;
			destroyTool.contactFilter.SetLayerMask(CollideLayer.HuntZoneEdge);

			CreateStartZone();
		}

		public Battle_HZone GetObject(int iSequenceID) => oPoolZone.GetObject(iSequenceID);

		public Battle_HZone PopObj(Transform trParent = null)
		{
			Battle_HZone hZone = oPoolZone.Pop(trParent);
			hZone.ReconnectRefSelf();

			return hZone;
		} // Push�� �����Ǹ� ������ �ʿ� ����

		public Battle_HZone CreateZone(List<Vector2> listPoint)
		{
			Battle_HZone hZone = PopObj();
			Battle_HLine hLine = SceneMain_Battle.Single.mcsHLine.PopLine();

			hZone.transform.position = listPoint[0];
			hLine.transform.parent = hZone.transform;
			hLine.transform.localPosition = Vector2.zero;

			listPoint.ForEach(vec2 =>
			{
				hLine.AddLinePoint(vec2);
			});

			SetEdgeToHuntZone(hZone, hLine, false);
			SetZoneSpace(hZone);

			return hZone;
		}

		public Battle_HZone CreateZone(Battle_HLine hlc)
		{
			Battle_HZone hZone = PopObj();

			hZone.transform.localPosition = hlc.transform.localPosition;

			SetEdgeToHuntZone(hZone, hlc, false);
			SetZoneSpace(hZone);
		
			return hZone;
		}

		public void DestroyZone(List<List<Vector2>> listPaths)
		{
			int iPathCount = listPaths.Count;

			destroyTool.colDestroyCaster.pathCount = iPathCount;
			
			for (int i = 0; i < iPathCount; i++)
			{
				destroyTool.colDestroyCaster.SetPath(i, listPaths[i]);
			}

			List<List<IntPoint>> listIntPath = GlobalUtility.Clipper.GetPathToPolygon(destroyTool.colDestroyCaster);
			List<List<IntPoint>> listExecuteResult = new List<List<IntPoint>>();

			bool isProcessedInside = false;
			bool isProcessedContact = false;
			bool isProcessedPunch = false;

			oPoolZone.LoopOnActive(hZone =>
			{
				bool isInside = false;
				bool isContact = false;
				bool isPunch = false;

				int iPunchIndex = 0;

				Battle_HLine hLine = hZone.lineEdge;

				List<Vector2> listLinePath = new List<Vector2>();
				int iPointCount = hZone.lineEdge.colPoly.GetWorldPath(0, listLinePath);

				// �ı����� ���ο� ����Ͱ� ��ġ�ϴ��� Ȯ��
				for (int i = 0; i < listPaths.Count && isContact == false; i++)
				{
					int iInsideCount = GlobalUtility.PPhysics.ContainPolyPoly(listPaths[i], listLinePath, true);

					if (iInsideCount == iPointCount)
					{
						isInside = true;
						break;
					}
					else if (0 < iInsideCount)
					{
						isContact = true;
						break;
					}
					else
					{
						// ����� ���ο� �ı������� �ִ��� Ȯ��
						iInsideCount = GlobalUtility.PPhysics.ContainPolyPoly(listLinePath, listPaths[i], true);
						if (iInsideCount == listPaths[i].Count)
						{
							iPunchIndex = i;
							isPunch = true;

							if (GlobalUtility.PPhysics.CollidePolyPoly(listLinePath, listPaths[i], null))
							{
								isContact = true;
							}

							break;
						}
						else if (0 < iInsideCount)
						{
							isContact = true;
							break;
						}
					}
				}

				if (isInside)
				{
					// �ش� ����� ����
					hZone.Push();
				}

				if (isContact)
				{
					// �ش� ����� �ı� ����
					destroyTool.clipper.Clear();
					listExecuteResult.Clear();
					
					destroyTool.clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(hLine.colPoly), PolyType.ptSubject, true);
					destroyTool.clipper.AddPaths(listIntPath, PolyType.ptClip, true);

					// �ı� ������ ��ġ�� ������ �ִ��� Ȯ��
					List<Battle_HLine> listHole = new List<Battle_HLine>();
					List<Vector2> listHolePath = new List<Vector2>();
					hZone.hsHlcHoles.ForEach(hlcHole =>
					{
						hlcHole.colPoly.GetWorldPath(0, listHolePath);

						// �ı����� ���ο� ������ ��ġ�ϴ��� Ȯ��
						for (int i = 0; i < listPaths.Count; i++)
						{
							int iInsideCount = GlobalUtility.PPhysics.ContainPolyPoly(listPaths[i], listHolePath, true);

							if (0 < iInsideCount)
							{
								destroyTool.clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(hlcHole.colPoly), PolyType.ptSubject, true);

								listHole.Add(hlcHole);
								break;
							}
						}
					});

					destroyTool.clipper.Execute(ClipType.ctDifference, listExecuteResult);

					// ���ƴ� ���� ����
					listHole.ForEach(hlcHole =>
					{
						hlcHole.Push();
						hZone.hsHlcHoles.Remove(hlcHole);
					});

					int iResultCount = listExecuteResult.Count;

					for (int i = 0; i < iResultCount; ++i)
					{
						List<IntPoint> listPath = listExecuteResult[i];

						int iObjectTypeSave = hLine.iObjectType;

						if (i == 0)
						{
							GlobalUtility.Clipper.SetPolygonToPathWithZero(listPath, hLine.colPoly);
							RefreshLineToPolyCollider(hLine);

							hLine.hZone = hZone;
							hLine.ApplyEdge(iObjectTypeSave != GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline);

							hLine.hZone.transform.PositionOnlyThisParent(hLine.transform.position);

							SetEdgeToHuntZone(hLine.hZone, hLine, false);
							SetZoneSpace(hLine.hZone);
						}
						else
						{
							// �ı��� ���� ����� ������
							Battle_HLine hLineSplit = SceneMain_Battle.Single.mcsHLine.PopLine();
							GlobalUtility.Clipper.SetPolygonToPathWithZero(listPath, hLineSplit.colPoly);
							RefreshLineToPolyCollider(hLineSplit);

							CreateZone(hLineSplit);
						}
					}
				}

				if (isPunch)
				{
					// ����� ������ ���� ����
					destroyTool.clipper.Clear();
					listExecuteResult.Clear();

					destroyTool.clipper.AddPath(listIntPath[iPunchIndex], PolyType.ptSubject, true);
					hZone.hsHlcHoles.ForEach(hlcHole =>
						destroyTool.clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(hlcHole.colPoly), PolyType.ptClip, true));

					destroyTool.clipper.Execute(ClipType.ctUnion, listExecuteResult);

					hZone.hsHlcHoles.ForEach(hlcHole => hlcHole.Push());
					hZone.hsHlcHoles.Clear();

					int iResultCount = listExecuteResult.Count;
					for (int i = 0; i < iResultCount; ++i)
					{
						List<IntPoint> listPath = listExecuteResult[i];
						Battle_HLine hlcHole = SceneMain_Battle.Single.mcsHLine.PopLine();

						GlobalUtility.Clipper.SetPolygonToPathWithZero(listPath, hlcHole.colPoly);
						RefreshLineToPolyCollider(hlcHole);

						// ������ ������ WindingOrder ���
						var eWOrder = GlobalUtility.Trigonometric.GetWindingOrder(listPath, p => new Vector2(p.X, p.Y));

						if (eWOrder == GVar.EWindingOrder.Clockwise)
						{
							// ���ۿ� ���� ����� ����
							CreateZone(hlcHole);
						}
						else if (eWOrder == GVar.EWindingOrder.CounterClockwise)
						{
							// ���� ����
							hlcHole.hZone = hZone;
							hlcHole.ApplyEdge(true);

							SetEdgeToHuntZone(hZone, hlcHole, true);
							hZone.hsHlcHoles.Add(hlcHole);
							hZone.p2mPolygon.holes.Add(hlcHole.listPointPos.ConvertAll<Vector3>(vec => vec));
						}
					}

					SetZoneSpace(hZone);
				}

				isProcessedInside |= isInside;
				isProcessedContact |= isContact;
				isProcessedPunch |= isPunch;
			});

			// �ʵ� Polygon ����
			if (isProcessedInside || isProcessedContact)
			{
				destroyTool.clipper.Clear();
				listExecuteResult.Clear();

				destroyTool.clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(SceneMain_Battle.Single.mcsField.colFieldTotal), PolyType.ptSubject, true);
				destroyTool.clipper.AddPaths(listIntPath, PolyType.ptClip, true);

				destroyTool.clipper.Execute(ClipType.ctUnion, listExecuteResult);

				GlobalUtility.Clipper.SetPolygonToPath(listExecuteResult, SceneMain_Battle.Single.mcsField.colFieldTotal);
			}
		}

		private void RefreshLineToPolyCollider(Battle_HLine hLine)
		{
			hLine.Reset();

			List<Vector2> listPoint = new List<Vector2>();
			hLine.colPoly.GetPath(0, listPoint);
			hLine.ReplaceLinePoint(listPoint);
		}

		private void SetEdgeToHuntZone(Battle_HZone hZone, Battle_HLine hlcEdge, bool isHole)
		{
			// ����Ϳ� ��ü ����
			if (false == isHole)
			{
				hZone.lineEdge = hlcEdge;
			}

			hlcEdge.hZone = hZone;
			hlcEdge.transform.parent = hZone.transform;
			hlcEdge.ApplyEdge(isHole);

			SetNormalVectorAboutEdge(hlcEdge, isHole);
		}

		private void SetNormalVectorAboutEdge(Battle_HLine hlcEdge, bool isHole)
		{
			// ����� �ܰ��� ���� �Է�
			GVar.EWindingOrder eWindingOrder = hlcEdge.eWindingOrder;

			if (isHole)
			{
				eWindingOrder = (GVar.EWindingOrder)((int)eWindingOrder).ModStep(1, 2);
			}

			// ����� �ܰ��� ���� �ʱⰪ ����
			int iIndex;
			int iStep;
			int iDest;
			float fNormalDirection;
			if (eWindingOrder == GVar.EWindingOrder.Clockwise)
			{
				iIndex = 0;
				iStep = 1;
				iDest = hlcEdge.listPoint.Count;
				fNormalDirection = 90f;
			}
			else
			{
				iIndex = hlcEdge.listPoint.Count - 1;
				iStep = -1;
				iDest = -1;
				fNormalDirection = -90;
			}
#if _debug
			if (iIndex == iDest)
			{
				// ������ �ܰ����� �������� ����
				Debug.LogAssertion("Invalid Huntline edge");
			}
#endif
			// �ܰ��� ���� �Է�
			while (iIndex != iDest)
			{
				Battle_HPoint hlp = hlcEdge.listPoint[iIndex];
				hlp.vec2pNormal = hlp.vec2Interval.Rotate(fNormalDirection).normalized;
				hlp.ApplyHuntZone(isHole);

				iIndex += iStep;
			}
		}

		private void SetZoneSpace(Battle_HZone hZone)
		{
			// �ܰ��� ����
			hZone.CalcEdgePoints();

			// �޽� ����
			hZone.CalcMeshZone();

			// �߽��� �Է�
			int iEdgeCount = hZone.lineEdge.listPoint.Count;

			Vector3 vec3Center = Vector3.zero;
			hZone.lineEdge.listPoint.ForEach(hlp => vec3Center += hlp.transform.position);
			vec3Center /= hZone.lineEdge.listPoint.Count;

			hZone.vec2Center = vec3Center;

			// �߽ɿ� ���� ��ɼ� ������� �Է�
			hZone.listDirectionalPoint.ForEach(list => list.Clear());

			iEdgeCount = hZone.lineEdge.listPoint.Count;
			for (int i = 0; i < iEdgeCount; ++i)
			{
				Battle_HPoint hlp = hZone.lineEdge.listPoint[i];

				if (0 == hlp.iContainIndex)
				{
					hlp.iDirection8 = Direction8.ciDir_5;
				}
				else
				{
					hlp.iDirection8 = Direction8.GetDirectionToInterval(vec3Center, hlp.transform.position);
					hZone.listDirectionalPoint[Direction8.cdictJoinDirArray[hlp.iDirection8][8]].Add(hlp);
				}
			}
		}

		public void CreateStartZone()
		{
			Battle_HLine hlcStart = SceneMain_Battle.Single.mcsHLine.PopLine();

			Rect rtFieldSpace = SceneMain_Battle.Single.mcsField.rtFieldSpace;
			rtFieldSpace.size *= GVar.Zone.c_fStartHuntZoneScale; // ��ü �ʵ��� 5%�� ���� ����ͷ�

			Vector2 vec2Dir1 = rtFieldSpace.position;
			vec2Dir1 += new Vector2(-rtFieldSpace.width, -rtFieldSpace.height);												// Dir_1

			hlcStart.transform.localPosition = vec2Dir1;
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x, vec2Dir1.y + rtFieldSpace.height * 2));							// Dir_7
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x + rtFieldSpace.width * 2, vec2Dir1.y + rtFieldSpace.height * 2));	// Dir_9
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x + rtFieldSpace.width * 2, vec2Dir1.y));							// Dir_3
			hlcStart.AddLinePoint(vec2Dir1);																				// Dir_1

			zoneStart = CreateZone(hlcStart);

			// �ʵ� : �ܺ� ���� ����
			SceneMain_Battle.Single.mcsField.ApplyCreateZone(zoneStart);

			SceneMain_Battle.Single.charPlayer.behaviorOwn.bhvHuntline.OnEnterHuntZone(zoneStart);
		}

		public Battle_HZone ContactLine(Vector2 vec2ContactPoint, Battle_HPoint hlpContact)
		{
			Battle_HLine hlc = hlpContact.hLine;

			// ��ɼ� -> ����� �۾�
			hlc.ProcessToCreateHuntZone(hlpContact.iContainIndex, vec2ContactPoint);

			// �� ����� ����
			Battle_HZone hz = CreateZone(hlc);

			// �ʵ� : �ܺ� ���� ����
			SceneMain_Battle.Single.mcsField.ApplyCreateZone(hz);

			// Ʈ���� �۵� : ����� ����
			ProcessHuntzoneTrigger(hz, (obj) => obj.TriggeredByHuntZoneSpawnPlaced(hz));

			return hz;
		}

		/// <summary> ��ɼ��� ���� ����Ϳ��� ����/���� �׸��� ����� ���� </summary>
		/// <param name="hlpContact"> �浹�� ������� ���� </param>
		public void ContactZone(Vector2 vec2ContactPoint, Battle_HPoint hlpContact)
		{
			// �浹�ߴ� ����� ����
			Battle_HLine hlcContact = hlpContact.hLine;
			Battle_HZone hZoneContact = hlcContact.hZone;
		
			Battle_BehaviourPlayer bhvPlayer = SceneMain_Battle.Single.charPlayer.behaviorOwn;
			if (bhvPlayer.bhvHuntline.hzPlaced != hZoneContact)
				return;

			// �浹�� ���� ȹ��, �߰�
			Battle_HLine hlpDrawing = SceneMain_Battle.Single.mcsHLine.nowDrawingLine;

			var tpProcessInfo = (
				vec2ContactPoint,						// ���� ��ġ
				bhvPlayer.bhvHuntline.hlpLastPlaced.iContainIndex,	// ���������� ��ġ�ߴ� ��ɼ� �ܰ�
				hlpContact.iContainIndex				// ���� ��ɼ� �ܰ�
				);

			ProcessExtendHuntZone(hZoneContact, hlpDrawing, ref tpProcessInfo, out List<Battle_HPoint> listHLP);

			// Ȯ��� ����� ��ġ ����
			List<Vector2> listExtendPoint = new List<Vector2>();
			int iCount = listHLP.Count;
			for (int i = 0; i < iCount; ++i)
			{
				listExtendPoint.Add(listHLP[i].transform.position);
			}

			// ����� �缳��
			hlpDrawing.DelegateLinePoint(hlcContact);
			hlcContact.ReplaceLinePoint(listHLP);

			SetEdgeToHuntZone(hZoneContact, hlcContact, false);
			SetZoneSpace(hZoneContact);

			// �ʵ� : �ܺ� ���� ����
			SceneMain_Battle.Single.mcsField.ApplyCreateZone(hZoneContact);

			// Ʈ���� �۵� : ����� Ȯ��
			ProcessHuntzoneTrigger(hZoneContact, (obj) => obj.TriggeredByHuntZoneExtendPlaced(hZoneContact, listExtendPoint));
		}

		private void ProcessExtendHuntZone(Battle_HZone hzSource, Battle_HLine hlcExtend, ref ValueTuple<Vector2, int, int> tpProcessInfo, out List<Battle_HPoint> listHLP)
		{
			/* <Ʃ�� ����>
			 * 1 : Vector2	: ���� ��ġ
			 * 2 : int		: ����� -> ��ɼ� �ۼ� ���� Index
			 * 3 : int		: ��ɼ� -> ����� �浹 ���� Index
			 */

			// 1. �ۼ��� ����� ���� �Է�
			int iCountSource = hzSource.lineEdge.listPoint.Count;
			int iCountExtend = hlcExtend.listPoint.Count;
			{
				// �����, ù ������� ��ɼ� �ۼ� ���� ��ġ�� ����
				Vector3 vec3PosDrawStart = hlcExtend.transform.position;
				hzSource.transform.PositionOnlyThisParent(vec3PosDrawStart);
				hzSource.lineEdge.transform.PositionOnlyThisParent(vec3PosDrawStart);

				listHLP = new List<Battle_HPoint>(iCountSource + iCountExtend + 1);
			}

			// ��ɼ� �Է� ����
			int iStep;
			int iIndex;
			int iEndIndex;

			// 2. ���� ��ɼ� �Է�
			{
				iStep = 1;
				iIndex = 0;
				iEndIndex = 0;

				do // �ۼ� ��ɼ� Vertex Index �߰�
				{
					listHLP.Add(hlcExtend.listPoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountExtend);
				} while (iIndex != iEndIndex);

				// ������ �׸��� ��ɼ��� ���� ��ġ�� �̵�
				hlcExtend.listPoint.Back().transform.position = tpProcessInfo.Item1;
			}

			// 3. ���� ����� ���� -> ���� ��ɼ� Index ���� ����
			int iContactBeginIndex = tpProcessInfo.Item2;
			int iContactEndIndex = tpProcessInfo.Item3;
			{
				// �ۼ� ���⿡ ���� ����� �Է� ������� ����
				GVar.EWindingOrder eSourceWindingOrder = hzSource.lineEdge.eWindingOrder;

				GVar.EWindingOrder eProcessWindingOrder;

				if (iCountExtend == 1)
				{
					// Index ���� ȹ��
					int iPointIndexInterval = Mathf.Min(
						iCountSource - Mathf.Abs(iContactEndIndex - iContactBeginIndex),
						Mathf.Abs(iContactEndIndex - iContactBeginIndex));

					// ���ݿ��� ������ ��ŭ�� ���� Ȯ��
					if (iContactBeginIndex.ModStep(iPointIndexInterval, iCountSource) == iContactEndIndex)
					{
						eProcessWindingOrder = eSourceWindingOrder;
					}
					else
					{
						eProcessWindingOrder = (GVar.EWindingOrder)((int)eSourceWindingOrder).ModStep(1, 1, 2);
					}
				}
				else
				{
					eProcessWindingOrder = 0 < hlcExtend.fWindingAngle ?
						GVar.EWindingOrder.CounterClockwise : GVar.EWindingOrder.Clockwise;
				}

				if (eProcessWindingOrder == eSourceWindingOrder)
				{
					iStep = 1;
				}
				else
				{
					iStep = -1;
					iContactBeginIndex	= iContactBeginIndex.ModStep(iStep, iCountSource);
					iContactEndIndex	= iContactEndIndex.ModStep(iStep, iCountSource);
				}

				iIndex = iContactEndIndex;
				iEndIndex = iContactBeginIndex;

				do // �ۼ� ��ɼ� Vertex Index �߰�
				{
					listHLP.Add(hzSource.lineEdge.listPoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountSource);
				} while (iIndex != iEndIndex);
			}

			// 4. �ۼ� ���� ������ ������ Point �Է�
			{
				Battle_HPoint hPointAddition = SceneMain_Battle.Single.mcsHLine.PopPoint();
				hPointAddition.transform.position = hlcExtend.transform.position;
				listHLP.Add(hPointAddition);
			}

			// 5. ������ �ʴ� ��ɼ� ����
			{
				iIndex = iContactBeginIndex;
				iEndIndex = iContactEndIndex;

				while (iIndex != iEndIndex)
				{
					hzSource.lineEdge.listPoint[iIndex].Push();
					iIndex = iIndex.ModStep(iStep, iCountSource);
				};
			}
		}

		private void ProcessHuntzoneTrigger(Battle_HZone hz, Action<Battle_BaseObject> actCallback)
		{
			int iSearchType = ObjectData.ObjectType.ciCharacter | ObjectData.ObjectType.ciHuntZoneOutline;
			if (hz.SearchPlacedObject(out List<(Battle_BaseObject, int)> listOutput, iSearchType))
			{
				int iCount = listOutput.Count;
				for (int i = 0; i < iCount; ++i)
				{
					bool isTrigger = true;
					var tpItem = listOutput[i];

					if (0 < (tpItem.Item2 & ObjectData.ObjectType.ciAlly))
					{
						isTrigger = false;  // �Ʊ� ĳ���ʹ� ���� Ʈ���� ���� �̱���
					}

					if (isTrigger)
					{
						actCallback(tpItem.Item1);
					}
				}
			}
		}
	}
}
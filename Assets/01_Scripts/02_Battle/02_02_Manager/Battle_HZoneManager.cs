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
		} // Push는 관리되며 구현할 필요 없음

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

				// 파괴구역 내부에 사냥터가 위치하는지 확인
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
						// 사냥터 내부에 파괴구역이 있는지 확인
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
					// 해당 사냥터 제거
					hZone.Push();
				}

				if (isContact)
				{
					// 해당 사냥터 파괴 진행
					destroyTool.clipper.Clear();
					listExecuteResult.Clear();
					
					destroyTool.clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(hLine.colPoly), PolyType.ptSubject, true);
					destroyTool.clipper.AddPaths(listIntPath, PolyType.ptClip, true);

					// 파괴 범위와 겹치는 구멍이 있는지 확인
					List<Battle_HLine> listHole = new List<Battle_HLine>();
					List<Vector2> listHolePath = new List<Vector2>();
					hZone.hsHlcHoles.ForEach(hlcHole =>
					{
						hlcHole.colPoly.GetWorldPath(0, listHolePath);

						// 파괴구역 내부에 구멍이 위치하는지 확인
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

					// 겹쳤던 구멍 제거
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
							// 파괴로 인한 사냥터 갈라짐
							Battle_HLine hLineSplit = SceneMain_Battle.Single.mcsHLine.PopLine();
							GlobalUtility.Clipper.SetPolygonToPathWithZero(listPath, hLineSplit.colPoly);
							RefreshLineToPolyCollider(hLineSplit);

							CreateZone(hLineSplit);
						}
					}
				}

				if (isPunch)
				{
					// 사냥터 내부의 구멍 병합
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

						// 생성된 영역의 WindingOrder 계산
						var eWOrder = GlobalUtility.Trigonometric.GetWindingOrder(listPath, p => new Vector2(p.X, p.Y));

						if (eWOrder == GVar.EWindingOrder.Clockwise)
						{
							// 구멍에 의한 사냥터 생성
							CreateZone(hlcHole);
						}
						else if (eWOrder == GVar.EWindingOrder.CounterClockwise)
						{
							// 구멍 생성
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

			// 필드 Polygon 갱신
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
			// 사냥터에 객체 종속
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
			// 사냥터 외곽선 구조 입력
			GVar.EWindingOrder eWindingOrder = hlcEdge.eWindingOrder;

			if (isHole)
			{
				eWindingOrder = (GVar.EWindingOrder)((int)eWindingOrder).ModStep(1, 2);
			}

			// 사냥터 외곽선 구조 초기값 설정
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
				// 설정할 외곽선이 존재하지 않음
				Debug.LogAssertion("Invalid Huntline edge");
			}
#endif
			// 외곽선 구조 입력
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
			// 외곽선 설정
			hZone.CalcEdgePoints();

			// 메쉬 설정
			hZone.CalcMeshZone();

			// 중심점 입력
			int iEdgeCount = hZone.lineEdge.listPoint.Count;

			Vector3 vec3Center = Vector3.zero;
			hZone.lineEdge.listPoint.ForEach(hlp => vec3Center += hlp.transform.position);
			vec3Center /= hZone.lineEdge.listPoint.Count;

			hZone.vec2Center = vec3Center;

			// 중심에 대한 사냥선 상관관계 입력
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
			rtFieldSpace.size *= GVar.Zone.c_fStartHuntZoneScale; // 전체 필드의 5%를 시작 사냥터로

			Vector2 vec2Dir1 = rtFieldSpace.position;
			vec2Dir1 += new Vector2(-rtFieldSpace.width, -rtFieldSpace.height);												// Dir_1

			hlcStart.transform.localPosition = vec2Dir1;
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x, vec2Dir1.y + rtFieldSpace.height * 2));							// Dir_7
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x + rtFieldSpace.width * 2, vec2Dir1.y + rtFieldSpace.height * 2));	// Dir_9
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x + rtFieldSpace.width * 2, vec2Dir1.y));							// Dir_3
			hlcStart.AddLinePoint(vec2Dir1);																				// Dir_1

			zoneStart = CreateZone(hlcStart);

			// 필드 : 외부 영역 적용
			SceneMain_Battle.Single.mcsField.ApplyCreateZone(zoneStart);

			SceneMain_Battle.Single.charPlayer.behaviorOwn.bhvHuntline.OnEnterHuntZone(zoneStart);
		}

		public Battle_HZone ContactLine(Vector2 vec2ContactPoint, Battle_HPoint hlpContact)
		{
			Battle_HLine hlc = hlpContact.hLine;

			// 사냥선 -> 사냥터 작업
			hlc.ProcessToCreateHuntZone(hlpContact.iContainIndex, vec2ContactPoint);

			// 새 사냥터 생성
			Battle_HZone hz = CreateZone(hlc);

			// 필드 : 외부 영역 적용
			SceneMain_Battle.Single.mcsField.ApplyCreateZone(hz);

			// 트리거 작동 : 사냥터 생성
			ProcessHuntzoneTrigger(hz, (obj) => obj.TriggeredByHuntZoneSpawnPlaced(hz));

			return hz;
		}

		/// <summary> 사냥선이 같은 사냥터에서 시작/끝을 그릴때 사냥터 생성 </summary>
		/// <param name="hlpContact"> 충돌한 사냥터의 접점 </param>
		public void ContactZone(Vector2 vec2ContactPoint, Battle_HPoint hlpContact)
		{
			// 충돌했던 사냥터 검증
			Battle_HLine hlcContact = hlpContact.hLine;
			Battle_HZone hZoneContact = hlcContact.hZone;
		
			Battle_BehaviourPlayer bhvPlayer = SceneMain_Battle.Single.charPlayer.behaviorOwn;
			if (bhvPlayer.bhvHuntline.hzPlaced != hZoneContact)
				return;

			// 충돌한 접점 획득, 추가
			Battle_HLine hlpDrawing = SceneMain_Battle.Single.mcsHLine.nowDrawingLine;

			var tpProcessInfo = (
				vec2ContactPoint,						// 접점 위치
				bhvPlayer.bhvHuntline.hlpLastPlaced.iContainIndex,	// 마지막으로 위치했던 사냥선 외곽
				hlpContact.iContainIndex				// 접점 사냥선 외곽
				);

			ProcessExtendHuntZone(hZoneContact, hlpDrawing, ref tpProcessInfo, out List<Battle_HPoint> listHLP);

			// 확장된 사냥점 위치 보존
			List<Vector2> listExtendPoint = new List<Vector2>();
			int iCount = listHLP.Count;
			for (int i = 0; i < iCount; ++i)
			{
				listExtendPoint.Add(listHLP[i].transform.position);
			}

			// 사냥터 재설정
			hlpDrawing.DelegateLinePoint(hlcContact);
			hlcContact.ReplaceLinePoint(listHLP);

			SetEdgeToHuntZone(hZoneContact, hlcContact, false);
			SetZoneSpace(hZoneContact);

			// 필드 : 외부 영역 적용
			SceneMain_Battle.Single.mcsField.ApplyCreateZone(hZoneContact);

			// 트리거 작동 : 사냥터 확장
			ProcessHuntzoneTrigger(hZoneContact, (obj) => obj.TriggeredByHuntZoneExtendPlaced(hZoneContact, listExtendPoint));
		}

		private void ProcessExtendHuntZone(Battle_HZone hzSource, Battle_HLine hlcExtend, ref ValueTuple<Vector2, int, int> tpProcessInfo, out List<Battle_HPoint> listHLP)
		{
			/* <튜플 형식>
			 * 1 : Vector2	: 접점 위치
			 * 2 : int		: 사냥터 -> 사냥선 작성 시작 Index
			 * 3 : int		: 사냥선 -> 사냥터 충돌 접점 Index
			 */

			// 1. 작성될 사냥터 정보 입력
			int iCountSource = hzSource.lineEdge.listPoint.Count;
			int iCountExtend = hlcExtend.listPoint.Count;
			{
				// 사냥터, 첫 사냥점을 사냥선 작성 시작 위치로 설정
				Vector3 vec3PosDrawStart = hlcExtend.transform.position;
				hzSource.transform.PositionOnlyThisParent(vec3PosDrawStart);
				hzSource.lineEdge.transform.PositionOnlyThisParent(vec3PosDrawStart);

				listHLP = new List<Battle_HPoint>(iCountSource + iCountExtend + 1);
			}

			// 사냥선 입력 시작
			int iStep;
			int iIndex;
			int iEndIndex;

			// 2. 접점 사냥선 입력
			{
				iStep = 1;
				iIndex = 0;
				iEndIndex = 0;

				do // 작성 사냥선 Vertex Index 추가
				{
					listHLP.Add(hlcExtend.listPoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountExtend);
				} while (iIndex != iEndIndex);

				// 마지막 그리던 사냥선을 접점 위치로 이동
				hlcExtend.listPoint.Back().transform.position = tpProcessInfo.Item1;
			}

			// 3. 기존 사냥터 시작 -> 접점 사냥선 Index 까지 진행
			int iContactBeginIndex = tpProcessInfo.Item2;
			int iContactEndIndex = tpProcessInfo.Item3;
			{
				// 작성 방향에 따른 사냥터 입력 진행방향 설정
				GVar.EWindingOrder eSourceWindingOrder = hzSource.lineEdge.eWindingOrder;

				GVar.EWindingOrder eProcessWindingOrder;

				if (iCountExtend == 1)
				{
					// Index 간격 획득
					int iPointIndexInterval = Mathf.Min(
						iCountSource - Mathf.Abs(iContactEndIndex - iContactBeginIndex),
						Mathf.Abs(iContactEndIndex - iContactBeginIndex));

					// 간격에서 떨어진 만큼의 방향 확인
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

				do // 작성 사냥선 Vertex Index 추가
				{
					listHLP.Add(hzSource.lineEdge.listPoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountSource);
				} while (iIndex != iEndIndex);
			}

			// 4. 작성 시작 지점의 마지막 Point 입력
			{
				Battle_HPoint hPointAddition = SceneMain_Battle.Single.mcsHLine.PopPoint();
				hPointAddition.transform.position = hlcExtend.transform.position;
				listHLP.Add(hPointAddition);
			}

			// 5. 사용되지 않는 사냥선 제거
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
						isTrigger = false;  // 아군 캐릭터는 현재 트리거 영향 미구현
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
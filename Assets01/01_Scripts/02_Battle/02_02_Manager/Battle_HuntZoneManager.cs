using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

namespace Proto_00_N
{
	using GlobalDefine;
	using GlobalUtility;

	[System.Serializable]
	public class Battle_HuntZoneManager
	{
		[SerializeField] private ObjectPool<Battle_HuntZone> oPoolHuntZone = new ObjectPool<Battle_HuntZone>();

		public Battle_HuntZone hzStart { get; private set; }

		[SerializeField]
		public bool isDebug;

		public void Init()
		{
			oPoolHuntZone.Init();

			CreateStartZone();
		}

		public Battle_HuntZone GetObject(int iSequenceID) => oPoolHuntZone.GetObject(iSequenceID);

		public Battle_HuntZone PopObj(Transform trParent = null)
		{
			Battle_HuntZone hZone = oPoolHuntZone.Pop(trParent);
			hZone.ReconnectRefSelf();

			return hZone;
		} // Push는 관리되며 구현할 필요 없음

		public Battle_HuntZone CreateZone(Battle_HuntLineContainer hlc)
		{
			Battle_HuntZone hZone = PopObj();

			hZone.transform.localPosition = hlc.transform.localPosition;

			SetEdgeToHuntZone(hZone, hlc, false);
			SetZoneSpace(hZone);
		
			return hZone;
		}

		public HashSet<Battle_HuntZone> DestroyZone(List<Vector2> listHolePoint)		// WorldPosition
		{
			Dictionary<Battle_HuntZone,													// Zone 단위 구분
				Dictionary<Battle_HuntLineContainer,									// 외곽선 단위 구분 ( 외곽선, 구멍 )
					List<(Battle_HuntLinePoint, int, Vector2)>>> dictContactInfo =		// 각 외곽선 간 충돌 정보
				new Dictionary<Battle_HuntZone, Dictionary<Battle_HuntLineContainer, List<(Battle_HuntLinePoint, int, Vector2)>>>();

			// Local : 사냥터 내 충돌 정보 입력
			Dictionary<Battle_HuntLineContainer, List<Vector2>> dictEdgeWorldPoints = new Dictionary<Battle_HuntLineContainer, List<Vector2>>();
			Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>> dictPointInsideEdge = new Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>>();

			bool CollideInputByHuntLineContainer(Battle_HuntLineContainer hlcEdge)
			{
				// 캐싱 : 각 외곽선의 월드 좌표
				List<Vector2> listWorldPoints;
				if (false == dictEdgeWorldPoints.TryGetValue(hlcEdge, out listWorldPoints))
				{
					listWorldPoints = hlcEdge.listVec2Point.ConvertAll<Vector2>(
						vec2 => hlcEdge.transform.TransformPoint(vec2));

					dictEdgeWorldPoints.Add(hlcEdge, listWorldPoints);
				}

				// 캐싱 : 각 외곽선 간 파괴 좌표의 안/밖 구분
				List<(Vector2, bool)> listPointInsideEdge;
				if (false == dictPointInsideEdge.TryGetValue(hlcEdge, out listPointInsideEdge))
				{
					listPointInsideEdge = listHolePoint.ConvertAll<(Vector2, bool)>(
						vec2 => (vec2, GlobalUtility.PPhysics.InsidePointInPolygon(hlcEdge.listVec2Point, vec2)));

					dictPointInsideEdge.Add(hlcEdge, listPointInsideEdge);
				}

				// 충돌 정보 입력
				Dictionary<(int, int), Vector2> dictContactResult = new Dictionary<(int, int), Vector2>();

				bool isContactEdge = PPhysics.CollidePolyPoly(
					listWorldPoints,
					listHolePoint,
					ref dictContactResult);

				// 외곽선 검출 시 충돌 정보 입력
				if (isContactEdge)
				{
					List<(Battle_HuntLinePoint, int, Vector2)> listContactValue = dictContactInfo.GetSafe(hlcEdge.hZone).GetSafe(hlcEdge);

					foreach (var item in dictContactResult)
					{
						listContactValue.Add((
								hlcEdge.listLinePoint[item.Key.Item1],
								item.Key.Item2,
								item.Value));
					}
				}

				return isContactEdge;
			}

			bool isContact = false;
			Battle_HuntLineContainer tempObj = SceneMain_Battle.Single.mcsHuntLine.PopContainer();		// null key 오류 회피

			// 사냥터 순회 : 외곽선 충돌 탐색
			oPoolHuntZone.LoopOnActive(hZone =>
			{
				bool isContactLine = false;

				// 충돌 정보 입력 : 사냥터
				isContactLine = isContactLine || CollideInputByHuntLineContainer(hZone.hlcEdge);
				
				// 사냥터 내 구멍 순회 : 구멍 외곽선 충돌 탐색
				hZone.hsHlcHoles.ForEach(hlcHole => {
					isContactLine = isContactLine || CollideInputByHuntLineContainer(hlcHole);
				});

				isContact = isContact || isContactLine;

				// 겹치는 선분이 없다면 사냥터 내부 좌표 여부 확인
				if (false == isContactLine)
				{
					if (hZone.hlcEdge.colPoly.OverlapPoint(listHolePoint[0]))
					{
						// 하나라도 덮인다면 나머지 좌표도 모두 덮임

						// 새로 생성될 구멍 정보 입력 ( tempObj )
						List<(Battle_HuntLinePoint, int, Vector2)> listContactInfo = dictContactInfo.GetSafe(hZone).GetSafe(tempObj);

						int iCount = listHolePoint.Count;
						for (int i = 0; i < iCount; ++i)
						{
							listContactInfo.Add((null, i, listHolePoint[i]));
						}

						isContact = true;
					}
					else
					{
						// 캐싱된 사냥터 외부 좌표 여부 확인
						List<Vector2> listPolygonZone = dictEdgeWorldPoints.GetDef(hZone.hlcEdge);

						// 사냥터가 파괴 영역 내부에 있다면 제거
						if (GlobalUtility.PPhysics.InsidePolygonInPolygon(listHolePoint, listPolygonZone))
						{
							hZone.Push();
						}
						
						isContact = false;
					}
				}
			});

			// 사냥터 파괴
			if (isContact)
			{
				foreach (var pairContactDict in dictContactInfo)
				{
					Battle_HuntZone hZone;
					Dictionary<Battle_HuntLineContainer, List<(Battle_HuntLinePoint, int, Vector2)>> dictContactList;

					hZone			= pairContactDict.Key;
					dictContactList	= pairContactDict.Value;

					if (1 == dictContactList.Count && dictContactList.ContainsKey(tempObj))
					{
						// 구멍 생성 : 모든 좌표가 사냥터 내부에 있음
						PunchingZone(hZone, dictPointInsideEdge, dictContactList, false, tempObj);
					}
					else
					{
						if (dictContactList.ContainsKey(hZone.hlcEdge))
						{
							if (dictContactList.Count == 1)
							{
								// 클리핑 : 외곽선과 겹치는 선(O) / 구멍 생성(X)
							}
							else
							{
								// 클리핑 : 외곽선과 겹치는 선(O) / 구멍 생성(O)
							}
						}
						else
						{
							// 구멍 확장 : 외곽선과 겹치는 선(X) / 구멍과 겹치는 선(O)
							PunchingZone(hZone, dictPointInsideEdge, dictContactList, true, tempObj);
						}
					}

					// 사냥터 갱신
					SetZoneSpace(hZone);

					// 트리거 작동 : 사냥터 손상
					ProcessHuntzoneTrigger(hZone, (obj) => obj.TriggeredByHuntZoneDamagedPlaced(hZone));
				}
			}

			tempObj.Push();	// 임시 객체 제거

			return new HashSet<Battle_HuntZone>(dictContactInfo.Keys);
		}

		/// <summary> 사냥터 내부 구멍 뚫기 </summary>
		/// <param name="bCheckExtend"> 구멍 확장여부 확인 </param>
		private void PunchingZone(Battle_HuntZone hZone, Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>> dictPointInfo, Dictionary<Battle_HuntLineContainer, List<(Battle_HuntLinePoint, int, Vector2)>> dictContactHoleInfo, bool bCheckExtend, Battle_HuntLineContainer blcTemp)
		{
			if (false == bCheckExtend)
			{
				// ----- Only 구멍 생성 진행 ----- //
				ClipingZone(hZone, dictPointInfo.GetDef(hZone.hlcEdge));
			}
			else
			{
				// ----- 사냥터 구멍 확장 ----- //
				if (1 == dictContactHoleInfo.Count)
				{
					// ----- 사냥터 구멍 단일 확장 ----- //
					foreach (var item in dictContactHoleInfo)
					{
						Battle_HuntLineContainer hlcHole							= item.Key;
						List<(Battle_HuntLinePoint, int, Vector2)> listContactInfo	= item.Value;

						ExtendZoneHole(hlcHole, dictPointInfo.GetDef(hlcHole), listContactInfo);
					}
				}
				else
				{
					// ----- 사냥터 구멍 다중 확장 ----- //
					ExtendZoneHole(dictPointInfo, dictContactHoleInfo);
				}
			}
		}

		/// <summary> 사냥터 내부 구멍 확장 </summary>
		private void ExtendZoneHole(Battle_HuntLineContainer hlcHole, List<(Vector2, bool)> listPoint, List<(Battle_HuntLinePoint, int, Vector2)> listContactInfo)
		{
			// 외곽선 방향에 따른 초기값 설정
			GVar.EWindingOrder eDestroyOrder = Trigonometric.GetWindingOrder(listPoint, (tp) => tp.Item1);
			GVar.EWindingOrder eHoleOrder = hlcHole.eWindingOrder;
			int iStep = eDestroyOrder == eHoleOrder ? 1 : -1;

			// 입력될 파괴 구역 가공
			Dictionary<int, SortedDictionary<Battle_HuntLinePoint, Vector2>> dictContactInfo = new Dictionary<int, SortedDictionary<Battle_HuntLinePoint, Vector2>>();
			
			int iInfoCount = listContactInfo.Count;
			for (int i = 0; i < iInfoCount; ++i)
			{
				(Battle_HuntLinePoint, int, Vector2) tpInfo = listContactInfo[i];
				
				SortedDictionary<Battle_HuntLinePoint, Vector2> sdictContactInfo;

				if (false == dictContactInfo.TryGetValue(tpInfo.Item2, out sdictContactInfo))
				{
					// 다음 Index로 갈 순서대로 정렬
					Comparer<Battle_HuntLinePoint> comparer = Comparer<Battle_HuntLinePoint>.Create(
						(x, y) => x.iContainIndex.CompareTo(y.iContainIndex) * iStep);

					sdictContactInfo = new SortedDictionary<Battle_HuntLinePoint, Vector2>(comparer);
				}

				sdictContactInfo.Add(tpInfo.Item1, tpInfo.Item3);
			}

			// 구멍 외곽선 계산
			List<List<Battle_HuntLinePoint>> listEdgePointBuildTotal = new List<List<Battle_HuntLinePoint>>();
			List<Battle_HuntLinePoint> listEdgePointBuild = new List<Battle_HuntLinePoint>();
			listEdgePointBuildTotal.Add(listEdgePointBuild);

			int iPlacedPointCount = hlcHole.listLinePoint.Count;
			int iExtendPointCount = listPoint.Count;
			int iCurrentPointIndex = 0;

			while (0 < dictContactInfo.Count)
			{
				// if (iCurrentPointIndex == 0 && 0 < listEdgePointBuild.Count)
				// 	break;

				SortedDictionary<Battle_HuntLinePoint, Vector2> sdictContactInfo = dictContactInfo.GetDef(iCurrentPointIndex);

				// 현재 입력될 확장점이 구멍 밖이라면 좌표 입력
				if (false == listPoint[iCurrentPointIndex].Item2)
				{
					Battle_HuntLinePoint hlp = SceneMain_Battle.Single.mcsHuntLine.PopLinePoint();
					hlp.transform.position = listPoint[iCurrentPointIndex].Item1;
					listEdgePointBuild.Add(hlp);
				}

				if (sdictContactInfo == null)
				{
					// 접점이 없음 : 제거 후 다음 Index 진행
					dictContactInfo.Remove(iCurrentPointIndex);

					iCurrentPointIndex = iCurrentPointIndex.ModStep(1, iExtendPointCount);
				}
				else
				{
					var iterContact = sdictContactInfo.GetEnumerator();
					bool isProcessNext;

					do
					{
						Battle_HuntLinePoint hlpPlaced = iterContact.Current.Key;
						Vector2 vec2ContactPoint = iterContact.Current.Value;

						// 접점 생성
						Battle_HuntLinePoint hlpContact = SceneMain_Battle.Single.mcsHuntLine.PopLinePoint();
						hlpContact.transform.position = vec2ContactPoint;
						listEdgePointBuild.Add(hlpContact);

						// 다음 접점까지 기존 구멍 Point 획득
						isProcessNext = iterContact.MoveNext();

						if (isProcessNext)
						{
							Battle_HuntLinePoint hlpNextPlaced = iterContact.Current.Key;
							Vector2 vec2NextContactPoint = iterContact.Current.Value;

							Battle_HuntLinePoint hlpProcess = hlpPlaced;
							if (iStep < 0)
							{
								// 0번 Point는 포함하지 않음
								hlpProcess = hlcHole.GetLinePoint(hlpProcess.iContainIndex.ModStep(iStep, 1, iPlacedPointCount));
							}

							// 다음 접점까지 진행
							while (hlpNextPlaced != hlpProcess)
							{
								listEdgePointBuild.Add(hlpProcess);

								hlpProcess = hlcHole.GetLinePoint(hlpProcess.iContainIndex.ModStep(iStep, 1, iPlacedPointCount));
							}
						}
					} 
					while (isProcessNext);
				}

			}
		}

		/// <summary> 사냥터 내부 구멍 확장 : 다른 구멍과 연결 </summary>
		/// <returns> 영향을 받은 구멍 반환 </returns>
		private HashSet<Battle_HuntLineContainer> ExtendZoneHole(Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>> dictPoint, Dictionary<Battle_HuntLineContainer, List<(Battle_HuntLinePoint, int, Vector2)>> dictContactHoleInfo)
		{
			return null;
		}

		/// <summary> 사냥터 구멍 뚫기 </summary>
		private void ClipingZone(Battle_HuntZone hZone, List<(Vector2, bool)> listPoint)
		{
			Battle_HuntLineContainer hlcHole = SceneMain_Battle.Single.mcsHuntLine.PopContainer(hZone.transform);

			hlcHole.gameObject.layer = GlobalDefine.CollideLayer.HuntZoneHole;
			hlcHole.transform.position = listPoint[listPoint.Count - 1].Item1;

			listPoint.ForEach(tpItem => hlcHole.AddLinePoint(tpItem.Item1, true));

			hZone.hsHlcHoles.Add(hlcHole);

			SetEdgeToHuntZone(hZone, hlcHole, true);
		}

		/// <summary> 사냥터 자르기 </summary>
		private List<Battle_HuntZone> SlicingZone(Battle_HuntZone hZone, List<(Vector2, bool)> listLine)
		{
			return null;
		}

		private void SetNormalVectorAboutEdge(Battle_HuntLineContainer hlcEdge, bool isHole)
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
				iDest = hlcEdge.listLinePoint.Count;
				fNormalDirection = 90f;
			}
			else
			{
				iIndex = hlcEdge.listLinePoint.Count - 1;
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
				Battle_HuntLinePoint hlp = hlcEdge.listLinePoint[iIndex];
				hlp.vec2pNormal = hlp.vec2Interval.Rotate(fNormalDirection).normalized;
				hlp.ApplyHuntZone(isHole);

				iIndex += iStep;
			}
		}

		private void SetEdgeToHuntZone(Battle_HuntZone hZone, Battle_HuntLineContainer hlcEdge, bool isHole)
		{
			// 사냥터에 객체 종속
			if (false == isHole)
			{
				hZone.hlcEdge = hlcEdge;
			}

			hlcEdge.hZone = hZone;
			hlcEdge.transform.parent = hZone.transform;
			hlcEdge.ApplyEdge(isHole);

			SetNormalVectorAboutEdge(hlcEdge, isHole);
		}

		private void SetZoneSpace(Battle_HuntZone hZone)
		{
			// 외곽선 설정
			hZone.CalcEdgePoints();

			// 메쉬 설정
			hZone.CalcMeshZone();

			// 중심점 입력
			int iEdgeCount = hZone.hlcEdge.listLinePoint.Count;

			Vector3 vec3Center = Vector3.zero;
			hZone.hlcEdge.listLinePoint.ForEach(hlp => vec3Center += hlp.transform.position);
			vec3Center /= hZone.hlcEdge.listLinePoint.Count;

			hZone.vec2Center = vec3Center;

			// 중심에 대한 사냥선 상관관계 입력
			hZone.listDirectionalPoint.ForEach(list => list.Clear());

			iEdgeCount = hZone.hlcEdge.listLinePoint.Count;
			for (int i = 0; i < iEdgeCount; ++i)
			{
				Battle_HuntLinePoint hlp = hZone.hlcEdge.listLinePoint[i];

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
			Battle_HuntLineContainer hlcStart = SceneMain_Battle.Single.mcsHuntLine.PopContainer();

			Rect rtFieldSpace = SceneMain_Battle.Single.mcsField.rtFieldSpace;
			rtFieldSpace.size *= GVar.Zone.c_fStartHuntZoneScale; // 전체 필드의 5%를 시작 사냥터로

			Vector2 vec2Dir1 = rtFieldSpace.position;
			vec2Dir1 += new Vector2(-rtFieldSpace.width, -rtFieldSpace.height);												// Dir_1

			hlcStart.transform.localPosition = vec2Dir1;
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x, vec2Dir1.y + rtFieldSpace.height * 2));							// Dir_7
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x + rtFieldSpace.width * 2, vec2Dir1.y + rtFieldSpace.height * 2));	// Dir_9
			hlcStart.AddLinePoint(new Vector2(vec2Dir1.x + rtFieldSpace.width * 2, vec2Dir1.y));							// Dir_3
			hlcStart.AddLinePoint(vec2Dir1);																				// Dir_1

			hzStart = CreateZone(hlcStart);

			SceneMain_Battle.Single.charPlayer.behaviorOwn.OnEnterHuntZone(hzStart);
		}

		public Battle_HuntZone ContactHuntLine(Vector2 vec2ContactPoint, Battle_HuntLinePoint hlpContact)
		{
			Battle_HuntLineContainer hlc = hlpContact.hlContainer;

			// 사냥선 -> 사냥터 작업
			hlc.ProcessToCreateHuntZone(hlpContact.iContainIndex, vec2ContactPoint);

			// 새 사냥터 생성
			Battle_HuntZone hz = CreateZone(hlc);

			// 트리거 작동 : 사냥터 생성
			ProcessHuntzoneTrigger(hz, (obj) => obj.TriggeredByHuntZoneSpawnPlaced(hz));

			return hz;
		}

		/// <summary> 사냥선이 같은 사냥터에서 시작/끝을 그릴때 사냥터 생성 </summary>
		/// <param name="hlpContact"> 충돌한 사냥터의 접점 </param>
		public void ContactHuntZone(Vector2 vec2ContactPoint, Battle_HuntLinePoint hlpContact)
		{
			// 충돌했던 사냥터 검증
			Battle_HuntLineContainer hlcContact = hlpContact.hlContainer;
			Battle_HuntZone hZoneContact = hlcContact.hZone;
		
			Battle_BehaviourPlayer bhvPlayer = SceneMain_Battle.Single.charPlayer.behaviorOwn;
			if (bhvPlayer.hzPlaced != hZoneContact)
				return;

			// 충돌한 접점 획득, 추가
			Battle_HuntLineContainer hlpDrawing = SceneMain_Battle.Single.mcsHuntLine.hlcDrawing;

			var tpProcessInfo = (
				vec2ContactPoint,						// 접점 위치
				bhvPlayer.hlpLastPlaced.iContainIndex,	// 마지막으로 위치했던 사냥선 외곽
				hlpContact.iContainIndex				// 접점 사냥선 외곽
				);

			ProcessExtendHuntZone(hZoneContact, hlpDrawing, ref tpProcessInfo, out List<Battle_HuntLinePoint> listHLP);

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

			// 트리거 작동 : 사냥터 확장
			ProcessHuntzoneTrigger(hZoneContact, (obj) => obj.TriggeredByHuntZoneExtendPlaced(hZoneContact, listExtendPoint));
		}

		private void ProcessExtendHuntZone(Battle_HuntZone hzSource, Battle_HuntLineContainer hlcExtend, ref ValueTuple<Vector2, int, int> tpProcessInfo, out List<Battle_HuntLinePoint> listHLP)
		{
			/* <튜플 형식>
			 * 1 : Vector2	: 접점 위치
			 * 2 : int		: 사냥터 -> 사냥선 작성 시작 Index
			 * 3 : int		: 사냥선 -> 사냥터 충돌 접점 Index
			 */

			// 1. 작성될 사냥터 정보 입력
			Battle_HuntLinePoint hlpTemp;
			int iCountSource = hzSource.hlcEdge.listLinePoint.Count;
			int iCountExtend = hlcExtend.listLinePoint.Count;
			{
				// 사냥터, 첫 사냥점을 사냥선 작성 시작 위치로 설정
				Vector3 vec3PosDrawStart = hlcExtend.listLinePoint[0].transform.position;
				hzSource.transform.PositionOnlyThisParent(vec3PosDrawStart);
				hzSource.hlcEdge.transform.PositionOnlyThisParent(vec3PosDrawStart);

				// 사냥터 첫 사냥점 제거 ( 보정점 보존 )
				hlpTemp = hzSource.hlcEdge.listLinePoint[0];
				hlpTemp.transform.position = vec3PosDrawStart;
				hzSource.hlcEdge.listLinePoint.RemoveAt(0);

				iCountSource = hzSource.hlcEdge.listLinePoint.Count;
				iCountExtend = hlcExtend.listLinePoint.Count;

				tpProcessInfo.Item2 = tpProcessInfo.Item2.ModStep(-1, iCountSource);
				tpProcessInfo.Item3 = tpProcessInfo.Item3.ModStep(-1, iCountSource);

				listHLP = new List<Battle_HuntLinePoint>(iCountSource + iCountExtend);
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
					listHLP.Add(hlcExtend.listLinePoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountExtend);
				} while (iIndex != iEndIndex);

				// 마지막 그리던 사냥선을 접점 위치로 이동
				hlcExtend.listLinePoint[iCountExtend - 1].transform.position = tpProcessInfo.Item1;
			}

			// 3. 기존 사냥터 시작 -> 접점 사냥선 Index 까지 진행
			int iContactBeginIndex = tpProcessInfo.Item3;
			int iContactEndIndex = tpProcessInfo.Item2;
			{
				// 작성 방향에 따른 사냥터 입력 진행방향 설정
				GVar.EWindingOrder eSourceWindingOrder = hzSource.hlcEdge.eWindingOrder;

				GVar.EWindingOrder eProcessWindingOrder;

				if (hlcExtend.fWindingAngle == 0)
				{
					// Index 간격 획득
					int iPointIndexInterval = iCountSource - Mathf.Abs(tpProcessInfo.Item2 - tpProcessInfo.Item3);

					// 방향 입력 후, 정방향이 아니라면 방향 반전
					eProcessWindingOrder = eSourceWindingOrder;
					if (tpProcessInfo.Item2.ModStep(iPointIndexInterval, iCountSource) != tpProcessInfo.Item3)
					{
						eProcessWindingOrder = (GVar.EWindingOrder)((int)eSourceWindingOrder).ModStep(1, 2);
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

				iIndex = iContactBeginIndex;
				iEndIndex = iContactEndIndex;

				do // 작성 사냥선 Vertex Index 추가
				{
					listHLP.Add(hzSource.hlcEdge.listLinePoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountSource);
				} while (iIndex != iEndIndex);

				// 보정점 정보 입력
				listHLP.Add(hlpTemp);
			}

			// 4. 사용되지 않는 사냥선 제거
			{
				iIndex = iContactEndIndex;
				iEndIndex = iContactBeginIndex;

				while (iIndex != iEndIndex)
				{
					hzSource.hlcEdge.listLinePoint[iIndex].Push();
					iIndex = iIndex.ModStep(iStep, iCountSource);
				};
			}
		}

		private void ProcessHuntzoneTrigger(Battle_HuntZone hz, Action<Battle_BaseObject> actCallback)
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
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
		} // Push�� �����Ǹ� ������ �ʿ� ����

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
			Dictionary<Battle_HuntZone,													// Zone ���� ����
				Dictionary<Battle_HuntLineContainer,									// �ܰ��� ���� ���� ( �ܰ���, ���� )
					List<(Battle_HuntLinePoint, int, Vector2)>>> dictContactInfo =		// �� �ܰ��� �� �浹 ����
				new Dictionary<Battle_HuntZone, Dictionary<Battle_HuntLineContainer, List<(Battle_HuntLinePoint, int, Vector2)>>>();

			// Local : ����� �� �浹 ���� �Է�
			Dictionary<Battle_HuntLineContainer, List<Vector2>> dictEdgeWorldPoints = new Dictionary<Battle_HuntLineContainer, List<Vector2>>();
			Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>> dictPointInsideEdge = new Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>>();

			bool CollideInputByHuntLineContainer(Battle_HuntLineContainer hlcEdge)
			{
				// ĳ�� : �� �ܰ����� ���� ��ǥ
				List<Vector2> listWorldPoints;
				if (false == dictEdgeWorldPoints.TryGetValue(hlcEdge, out listWorldPoints))
				{
					listWorldPoints = hlcEdge.listVec2Point.ConvertAll<Vector2>(
						vec2 => hlcEdge.transform.TransformPoint(vec2));

					dictEdgeWorldPoints.Add(hlcEdge, listWorldPoints);
				}

				// ĳ�� : �� �ܰ��� �� �ı� ��ǥ�� ��/�� ����
				List<(Vector2, bool)> listPointInsideEdge;
				if (false == dictPointInsideEdge.TryGetValue(hlcEdge, out listPointInsideEdge))
				{
					listPointInsideEdge = listHolePoint.ConvertAll<(Vector2, bool)>(
						vec2 => (vec2, GlobalUtility.PPhysics.InsidePointInPolygon(hlcEdge.listVec2Point, vec2)));

					dictPointInsideEdge.Add(hlcEdge, listPointInsideEdge);
				}

				// �浹 ���� �Է�
				Dictionary<(int, int), Vector2> dictContactResult = new Dictionary<(int, int), Vector2>();

				bool isContactEdge = PPhysics.CollidePolyPoly(
					listWorldPoints,
					listHolePoint,
					ref dictContactResult);

				// �ܰ��� ���� �� �浹 ���� �Է�
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
			Battle_HuntLineContainer tempObj = SceneMain_Battle.Single.mcsHuntLine.PopContainer();		// null key ���� ȸ��

			// ����� ��ȸ : �ܰ��� �浹 Ž��
			oPoolHuntZone.LoopOnActive(hZone =>
			{
				bool isContactLine = false;

				// �浹 ���� �Է� : �����
				isContactLine = isContactLine || CollideInputByHuntLineContainer(hZone.hlcEdge);
				
				// ����� �� ���� ��ȸ : ���� �ܰ��� �浹 Ž��
				hZone.hsHlcHoles.ForEach(hlcHole => {
					isContactLine = isContactLine || CollideInputByHuntLineContainer(hlcHole);
				});

				isContact = isContact || isContactLine;

				// ��ġ�� ������ ���ٸ� ����� ���� ��ǥ ���� Ȯ��
				if (false == isContactLine)
				{
					if (hZone.hlcEdge.colPoly.OverlapPoint(listHolePoint[0]))
					{
						// �ϳ��� ���δٸ� ������ ��ǥ�� ��� ����

						// ���� ������ ���� ���� �Է� ( tempObj )
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
						// ĳ�̵� ����� �ܺ� ��ǥ ���� Ȯ��
						List<Vector2> listPolygonZone = dictEdgeWorldPoints.GetDef(hZone.hlcEdge);

						// ����Ͱ� �ı� ���� ���ο� �ִٸ� ����
						if (GlobalUtility.PPhysics.InsidePolygonInPolygon(listHolePoint, listPolygonZone))
						{
							hZone.Push();
						}
						
						isContact = false;
					}
				}
			});

			// ����� �ı�
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
						// ���� ���� : ��� ��ǥ�� ����� ���ο� ����
						PunchingZone(hZone, dictPointInsideEdge, dictContactList, false, tempObj);
					}
					else
					{
						if (dictContactList.ContainsKey(hZone.hlcEdge))
						{
							if (dictContactList.Count == 1)
							{
								// Ŭ���� : �ܰ����� ��ġ�� ��(O) / ���� ����(X)
							}
							else
							{
								// Ŭ���� : �ܰ����� ��ġ�� ��(O) / ���� ����(O)
							}
						}
						else
						{
							// ���� Ȯ�� : �ܰ����� ��ġ�� ��(X) / ���۰� ��ġ�� ��(O)
							PunchingZone(hZone, dictPointInsideEdge, dictContactList, true, tempObj);
						}
					}

					// ����� ����
					SetZoneSpace(hZone);

					// Ʈ���� �۵� : ����� �ջ�
					ProcessHuntzoneTrigger(hZone, (obj) => obj.TriggeredByHuntZoneDamagedPlaced(hZone));
				}
			}

			tempObj.Push();	// �ӽ� ��ü ����

			return new HashSet<Battle_HuntZone>(dictContactInfo.Keys);
		}

		/// <summary> ����� ���� ���� �ձ� </summary>
		/// <param name="bCheckExtend"> ���� Ȯ�忩�� Ȯ�� </param>
		private void PunchingZone(Battle_HuntZone hZone, Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>> dictPointInfo, Dictionary<Battle_HuntLineContainer, List<(Battle_HuntLinePoint, int, Vector2)>> dictContactHoleInfo, bool bCheckExtend, Battle_HuntLineContainer blcTemp)
		{
			if (false == bCheckExtend)
			{
				// ----- Only ���� ���� ���� ----- //
				ClipingZone(hZone, dictPointInfo.GetDef(hZone.hlcEdge));
			}
			else
			{
				// ----- ����� ���� Ȯ�� ----- //
				if (1 == dictContactHoleInfo.Count)
				{
					// ----- ����� ���� ���� Ȯ�� ----- //
					foreach (var item in dictContactHoleInfo)
					{
						Battle_HuntLineContainer hlcHole							= item.Key;
						List<(Battle_HuntLinePoint, int, Vector2)> listContactInfo	= item.Value;

						ExtendZoneHole(hlcHole, dictPointInfo.GetDef(hlcHole), listContactInfo);
					}
				}
				else
				{
					// ----- ����� ���� ���� Ȯ�� ----- //
					ExtendZoneHole(dictPointInfo, dictContactHoleInfo);
				}
			}
		}

		/// <summary> ����� ���� ���� Ȯ�� </summary>
		private void ExtendZoneHole(Battle_HuntLineContainer hlcHole, List<(Vector2, bool)> listPoint, List<(Battle_HuntLinePoint, int, Vector2)> listContactInfo)
		{
			// �ܰ��� ���⿡ ���� �ʱⰪ ����
			GVar.EWindingOrder eDestroyOrder = Trigonometric.GetWindingOrder(listPoint, (tp) => tp.Item1);
			GVar.EWindingOrder eHoleOrder = hlcHole.eWindingOrder;
			int iStep = eDestroyOrder == eHoleOrder ? 1 : -1;

			// �Էµ� �ı� ���� ����
			Dictionary<int, SortedDictionary<Battle_HuntLinePoint, Vector2>> dictContactInfo = new Dictionary<int, SortedDictionary<Battle_HuntLinePoint, Vector2>>();
			
			int iInfoCount = listContactInfo.Count;
			for (int i = 0; i < iInfoCount; ++i)
			{
				(Battle_HuntLinePoint, int, Vector2) tpInfo = listContactInfo[i];
				
				SortedDictionary<Battle_HuntLinePoint, Vector2> sdictContactInfo;

				if (false == dictContactInfo.TryGetValue(tpInfo.Item2, out sdictContactInfo))
				{
					// ���� Index�� �� ������� ����
					Comparer<Battle_HuntLinePoint> comparer = Comparer<Battle_HuntLinePoint>.Create(
						(x, y) => x.iContainIndex.CompareTo(y.iContainIndex) * iStep);

					sdictContactInfo = new SortedDictionary<Battle_HuntLinePoint, Vector2>(comparer);
				}

				sdictContactInfo.Add(tpInfo.Item1, tpInfo.Item3);
			}

			// ���� �ܰ��� ���
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

				// ���� �Էµ� Ȯ������ ���� ���̶�� ��ǥ �Է�
				if (false == listPoint[iCurrentPointIndex].Item2)
				{
					Battle_HuntLinePoint hlp = SceneMain_Battle.Single.mcsHuntLine.PopLinePoint();
					hlp.transform.position = listPoint[iCurrentPointIndex].Item1;
					listEdgePointBuild.Add(hlp);
				}

				if (sdictContactInfo == null)
				{
					// ������ ���� : ���� �� ���� Index ����
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

						// ���� ����
						Battle_HuntLinePoint hlpContact = SceneMain_Battle.Single.mcsHuntLine.PopLinePoint();
						hlpContact.transform.position = vec2ContactPoint;
						listEdgePointBuild.Add(hlpContact);

						// ���� �������� ���� ���� Point ȹ��
						isProcessNext = iterContact.MoveNext();

						if (isProcessNext)
						{
							Battle_HuntLinePoint hlpNextPlaced = iterContact.Current.Key;
							Vector2 vec2NextContactPoint = iterContact.Current.Value;

							Battle_HuntLinePoint hlpProcess = hlpPlaced;
							if (iStep < 0)
							{
								// 0�� Point�� �������� ����
								hlpProcess = hlcHole.GetLinePoint(hlpProcess.iContainIndex.ModStep(iStep, 1, iPlacedPointCount));
							}

							// ���� �������� ����
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

		/// <summary> ����� ���� ���� Ȯ�� : �ٸ� ���۰� ���� </summary>
		/// <returns> ������ ���� ���� ��ȯ </returns>
		private HashSet<Battle_HuntLineContainer> ExtendZoneHole(Dictionary<Battle_HuntLineContainer, List<(Vector2, bool)>> dictPoint, Dictionary<Battle_HuntLineContainer, List<(Battle_HuntLinePoint, int, Vector2)>> dictContactHoleInfo)
		{
			return null;
		}

		/// <summary> ����� ���� �ձ� </summary>
		private void ClipingZone(Battle_HuntZone hZone, List<(Vector2, bool)> listPoint)
		{
			Battle_HuntLineContainer hlcHole = SceneMain_Battle.Single.mcsHuntLine.PopContainer(hZone.transform);

			hlcHole.gameObject.layer = GlobalDefine.CollideLayer.HuntZoneHole;
			hlcHole.transform.position = listPoint[listPoint.Count - 1].Item1;

			listPoint.ForEach(tpItem => hlcHole.AddLinePoint(tpItem.Item1, true));

			hZone.hsHlcHoles.Add(hlcHole);

			SetEdgeToHuntZone(hZone, hlcHole, true);
		}

		/// <summary> ����� �ڸ��� </summary>
		private List<Battle_HuntZone> SlicingZone(Battle_HuntZone hZone, List<(Vector2, bool)> listLine)
		{
			return null;
		}

		private void SetNormalVectorAboutEdge(Battle_HuntLineContainer hlcEdge, bool isHole)
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
				// ������ �ܰ����� �������� ����
				Debug.LogAssertion("Invalid Huntline edge");
			}
#endif
			// �ܰ��� ���� �Է�
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
			// ����Ϳ� ��ü ����
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
			// �ܰ��� ����
			hZone.CalcEdgePoints();

			// �޽� ����
			hZone.CalcMeshZone();

			// �߽��� �Է�
			int iEdgeCount = hZone.hlcEdge.listLinePoint.Count;

			Vector3 vec3Center = Vector3.zero;
			hZone.hlcEdge.listLinePoint.ForEach(hlp => vec3Center += hlp.transform.position);
			vec3Center /= hZone.hlcEdge.listLinePoint.Count;

			hZone.vec2Center = vec3Center;

			// �߽ɿ� ���� ��ɼ� ������� �Է�
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
			rtFieldSpace.size *= GVar.Zone.c_fStartHuntZoneScale; // ��ü �ʵ��� 5%�� ���� ����ͷ�

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

			// ��ɼ� -> ����� �۾�
			hlc.ProcessToCreateHuntZone(hlpContact.iContainIndex, vec2ContactPoint);

			// �� ����� ����
			Battle_HuntZone hz = CreateZone(hlc);

			// Ʈ���� �۵� : ����� ����
			ProcessHuntzoneTrigger(hz, (obj) => obj.TriggeredByHuntZoneSpawnPlaced(hz));

			return hz;
		}

		/// <summary> ��ɼ��� ���� ����Ϳ��� ����/���� �׸��� ����� ���� </summary>
		/// <param name="hlpContact"> �浹�� ������� ���� </param>
		public void ContactHuntZone(Vector2 vec2ContactPoint, Battle_HuntLinePoint hlpContact)
		{
			// �浹�ߴ� ����� ����
			Battle_HuntLineContainer hlcContact = hlpContact.hlContainer;
			Battle_HuntZone hZoneContact = hlcContact.hZone;
		
			Battle_BehaviourPlayer bhvPlayer = SceneMain_Battle.Single.charPlayer.behaviorOwn;
			if (bhvPlayer.hzPlaced != hZoneContact)
				return;

			// �浹�� ���� ȹ��, �߰�
			Battle_HuntLineContainer hlpDrawing = SceneMain_Battle.Single.mcsHuntLine.hlcDrawing;

			var tpProcessInfo = (
				vec2ContactPoint,						// ���� ��ġ
				bhvPlayer.hlpLastPlaced.iContainIndex,	// ���������� ��ġ�ߴ� ��ɼ� �ܰ�
				hlpContact.iContainIndex				// ���� ��ɼ� �ܰ�
				);

			ProcessExtendHuntZone(hZoneContact, hlpDrawing, ref tpProcessInfo, out List<Battle_HuntLinePoint> listHLP);

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

			// Ʈ���� �۵� : ����� Ȯ��
			ProcessHuntzoneTrigger(hZoneContact, (obj) => obj.TriggeredByHuntZoneExtendPlaced(hZoneContact, listExtendPoint));
		}

		private void ProcessExtendHuntZone(Battle_HuntZone hzSource, Battle_HuntLineContainer hlcExtend, ref ValueTuple<Vector2, int, int> tpProcessInfo, out List<Battle_HuntLinePoint> listHLP)
		{
			/* <Ʃ�� ����>
			 * 1 : Vector2	: ���� ��ġ
			 * 2 : int		: ����� -> ��ɼ� �ۼ� ���� Index
			 * 3 : int		: ��ɼ� -> ����� �浹 ���� Index
			 */

			// 1. �ۼ��� ����� ���� �Է�
			Battle_HuntLinePoint hlpTemp;
			int iCountSource = hzSource.hlcEdge.listLinePoint.Count;
			int iCountExtend = hlcExtend.listLinePoint.Count;
			{
				// �����, ù ������� ��ɼ� �ۼ� ���� ��ġ�� ����
				Vector3 vec3PosDrawStart = hlcExtend.listLinePoint[0].transform.position;
				hzSource.transform.PositionOnlyThisParent(vec3PosDrawStart);
				hzSource.hlcEdge.transform.PositionOnlyThisParent(vec3PosDrawStart);

				// ����� ù ����� ���� ( ������ ���� )
				hlpTemp = hzSource.hlcEdge.listLinePoint[0];
				hlpTemp.transform.position = vec3PosDrawStart;
				hzSource.hlcEdge.listLinePoint.RemoveAt(0);

				iCountSource = hzSource.hlcEdge.listLinePoint.Count;
				iCountExtend = hlcExtend.listLinePoint.Count;

				tpProcessInfo.Item2 = tpProcessInfo.Item2.ModStep(-1, iCountSource);
				tpProcessInfo.Item3 = tpProcessInfo.Item3.ModStep(-1, iCountSource);

				listHLP = new List<Battle_HuntLinePoint>(iCountSource + iCountExtend);
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
					listHLP.Add(hlcExtend.listLinePoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountExtend);
				} while (iIndex != iEndIndex);

				// ������ �׸��� ��ɼ��� ���� ��ġ�� �̵�
				hlcExtend.listLinePoint[iCountExtend - 1].transform.position = tpProcessInfo.Item1;
			}

			// 3. ���� ����� ���� -> ���� ��ɼ� Index ���� ����
			int iContactBeginIndex = tpProcessInfo.Item3;
			int iContactEndIndex = tpProcessInfo.Item2;
			{
				// �ۼ� ���⿡ ���� ����� �Է� ������� ����
				GVar.EWindingOrder eSourceWindingOrder = hzSource.hlcEdge.eWindingOrder;

				GVar.EWindingOrder eProcessWindingOrder;

				if (hlcExtend.fWindingAngle == 0)
				{
					// Index ���� ȹ��
					int iPointIndexInterval = iCountSource - Mathf.Abs(tpProcessInfo.Item2 - tpProcessInfo.Item3);

					// ���� �Է� ��, �������� �ƴ϶�� ���� ����
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

				do // �ۼ� ��ɼ� Vertex Index �߰�
				{
					listHLP.Add(hzSource.hlcEdge.listLinePoint[iIndex]);
					iIndex = iIndex.ModStep(iStep, iCountSource);
				} while (iIndex != iEndIndex);

				// ������ ���� �Է�
				listHLP.Add(hlpTemp);
			}

			// 4. ������ �ʴ� ��ɼ� ����
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	using GlobalDefine;

	public class Battle_HuntLineContainer : Battle_BaseObject
	{
		public enum EdgeType
		{
			None,
			Outline,
			Hole,
			Max,
		}

		[Header("----- Hunt Line Container -----")]
		[Header("Own Component")]
		public PolygonCollider2D colPoly;
		public EdgeType eEdgeType;

		[Header("Contains")]
		public List<Battle_HuntLinePoint> listLinePoint;
		public List<Vector2> listVec2Point;

		[Header("Ref")]
		public Battle_HuntZone hZone;

		public float fWindingAngle { get; set; }

		public GVar.EWindingOrder eWindingOrder 
		{
			get => fWindingAngle < 0 ?
				 GVar.EWindingOrder.Clockwise :
				 GVar.EWindingOrder.CounterClockwise;
		}

		protected override void Init()
		{
			base.Init();

			listLinePoint = new List<Battle_HuntLinePoint>();
			listVec2Point = new List<Vector2>();
			hZone = null;

			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;
		}

		public void Reset()
		{
			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;

			ClearLinePoint();
			fWindingAngle = 0f;

			// 사냥점이 없을 경우 추가
			if (listLinePoint.Count < 1)
			{
				listVec2Point.Clear();
				AddLinePoint(transform.localPosition);
			}
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			colPoly = GetComponent<PolygonCollider2D>();
		}

		public override void OnPopedFromPool()
		{
			base.OnPopedFromPool();
		}

		public override void OnPushedToPool()
		{
			Reset();
			base.OnPushedToPool();
		}

		public Battle_HuntLinePoint AddLinePoint(Vector2 vec2To, bool isWorldPosition = false)
		{
			Battle_HuntLinePoint hlp = SceneMain_Battle.Single.mcsHuntLine.PopLinePoint(this.transform);

			hlp.Reset();
			hlp.hlContainer = this;
			hlp.iContainIndex = listLinePoint.Count;

			if (isWorldPosition)
			{
				hlp.vec2Position = vec2To;
			}
			else
			{
				hlp.vec2LocalPosition = vec2To;
			}

			listLinePoint.Add(hlp);
			listVec2Point.Add(hlp.transform.localPosition);

			hlp.RefreshLineInfo();
			hlp.ApplyLine();
			hlp.CalcOwnAngle(false);

			// 작성중인 사냥선 적용
			if (null == hZone)
			{
				hlp.ApplyDrawLine(true);
				GetLinePoint(listLinePoint.Count - 2)?.ApplyDrawLine(false);
			}

			return hlp;
		}

		public Battle_HuntLinePoint DeleteLastLinePoint()
		{
			Battle_HuntLinePoint hlpLast = null;

			int iLastIndex = listLinePoint.Count - 1;
			if (iLastIndex < 0)
				return hlpLast;

			listLinePoint[iLastIndex].Push();
			listLinePoint.RemoveAt(iLastIndex);
			listVec2Point.RemoveAt(iLastIndex);

			// 작성중인 사냥선 적용
			if (null == hZone)
			{
				hlpLast = GetLinePoint(listLinePoint.Count - 1);
				if (null != hlpLast)
				{
					hlpLast.gameObject.layer = CollideLayer.HuntLineDrawing;
				}
			}

			return hlpLast;
		}

		public void DelegateLinePoint(Battle_HuntLineContainer hlcOwner)
		{
			for (int i = 0; i < listLinePoint.Count; ++i)
			{
				listLinePoint[i].transform.parent = hlcOwner.transform;
			}
			listLinePoint.Clear();
			listVec2Point.Clear();

			Push();
		}

		public void ReplaceLinePoint(List<Battle_HuntLinePoint> listHlc)
		{
			fWindingAngle = 0;
			listLinePoint = listHlc;
			listVec2Point.Clear();

			// 참조 갱신
			int iCount = listLinePoint.Count;
			for (int i = 0; i < iCount; ++i)
			{
				Battle_HuntLinePoint hlp = listLinePoint[i];

				hlp.hlContainer = this;
				hlp.iContainIndex = i;

				if (hlp.transform.parent != this.transform)
				{
					hlp.transform.parent = this.transform;
				}

				fWindingAngle += hlp.fDegreeByPrevPoint; // 추가될 각도 보정

				listVec2Point.Add(hlp.transform.localPosition);
			}

			// Point 정보 입력
			for (int i = 0; i < iCount; ++i)
			{
				Battle_HuntLinePoint hlp = listLinePoint[i];

				hlp.RefreshLineInfo();
				hlp.ApplyLine();
				hlp.CalcOwnAngle(false);
				hlp.ApplyHuntZone(false);

#if _debug
				hlp.transform.SetSiblingIndex(i);
#endif
			}
		}

		public void ClearLinePoint()
		{
			if (1 < listLinePoint.Count)
			{
				for (int i = 1; i < listLinePoint.Count; ++i)
				{
					listLinePoint[i].Push();
				}

				listLinePoint.RemoveRange(1, listLinePoint.Count - 1);
				listVec2Point.RemoveRange(1, listLinePoint.Count - 1);

				listLinePoint[0].Reset();
			}
		}

		public Battle_HuntLinePoint GetLinePoint(int iIndex)
		{
			Battle_HuntLinePoint hlPoint = null;

			if (0 <= iIndex && iIndex < listLinePoint.Count)
			{
				hlPoint = listLinePoint[iIndex];
			}

			return hlPoint;
		}

		/// <summary> 사냥터 전환 작업 </summary>
		public bool ProcessToCreateHuntZone(int iContactIndex, Vector2 vec2ContactPosition)
		{
			Battle_HuntLinePoint hlpContact = GetLinePoint(iContactIndex);
			if (null == hlpContact)
				return false;

			// 바로 이전 사냥선 지점 자리 이동
			int iRemoveIndex = iContactIndex - 1;
			Battle_HuntLinePoint hlpStart = GetLinePoint(iRemoveIndex);
			Battle_HuntLinePoint hlpNext = hlpStart.PointNext;

			hlpStart.transform.position = vec2ContactPosition;

			// 충돌정보 갱신
			hlpStart.RefreshLineInfo();
			hlpStart.ApplyLine();
			hlpNext.RefreshLineInfo();
			hlpNext.ApplyLine();

			// 충돌 지점 제외 제거 : 제거되는 각도 보정
			for (int i = 0; i < iRemoveIndex; ++i)
			{
				Battle_HuntLinePoint hlpRemove = listLinePoint[i];

				fWindingAngle -= hlpRemove.fDegreeByPrevPoint;
				hlpRemove.Push();
			}
			listLinePoint.RemoveRange(0, iRemoveIndex);
			listLinePoint[0].listLinePos[0] = Vector2.zero;
			listLinePoint[0].fDegreeByPrevPoint = 0;
			listLinePoint[0].ApplyLine();

			// 변경된 지점 적용, 사냥터로 객체타입 변환
			for (int i = 0; i < listLinePoint.Count; ++i)
			{
				Battle_HuntLinePoint hlp = listLinePoint[i];

				hlp.ApplyHuntZone(false);
				hlp.iContainIndex = i;
				listVec2Point[i] = hlp.transform.localPosition;
			}

			ApplyEdge(false);

			return true;
		}

		public void ApplyEdge(bool isHole)
		{
			if (isHole)
			{
				gameObject.layer = CollideLayer.HuntZoneHole;
				iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole;
				eEdgeType = EdgeType.Hole;
			}
			else
			{
				gameObject.layer = CollideLayer.HuntZoneEdge;
				iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline;
				eEdgeType = EdgeType.Outline;
			}
		}

		public override void TriggeredByHuntZoneExtendPlaced(Battle_HuntZone hzSpawned, List<Vector2> listExtendPoint)
		{
			hZone.TriggeredByHuntZoneExtendPlaced(hzSpawned, listExtendPoint);
			base.TriggeredByHuntZoneExtendPlaced(hzSpawned, listExtendPoint);

			hZone.Push();
		}

		public override void TriggeredByHuntZoneSpawnPlaced(Battle_HuntZone hzSpawned)
		{
			hZone.TriggeredByHuntZoneSpawnPlaced(hzSpawned);
			base.TriggeredByHuntZoneSpawnPlaced(hzSpawned);

			hZone.Push();
		}
	}
}

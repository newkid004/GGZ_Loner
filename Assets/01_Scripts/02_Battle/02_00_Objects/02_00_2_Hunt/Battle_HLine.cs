using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_HLine : Battle_BaseObject
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
		public List<Battle_HPoint> listPoint;
		public List<Vector2> listPointPos;

		[Header("Ref")]
		public Battle_HZone hZone;

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

			listPoint = new List<Battle_HPoint>();
			listPointPos = new List<Vector2>();
			hZone = null;

			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;
		}

		public void Reset()
		{
			listPoint.ForEach(p => p.Push());
			listPoint.Clear();
			listPointPos.Clear();
			hZone = null;

			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;

			fWindingAngle = 0f;
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();
			colPoly = GetComponent<PolygonCollider2D>();
		}

		public override void OnPushedToPool()
		{
			Reset();
			base.OnPushedToPool();
		}

		public Battle_HPoint AddLinePoint(Vector2 vec2Position, int iSetDirection = Direction8.ciDir_5)
		{
			Battle_HPoint hlp = SceneMain_Battle.Single.mcsHLine.PopPoint(this.transform);

			hlp.hLine = this;
			hlp.iContainIndex = listPoint.Count;
			hlp.transform.position = vec2Position;

			listPoint.Add(hlp);
			listPointPos.Add(hlp.transform.localPosition);

			hlp.RefreshLineInfo();
			hlp.ApplyLine();
 			hlp.CalcOwnAngle(false, iSetDirection);

			// 작성중인 사냥선 적용
			if (this == SceneMain_Battle.Single.mcsHLine.nowDrawingLine)
			{
				hlp.ApplyDrawLine(true);
				GetLinePoint(listPoint.Count - 2)?.ApplyDrawLine(false);
			}

			return hlp;
		}

		public Battle_HPoint DeleteLastLinePoint()
		{
			Battle_HPoint hlpLast = null;

			int iLastIndex = listPoint.Count - 1;
			if (iLastIndex < 0)
				return hlpLast;

			listPoint[iLastIndex].Push();
			listPoint.RemoveAt(iLastIndex);
			listPointPos.RemoveAt(iLastIndex);

			// 작성중인 사냥선 적용
			if (null == hZone)
			{
				hlpLast = GetLinePoint(listPoint.Count - 1);
				if (null != hlpLast)
				{
					hlpLast.gameObject.layer = CollideLayer.HuntLineDrawing;
				}
			}

			return hlpLast;
		}

		// 사냥터에만 사용
		public void ReplaceLinePoint(List<Vector2> listNewPoint)
		{
			listPoint.ForEach(p => p.Push());
			listPoint.Clear();
			listPointPos.Clear();
			fWindingAngle = 0f;

			int iCount = listNewPoint.Count;
			for (int i = 1; i <= iCount; ++i)
			{
				Battle_HPoint hlp = SceneMain_Battle.Single.mcsHLine.PopPoint(this.transform);
				hlp.hLine = this;

				hlp.iContainIndex = listPoint.Count;
				hlp.transform.localPosition = listNewPoint[i.ModStep(0, iCount)];

				listPoint.Add(hlp);
				listPointPos.Add(hlp.transform.localPosition);
			}

			listPoint.ForEach(p =>
			{
				p.RefreshLineInfo();
				p.ApplyLine();
				p.CalcOwnAngle(false);
			});
		}

		public void DelegateLinePoint(Battle_HLine hlcOwner)
		{
			for (int i = 0; i < listPoint.Count; ++i)
			{
				listPoint[i].transform.parent = hlcOwner.transform;
			}
			listPoint.Clear();
			listPointPos.Clear();

			Push();
		}

		public void ReplaceLinePoint(List<Battle_HPoint> listHlc)
		{
			fWindingAngle = 0;
			listPoint = listHlc;
			listPointPos.Clear();

			// 참조 갱신
			int iCount = listPoint.Count;
			for (int i = 0; i < iCount; ++i)
			{
				Battle_HPoint hlp = listPoint[i];

				hlp.hLine = this;
				hlp.iContainIndex = i;

				if (hlp.transform.parent != this.transform)
				{
					hlp.transform.parent = this.transform;
				}

				listPointPos.Add(hlp.transform.localPosition);
			}

			// Point 정보 입력
			for (int i = 0; i < iCount; ++i)
			{
				Battle_HPoint hlp = listPoint[i];

				hlp.RefreshLineInfo();
				hlp.ApplyLine();

				hlp.fDegreeByPrevPoint = 0;
				hlp.CalcOwnAngle(false);
				hlp.ApplyHuntZone(false);

#if _debug
				hlp.transform.SetSiblingIndex(i);
#endif
			}
		}

		public Battle_HPoint GetLinePoint(int iIndex, bool isCircle = false)
		{
			if (isCircle)
			{
				iIndex = iIndex.ModStep(0, listPoint.Count);
			}
			else
			{
				if (iIndex < 0 || listPoint.Count <= iIndex)
					return null;
			}

			return listPoint[iIndex];
		}

		/// <summary> 사냥터 전환 작업 </summary>
		public bool ProcessToCreateHuntZone(int iContactIndex, Vector2 vec2ContactPosition)
		{
			// 진행중인 사냥점을 기준으로 작업
			Battle_HPoint hpNow = SceneMain_Battle.Single.mcsHLine.nowDrawingPoint;
			Battle_HPoint hpNext = hpNow.PointNext;

			hpNow.PosWorld = vec2ContactPosition;

			// 충돌정보 갱신
			hpNow.RefreshLineInfo();
			hpNow.ApplyLine();
			if (hpNext != null)
			{
				hpNext.RefreshLineInfo();
				hpNext.ApplyLine();
			}

			// 충돌 지점 제외 제거 : 제거되는 각도 보정
			for (int i = 0; i < iContactIndex; ++i)
			{
				Battle_HPoint hpRemove = listPoint[i];

				fWindingAngle -= hpRemove.fDegreeByPrevPoint;
				hpRemove.Push();
			}

			listPoint.RemoveRange(0, iContactIndex);
			listPoint[0].listLinePos[0] = Vector2.zero;
			listPoint[0].fDegreeByPrevPoint = 0;
			listPoint[0].ApplyLine();

			// 변경된 지점 적용, 사냥터로 객체타입 변환
			for (int i = 0; i < listPoint.Count; ++i)
			{
				Battle_HPoint hlp = listPoint[i];

				hlp.ApplyHuntZone(false);
				hlp.iContainIndex = i;
				listPointPos[i] = hlp.transform.localPosition;
			}

			ApplyEdge(false);

			return true;
		}

		public void ApplyEdge(bool isHole)
		{
			listPoint[0].RefreshLineInfo();

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

		public override void TriggeredByHuntZoneExtendPlaced(Battle_HZone hzSpawned, List<Vector2> listExtendPoint)
		{
			hZone.TriggeredByHuntZoneExtendPlaced(hzSpawned, listExtendPoint);
			base.TriggeredByHuntZoneExtendPlaced(hzSpawned, listExtendPoint);

			hZone.Push();
		}

		public override void TriggeredByHuntZoneSpawnPlaced(Battle_HZone hzSpawned)
		{
			hZone.TriggeredByHuntZoneSpawnPlaced(hzSpawned);
			base.TriggeredByHuntZoneSpawnPlaced(hzSpawned);

			hZone.Push();
		}
	}
}

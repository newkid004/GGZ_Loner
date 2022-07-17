using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	using GlobalDefine;

	public class Battle_HuntLinePoint : Battle_BaseObject
	{
		[Header("----- Hunt Line -----")]
		[Header("Own Component")]
		public LineRenderer rdrLine;
		public EdgeCollider2D colEdge;

		[Header("Info : Zone")]
		public Vector2 vec2pNormal;	 // 법선 벡터
		public int iDirection8;			// 

		[Header("Info : Container")]
		public Battle_HuntLineContainer hlContainer;
		public int iContainIndex;
		public float fDegreeByPrevPoint;	// 이전 사냥선 지점에 대한 각도

		[Header("Info : Point")]
		public List<Vector2> listLinePos = new List<Vector2>(new Vector2[] { Vector2.zero, Vector2.zero });

		public Battle_HuntLinePoint PointPrev { get => hlContainer.GetLinePoint(iContainIndex - 1); }
		public Battle_HuntLinePoint PointNext { get => hlContainer.GetLinePoint(iContainIndex + 1); }
		public float Length { get => listLinePos[0].magnitude; }

		public Vector2 vec2Position
		{
			get => transform.position;
			set
			{
				transform.position = value;
				CalcOwnAngle(true);
			}
		}

		// 위치값 반환 : 컨테이너 위치 보정
		public Vector2 vec2LocalPosition 
		{
			get => transform.localPosition + hlContainer.transform.localPosition;
			set
			{
				Vector2 vec2ParentPos = hlContainer.transform.localPosition;
				Vector2 vec2PointPos = value - vec2ParentPos;
				transform.localPosition = vec2PointPos;

				CalcOwnAngle(true);
			}
		}

		// 선분 시작점
		public Vector2 vec2FromPoint 
		{
			get
			{
				Battle_HuntLinePoint hlpPrev = PointPrev;
				if (null == hlpPrev)
				{
					return hlContainer.transform.localPosition;
				}
				else
				{
					return hlpPrev.vec2LocalPosition;
				}
			}
			set
			{
				Vector2 vec2ParentPos = hlContainer.transform.localPosition;
				Vector2 vec2PointPos = value - vec2ParentPos;

				listLinePos[0] = vec2PointPos;

				Battle_HuntLinePoint hlPointPrev = PointPrev;
				if (null != hlPointPrev)
				{
					hlPointPrev.transform.localPosition = value;

					ApplyLine();
					hlPointPrev.ApplyLine();

					hlPointPrev.CalcOwnAngle(true);
				}
				else
				{
					this.CalcOwnAngle(false);
					// throw new System.Exception("Battle_HuntLinePoint : vec2FromPoint set Error ( iIndex == 0 )");
				}
			} 
		}

		// 선분 끝점
		public Vector2 vec2ToPoint 
		{
			get => vec2Position; 
			set
			{
				transform.position = value;

				RefreshLineInfo();
				ApplyLine();

				Battle_HuntLinePoint hlPointNext = PointNext;
				if (null != hlPointNext)
				{
					hlPointNext.RefreshLineInfo();
					hlPointNext.ApplyLine();
				}

				CalcOwnAngle(null != hlPointNext);
			}
		}

		// 선분 끝점
		public Vector2 vec2ToLocalPoint
		{
			get => vec2LocalPosition;
			set
			{
				transform.localPosition = value;

				RefreshLineInfo();
				ApplyLine();

				Battle_HuntLinePoint hlPointNext = PointNext;
				if (null != hlPointNext)
				{
					hlPointNext.RefreshLineInfo();
					hlPointNext.ApplyLine();
				}

				CalcOwnAngle(null != hlPointNext);
			}
		}

		public Vector2 vec2Interval { get => -listLinePos[0]; }
		public Vector2 vec2Direction { get => vec2Interval.normalized; }
		public int iDirectionDraw { get => Direction8.GetDirectionToNormal(vec2Direction); }
		public int iDirectionNormal { get => Direction8.GetDirectionToNormal(vec2pNormal); }

		private float GetDegreeByPrevPoint 
		{
			get
			{
				if (0 == iContainIndex)
				{
					return 0;
				}
				else
				{
					return Vector2.SignedAngle(PointPrev.vec2Interval.normalized,
						vec2Interval.normalized);
				}
			}
		}

		public void Reset()
		{
			hlContainer			= null;
			iContainIndex		= -1;
			vec2pNormal			= Vector2.zero;
			fDegreeByPrevPoint	= 0;

			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;
		}

		protected override void Init()
		{
			base.Init();
			
			Reset();
		}

		public override void OnPushedToPool()
		{
			Battle_BehaviourPlayer bhvPlayer = SceneMain_Battle.Single.charPlayer.behaviorOwn;
			bhvPlayer.OnExitHuntZoneOutline(this);

			Reset();

			base.OnPushedToPool();
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			rdrLine = GetComponent<LineRenderer>();
			colEdge = GetComponent<EdgeCollider2D>();
		}

		// 선분 값 재계산
		public void RefreshLineInfo()
		{
			// 위치 정보 저장
			if (0 == iContainIndex)
			{
				listLinePos[0] = Vector2.zero; // -transform.localPosition;
			}
			else
			{
				listLinePos[0] = PointPrev.transform.localPosition - transform.localPosition;
			}
		}

		// 선 정보 적용 : 보유한 선분값을 통해 출력 / 충돌 컴포넌트 최신화
		public void ApplyLine()
		{
			colEdge.SetPoints(listLinePos);
			rdrLine.SetPosition(0, listLinePos[0]);
			rdrLine.SetPosition(1, listLinePos[1]);
		}

		// 작성중인 사냥선으로 적용
		public void ApplyDrawLine(bool isDrawing)
		{
			// 타입 설정
			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;
			gameObject.layer = isDrawing ? CollideLayer.HuntLineDrawing :CollideLayer.HuntLineDrawings;

			colEdge.isTrigger = true;
		}

		// 사냥터 외곽선으로 적용
		public void ApplyHuntZone(bool isHole)
		{
			// 타입 설정
			gameObject.layer = CollideLayer.HuntLineDrawed;

			iObjectType = isHole ?
				GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole :
				GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline;

			// Edge Collider 사이값 보정
			List<Vector2> listOutlineCollider = new List<Vector2>(listLinePos);
			for (int i = 0; i < 2; ++i)
			{
				Vector2 vec2Edge = listOutlineCollider[i];

				listOutlineCollider[i] = vec2Edge - (vec2pNormal * GVar.Zone.c_fHuntZoneOutlineNarrowInterval);
			}

			colEdge.SetPoints(listOutlineCollider);
			colEdge.isTrigger = SceneMain_Battle.Single.charPlayer.behaviorOwn.isDownHuntLineBtn;

			rdrLine.SetPosition(0, listOutlineCollider[0]);
			rdrLine.SetPosition(1, listOutlineCollider[1]);
		}

		// 이전 사냥선 지점에 대한 각도 계산
		public void CalcOwnAngle(bool isCalcNextPointAngle)
		{
			// 컨테이너 내 기존 각도 제거
			if (null != hlContainer)
			{
				hlContainer.fWindingAngle -= this.fDegreeByPrevPoint;
			}

			Battle_HuntLinePoint hlpPrev = PointPrev;

			// 0번째 Index는 각도 0
			// 그 외 계산
			if (null != hlpPrev)
			{
				this.fDegreeByPrevPoint = GetDegreeByPrevPoint;
			}

			if (true == isCalcNextPointAngle)
			{
				PointNext?.CalcOwnAngle(false);
			}

			// 컨테이너 내 새 각도 입력
			if (null != hlContainer)
			{
				hlContainer.fWindingAngle += this.fDegreeByPrevPoint;
			}
		}

#if _debug
		private void OnDrawGizmos()
		{
			if (null != SceneMain_Battle.Single)
			{
				if (SceneMain_Battle.Single.mcsHuntZone.isDebug && iObjectType == GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline)
				{
					Gizmos.color = new Color(1, 0, 0);

					Vector3 vec3From = transform.position;
					Vector3 vec3To;

					vec3From.x -= (vec2Interval.x / 2f);
					vec3From.y -= (vec2Interval.y / 2f);

					vec3To = vec3From;
					vec3To.x += vec2pNormal.x * 0.1f;
					vec3To.y += vec2pNormal.y * 0.1f;

					Gizmos.DrawLine(vec3From, vec3To);
				}
			}
		}
	}
#endif
}

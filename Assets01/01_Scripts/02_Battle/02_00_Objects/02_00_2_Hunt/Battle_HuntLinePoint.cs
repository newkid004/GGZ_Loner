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
		public Vector2 vec2pNormal;	 // ���� ����
		public int iDirection8;			// 

		[Header("Info : Container")]
		public Battle_HuntLineContainer hlContainer;
		public int iContainIndex;
		public float fDegreeByPrevPoint;	// ���� ��ɼ� ������ ���� ����

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

		// ��ġ�� ��ȯ : �����̳� ��ġ ����
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

		// ���� ������
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

		// ���� ����
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

		// ���� ����
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

		// ���� �� ����
		public void RefreshLineInfo()
		{
			// ��ġ ���� ����
			if (0 == iContainIndex)
			{
				listLinePos[0] = Vector2.zero; // -transform.localPosition;
			}
			else
			{
				listLinePos[0] = PointPrev.transform.localPosition - transform.localPosition;
			}
		}

		// �� ���� ���� : ������ ���а��� ���� ��� / �浹 ������Ʈ �ֽ�ȭ
		public void ApplyLine()
		{
			colEdge.SetPoints(listLinePos);
			rdrLine.SetPosition(0, listLinePos[0]);
			rdrLine.SetPosition(1, listLinePos[1]);
		}

		// �ۼ����� ��ɼ����� ����
		public void ApplyDrawLine(bool isDrawing)
		{
			// Ÿ�� ����
			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;
			gameObject.layer = isDrawing ? CollideLayer.HuntLineDrawing :CollideLayer.HuntLineDrawings;

			colEdge.isTrigger = true;
		}

		// ����� �ܰ������� ����
		public void ApplyHuntZone(bool isHole)
		{
			// Ÿ�� ����
			gameObject.layer = CollideLayer.HuntLineDrawed;

			iObjectType = isHole ?
				GlobalDefine.ObjectData.ObjectType.ciHuntZoneHole :
				GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline;

			// Edge Collider ���̰� ����
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

		// ���� ��ɼ� ������ ���� ���� ���
		public void CalcOwnAngle(bool isCalcNextPointAngle)
		{
			// �����̳� �� ���� ���� ����
			if (null != hlContainer)
			{
				hlContainer.fWindingAngle -= this.fDegreeByPrevPoint;
			}

			Battle_HuntLinePoint hlpPrev = PointPrev;

			// 0��° Index�� ���� 0
			// �� �� ���
			if (null != hlpPrev)
			{
				this.fDegreeByPrevPoint = GetDegreeByPrevPoint;
			}

			if (true == isCalcNextPointAngle)
			{
				PointNext?.CalcOwnAngle(false);
			}

			// �����̳� �� �� ���� �Է�
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

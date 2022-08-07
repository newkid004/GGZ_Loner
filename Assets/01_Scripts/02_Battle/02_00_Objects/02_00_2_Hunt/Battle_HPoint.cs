using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_HPoint : Battle_BaseObject
	{
		[Header("----- H Point -----")]
		public LineRenderer rdrLine;
		public EdgeCollider2D colEdge;

		[Header("Info : Zone")]
		public Vector2 vec2pNormal;
		public int iDirection8;

		[Header("Info : Line")]
		public Battle_HLine hLine;
		public int iContainIndex;
		public float fDegreeByPrevPoint;    // ���� ��ɼ� ������ ���� ����

		[Header("Info : Point")]
		public List<Vector2> listLinePos = new List<Vector2>(new Vector2[] { Vector2.zero, Vector2.zero });

		public Vector2 vec2Interval { get => -listLinePos[0]; }
		public Vector2 vec2Direction { get => vec2Interval.normalized; }
		public int iDirectionDraw { get => Direction8.GetDirectionToNormal(vec2Direction); }
		public int iDirectionNormal { get => Direction8.GetDirectionToNormal(vec2pNormal); }

		public Battle_HPoint PointPrev => hLine.GetLinePoint(iContainIndex - 1);
		public Battle_HPoint PointNext => hLine.GetLinePoint(iContainIndex + 1);
		public Battle_HPoint PointPrevCircle => hLine.GetLinePoint(iContainIndex - 1, true);
		public Battle_HPoint PointNextCircle => hLine.GetLinePoint(iContainIndex + 1, true);

		public float Length => listLinePos[0].magnitude;

		public Vector2 PosWorld
		{
			get => transform.position;
			set
			{
				transform.position = value;
				RefreshLineInfo();
				ApplyLine();

				Battle_HPoint pNext = PointNext;

				if (gameObject.layer != CollideLayer.HuntLineDrawing)
				{
					if (this != pNext)
					{
						pNext.RefreshLineInfo();
						pNext.ApplyLine();
					}
				}

				if (pNext)
				{
					CalcOwnAngle(this != pNext);
				}
			}
		}

		public Vector2 PosLocal
		{
			get => transform.localPosition;
			set
			{
				transform.localPosition = value;
				RefreshLineInfo();
				ApplyLine();

				Battle_HPoint pNext = PointNext;

				if (gameObject.layer != CollideLayer.HuntLineDrawing)
				{
					if (this != pNext)
					{
						pNext.RefreshLineInfo();
						pNext.ApplyLine();
					}
				}

				if (pNext)
				{
					CalcOwnAngle(this != pNext);
				}
			}
		}

		protected override void Init()
		{
			base.Init();
			Reset();
		}

		public void Reset()
		{
			hLine = null;
			iContainIndex = -1;
			vec2pNormal = Vector2.zero;
			fDegreeByPrevPoint = 0;

			for (int i = 0; i < 2; ++i)
			{
				listLinePos[0] = Vector2.zero;
			}

			iObjectType = GlobalDefine.ObjectData.ObjectType.ciHuntLine;
		}

		public override void OnPushedToPool()
		{
			Battle_BehaviourPlayer bhvPlayer = SceneMain_Battle.Single.charPlayer.behaviorOwn;
			bhvPlayer.bhvHuntline.OnExitHuntZoneOutline(this);

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
			if (gameObject.layer == CollideLayer.HuntLineDrawing)
			{
				if (iContainIndex == 0)
				{
					listLinePos[0] = -transform.localPosition;
				}
				else
				{
					listLinePos[0] = PointPrev.transform.localPosition - transform.localPosition;
				}
			}
			else
			{
				if (iContainIndex == 0)
				{
					listLinePos[0] = -transform.localPosition;
				}
				else
				{
					listLinePos[0] = PointPrev.transform.localPosition - transform.localPosition;
				}
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
			gameObject.layer = isDrawing ? CollideLayer.HuntLineDrawing : CollideLayer.HuntLineDrawings;

			colEdge.isTrigger = true;
		}

		// ����� �ܰ������� ����
		public void ApplyHuntZone(bool isHole)
		{
			// Ÿ�� ����
			gameObject.layer = CollideLayer.HuntLineDrawed;

			iObjectType |= isHole ?
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
			colEdge.isTrigger = SceneMain_Battle.Single.charPlayer.behaviorOwn.bhvHuntline.isDownHuntLineBtn;

			rdrLine.SetPosition(0, listOutlineCollider[0]);
			rdrLine.SetPosition(1, listOutlineCollider[1]);
		}

		// ���� ��ɼ� ������ ���� ���� ���
		public void CalcOwnAngle(bool isCalcNextPointAngle, int iSetDirection = Direction8.ciDir_5)
		{
			// �����̳� �� ���� ���� ����
			if (null != hLine)
			{
				hLine.fWindingAngle -= this.fDegreeByPrevPoint;
			}

			Battle_HPoint hpPrev = PointPrev;

			// 0��° Index�� ���� 0
			// �� �� ���
			if (this != hpPrev)
			{
				if (this.iContainIndex != 0)
				{
					if (iSetDirection == Direction8.ciDir_5)
					{
						this.fDegreeByPrevPoint = Vector2.SignedAngle(PointPrev.vec2Interval.normalized, vec2Interval.normalized);
					}
					else
					{
						this.fDegreeByPrevPoint = Vector2.SignedAngle(PointPrev.vec2Interval.normalized, Direction8.GetNormalByDirection(iSetDirection));
					}
				}
				else
				{
					this.fDegreeByPrevPoint = 0;
				}
			}
			else
			{
				this.fDegreeByPrevPoint = 0;
			}

			if (true == isCalcNextPointAngle)
			{
				Battle_HPoint hpNext = PointNext;

				if (this != hpNext)
				{
					hpNext.CalcOwnAngle(false);
				}
			}

			// �����̳� �� �� ���� �Է�
			if (null != hLine)
			{
				hLine.fWindingAngle += this.fDegreeByPrevPoint;
			}
		}

#if _debug
		private void OnDrawGizmos()
		{
			if (null != SceneMain_Battle.Single)
			{
				if (SceneMain_Battle.Single.mcsHZone.isDebug && GlobalUtility.Digit.Include(iObjectType, GlobalDefine.ObjectData.ObjectType.ciHuntZoneOutline))
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

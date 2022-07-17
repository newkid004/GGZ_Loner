using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Proto_00_N
{
	using GlobalDefine;
	using System;

	public class Battle_HuntZone : Battle_BaseObject
	{
		[Header("----- Hunt Zone -----")]
		[Header("Own Component")]
		public MeshFilter filMesh;
		public MeshRenderer rdrMesh;

		public Vector2 vec2Center;													// �߽� ��ġ
		public List<List<Battle_HuntLinePoint>> listDirectionalPoint;				// �Ⱥи� ���� ����� ĳ��

		[Header("Ref : HuntZoneOutline")]
		public Battle_HuntLineContainer hlcEdge;									// �ܺ� ����� �ܰ���
		
		[Header("Ref : HuntZoneHole")]
		public HashSet<Battle_HuntLineContainer> hsHlcHoles;						// ���� ���� �ܰ���
		public Dictionary<Battle_HuntLineContainer, Battle_HuntZone> dictHzInHole;	// ���� �� �����

		// �ﰢ ��� ����
		public Poly2Mesh.Polygon p2mPolygon { get; private set; }

		protected override void Init()
		{
			base.Init();

			vec2Center = Vector2.zero;
			listDirectionalPoint = new List<List<Battle_HuntLinePoint>>(10);
			for (int i = 0; i < 10; ++i)
			{
				listDirectionalPoint.Add(new List<Battle_HuntLinePoint>());
			}

			hsHlcHoles = new HashSet<Battle_HuntLineContainer>();
			dictHzInHole = new Dictionary<Battle_HuntLineContainer, Battle_HuntZone>();

			base.iObjectType |= ObjectData.ObjectType.ciHuntZone;
			// base.iAttribute |= ObjectData.Attribute.ciBasic_Character;

			p2mPolygon = new Poly2Mesh.Polygon();
		}

		public void Reset()
		{
			vec2Center = Vector2.zero;
			listDirectionalPoint.ForEach(list => list.Clear());

			hlcEdge.Push();
			hlcEdge = null;

			dictHzInHole.ForEach((hlp, hz) => hz.Push());
			dictHzInHole.Clear();

			hsHlcHoles.ForEach(hlc => Push());
			hsHlcHoles.Clear();

			p2mPolygon.outside.Clear();
			p2mPolygon.holes.Clear();
			p2mPolygon.planeNormal = Vector3.zero;
			p2mPolygon.rotation = Quaternion.identity;
		}

		public override void OnPushedToPool()
		{
			Reset();
			base.OnPushedToPool();
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			filMesh = GetComponent<MeshFilter>();
			if (null == filMesh.mesh)
			{
				filMesh.mesh = new Mesh();
			}

			rdrMesh = GetComponent<MeshRenderer>();
		}

		public void CalcEdgePoints()
		{
			if (null == hlcEdge)
				return;

			int iCount;

			p2mPolygon.outside.Clear();
			iCount = hlcEdge.listVec2Point.Count;
			for (int i = 1; i < iCount; ++i)
			{
				p2mPolygon.outside.Add(hlcEdge.listVec2Point[i]);
			}

			p2mPolygon.holes.Clear();
			hsHlcHoles.ForEach(hlcHole =>
			{
				List<Vector3> listHolePoint = new List<Vector3>();
				Vector2 vec2LocalPos = hlcHole.transform.localPosition;

				iCount = hlcHole.listVec2Point.Count;
				for (int i = 1; i < iCount; ++i)
				{
					listHolePoint.Add(hlcHole.listVec2Point[i] + vec2LocalPos);
				}
				p2mPolygon.holes.Add(listHolePoint);
				hlcHole.colPoly.SetPath(0, hlcHole.listVec2Point);
			});

			hlcEdge.colPoly.SetPath(0, hlcEdge.listVec2Point);
		}

		public void CalcMeshZone()
		{
			filMesh.mesh = Poly2Mesh.CreateMesh(p2mPolygon);
		}

		// ���� Zone�� ��ġ�� ��ü Ž�� / ȹ��
		public bool SearchPlacedObject(out List<ValueTuple<Battle_BaseObject, int>> listOutput, int iObjectTypeFilterFlag, int iObjectLayer = CollideLayer.flagZoneIntersectBasic)
		{
			/* <Ʃ�� ����>
			 * 1 : Battle_BaseObject	: ��ġ�� ��ü
			 * 2 : int					: ��ġ�� ��ü Ÿ��
			 */

			listOutput = null;

			List<Collider2D> listCol = new List<Collider2D>();
			ContactFilter2D conFilter = new ContactFilter2D();
			conFilter.useTriggers = true;
			conFilter.useLayerMask = true;
			conFilter.SetLayerMask(iObjectLayer);

			int iContactCount = hlcEdge.colPoly.OverlapCollider(conFilter, listCol);
			if (0 < iContactCount)
			{
				listOutput = new List<(Battle_BaseObject, int)>();
				Bounds bndOwn = hlcEdge.colPoly.bounds;

				for (int i = 0; i < iContactCount; ++i)
				{
					Collider2D col = listCol[i];
					Battle_BaseObject obj = col.gameObject.GetComponent<Battle_BaseObject>();

					if (obj == null || 0 == (obj.iObjectType & iObjectTypeFilterFlag))
						continue;

					listOutput.Add((obj, obj.iObjectType));
				}
			}

			return null == listOutput ? false : (0 < listOutput.Count);
		}

		public override void TriggeredByHuntZoneExtendPlaced(Battle_HuntZone hzSpawned, List<Vector2> listExtendPoint)
		{
			base.TriggeredByHuntZoneExtendPlaced(hzSpawned, listExtendPoint);
			// Container���� ȣ�� �� ( Push ����!! )
		}

		public override void TriggeredByHuntZoneSpawnPlaced(Battle_HuntZone hzSpawned)
		{
			base.TriggeredByHuntZoneSpawnPlaced(hzSpawned);
			// Container���� ȣ�� �� ( Push ����!! )
		}

		public override void TriggeredByHuntZoneDamagedPlaced(Battle_HuntZone hzSpawned)
		{
			base.TriggeredByHuntZoneDamagedPlaced(hzSpawned);

#if _debug
			// �׽�Ʈ : ��¦��
			Color clrSource = rdrMesh.material.color;
			rdrMesh.material.color = Color.red;
			rdrMesh.material.DOColor(clrSource, 0.5f);
#endif
		}
	}
}
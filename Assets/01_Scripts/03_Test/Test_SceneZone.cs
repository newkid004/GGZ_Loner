using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Test_SceneZone : MonoBehaviour
	{
		[SerializeField] private ObjectPool<Test_PointView> poolPoint = new ObjectPool<Test_PointView>();
		[SerializeField] private ObjectPool<Test_Zone> poolZone = new ObjectPool<Test_Zone>();

		private HashSet<Test_Zone> hsAddZone = new HashSet<Test_Zone>();

		private bool isInputMode = true;
		public int iPointCount = 50;

		public void Awake()
		{
			poolPoint.Init();
			poolZone.Init();
		}

		public void CreateZone(Test_Zone zoneParent)
		{
			if (zoneParent == poolZone.tOrigin)
			{
				hsAddZone.Add(poolZone.Pop());
			}
			else
			{
				zoneParent.listInnerZone.Add(poolZone.Pop(zoneParent.transform));
			}
		}

		public void ChangeInputView()
		{
			isInputMode = !isInputMode;

			hsAddZone.ForEach(zone =>
			{
				zone.colPolyMain.gameObject.SetActive(isInputMode);
				zone.colPolyHole.gameObject.SetActive(isInputMode);
				zone.colPolyResult.gameObject.SetActive(!isInputMode);

				if (isInputMode)
				{
					ApplyMeshToPolygon(zone.colPolyMain);
					ApplyMeshToPolygon(zone.colPolyHole);
				}
				else
				{
					ApplyMeshToPolygon(zone.colPolyResult);
				}
			});
		}

		public void Execute()
		{
			hsAddZone.ForEach(zone => zone.Execute());
		}

		public void ResetRandomPointsInResultMesh()
		{
			poolPoint.CollectAllObject();
		}

		public void AddRandomPointsInResultMesh()
		{
			List<Vector2> listPolygonTriangleTotal = new List<Vector2>();

			hsAddZone.ForEach(zone =>
			{
				zone.colPolyResult.GenerateMeshInfo(out Mesh mesh, out List<Vector2> listPolygonTriangle);
				listPolygonTriangleTotal.AddRange(listPolygonTriangle);
			});

			for (int i = 0; i < iPointCount; ++i)
			{
				Test_PointView pw = poolPoint.Pop();
				pw.transform.position = GlobalUtility.Trigonometric.GetRandomPointInPolygon(listPolygonTriangleTotal, pw.transform);
			}
		}

		private void ApplyMeshToPolygon(PolygonCollider2D poly)
		{
			MeshFilter meshFilter = poly.GetComponent<MeshFilter>();
			meshFilter.mesh = poly.CreateMesh(false, false);
		}
	}
}
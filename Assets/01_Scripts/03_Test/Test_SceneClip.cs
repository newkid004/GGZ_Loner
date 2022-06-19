using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace GGZ
{
	public class Test_SceneClip : MonoBehaviour
	{
		public float fRatio = 100f;

		public PolygonCollider2D objMain;
		public PolygonCollider2D objSub;
		public PolygonCollider2D objResult;

		private bool isInputMode = true;

		public int iPointCount = 50;

		private ClipperLib.Clipper clipper = new ClipperLib.Clipper();

		[SerializeField] private ObjectPool<Test_PointView> poolPoint = new ObjectPool<Test_PointView>();

		private void Awake()
		{
			ApplyMeshToPolygon(objMain);
			ApplyMeshToPolygon(objSub);
			ApplyMeshToPolygon(objResult);

			objResult.gameObject.SetActive(false);

			poolPoint.Init();
		}

		public void InputPolygon()
		{
			Paths pathMain = GlobalUtility.Clipper.GetPathToPolygon(objMain);
			Paths pathSub = GlobalUtility.Clipper.GetPathToPolygon(objSub);

			clipper.Clear();

			clipper.AddPaths(pathMain, ClipperLib.PolyType.ptSubject, true);
			clipper.AddPaths(pathSub, ClipperLib.PolyType.ptClip, true);
		}

		public void ExecuteClip(int iBoolOper)
		{
			InputPolygon();

			Paths pathsResult = new Paths();
			clipper.Execute((ClipperLib.ClipType)iBoolOper, pathsResult);

			GlobalUtility.Clipper.SetPolygonToPath(pathsResult, objResult);
			ApplyMeshToPolygon(objResult);
		}

		public void ChangeInputView()
		{
			isInputMode = !isInputMode;

			objMain.gameObject.SetActive(isInputMode);
			objSub.gameObject.SetActive(isInputMode);

			objResult.gameObject.SetActive(!isInputMode);

			if (isInputMode)
			{
				ApplyMeshToPolygon(objMain);
				ApplyMeshToPolygon(objSub);
			}
			else
			{
				ApplyMeshToPolygon(objResult);
			}
		}

		public void ResetRandomPointsInResultMesh()
		{
			poolPoint.CollectAllObject();
		}

		public void AddRandomPointsInResultMesh()
		{
			objResult.GenerateMeshInfo(out Mesh mesh, out List<Vector2> listPolygonTriangle);

			for (int i = 0; i < iPointCount; ++i)
			{
				Test_PointView pw = poolPoint.Pop();

				pw.transform.position = GlobalUtility.Trigonometric.GetRandomPointInPolygon(listPolygonTriangle, pw.transform);
			}
		}

		private void ApplyMeshToPolygon(PolygonCollider2D poly)
		{
			MeshFilter meshFilter = poly.GetComponent<MeshFilter>();
			meshFilter.mesh = poly.CreateMesh(false, false);
		}
	}
}
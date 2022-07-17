using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class Test_Clip : MonoBehaviour
{
	public float fRatio = 100f;

	public PolygonCollider2D objMain;
	public PolygonCollider2D objSub;
	public PolygonCollider2D objResult;

	public bool isInputMode = true;

	private ClipperLib.Clipper clipper = new ClipperLib.Clipper();

	private void Awake()
	{
		ApplyMeshToPolygon(objMain);
		ApplyMeshToPolygon(objSub);
		ApplyMeshToPolygon(objResult);

		objResult.gameObject.SetActive(false);
	}

	public void InputPolygon()
	{
		Paths pathMain = GetPathToPolygon(objMain);
		Paths pathSub = GetPathToPolygon(objSub);

		clipper.Clear();

		clipper.AddPaths(pathMain, ClipperLib.PolyType.ptSubject, true);
		clipper.AddPaths(pathSub, ClipperLib.PolyType.ptClip, true);
	}

	public void ExecuteClip(int iBoolOper)
	{
		InputPolygon();

		Paths pathsResult = new Paths();
		clipper.Execute((ClipperLib.ClipType)iBoolOper, pathsResult);

		SetPolygonToPaths(pathsResult, objResult);
		ApplyMeshToPolygon(objResult);
	}

	public void ChangeInputMode()
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

	private Paths GetPathToPolygon(PolygonCollider2D poly)
	{
		Paths paths = new Paths();

		Transform trPoly = poly.transform;
		int iPathCount = poly.pathCount;

		for (int i = 0; i < iPathCount; i++)
		{
			Vector2[] vec2Points = poly.GetPath(i);

			Path path = new Path();

			foreach (Vector2 point in vec2Points)
			{
				Vector2 vec2TranPoint = trPoly.TransformPoint(point * fRatio);

				path.Add(new ClipperLib.IntPoint(
					vec2TranPoint.x,
					vec2TranPoint.y));
			}

			paths.Add(path);
		}

		return paths;
	}

	private void SetPolygonToPaths(Paths paths, PolygonCollider2D poly)
	{
		Transform trPoly = poly.transform;
		poly.pathCount = paths.Count;

		for (int i = 0; i < paths.Count; i++)
		{
			Vector2[] vec2Points = new Vector2[paths[i].Count];

			for (int j = 0; j < paths[i].Count; j++)
			{
				Vector2 vec2Point = new Vector2(paths[i][j].X, paths[i][j].Y);
				Vector2 vec2InTranPoint = trPoly.InverseTransformPoint(vec2Point);

				vec2InTranPoint /= fRatio;

				vec2Points[j] = vec2InTranPoint;
			}

			poly.SetPath(i, vec2Points);
		}
	}

	private void ApplyMeshToPolygon(PolygonCollider2D poly)
	{
		MeshFilter meshFilter = poly.GetComponent<MeshFilter>();
		meshFilter.mesh = poly.CreateMesh(false, false);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace GGZ
{ 
	public class Test_Zone : PooledObject
	{
		public PolygonCollider2D colPolyMain;
		public PolygonCollider2D colPolyHole;
		public PolygonCollider2D colPolyResult;

		public List<Test_Zone> listInnerZone = new List<Test_Zone>();

		public ClipperLib.Clipper clipper = new ClipperLib.Clipper();

		public void Execute()
		{
			if (colPolyHole.pathCount == 0)
			{
				List<Vector2> listPath = new List<Vector2>();
				int iPathCount = colPolyMain.pathCount;
				colPolyResult.pathCount = iPathCount;

				for (int i = 0; i < iPathCount; i++)
				{
					colPolyMain.GetPath(i, listPath);
					colPolyResult.SetPath(i, listPath);
				}
			}
			else
			{
				Paths pathMain = GlobalUtility.Clipper.GetPathToPolygon(colPolyMain);
				Paths pathHole = GlobalUtility.Clipper.GetPathToPolygon(colPolyHole);

				clipper.Clear();

				clipper.AddPaths(pathMain, ClipperLib.PolyType.ptSubject, true);
				clipper.AddPaths(pathHole, ClipperLib.PolyType.ptClip, true);

				Paths pathSolution = new Paths();
				bool bExecuteResult = clipper.Execute(ClipperLib.ClipType.ctDifference, pathSolution);
				clipper.Clear();

				if (0 < listInnerZone.Count)
				{
					clipper.AddPaths(pathSolution, ClipperLib.PolyType.ptSubject, true);
					listInnerZone.ForEach(zone =>
					{
						zone.gameObject.SetActive(true);
						zone.Execute();
						clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(zone.colPolyResult), ClipperLib.PolyType.ptClip, true);
						zone.gameObject.SetActive(false);
					});

					clipper.Execute(ClipperLib.ClipType.ctUnion, pathSolution);
				}

				GlobalUtility.Clipper.SetPolygonToPath(pathSolution, colPolyResult);
			}
		}
	}
}

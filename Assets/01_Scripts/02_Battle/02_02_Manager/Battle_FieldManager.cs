using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace GGZ
{
	using GlobalDefine;

	[System.Serializable]
	public class Battle_FieldManager
	{
		public bool IsDebug;

		public Transform trCurrentBattleField;
		public Transform trCharacterGround;

		public Battle_BaseField fldCurrentField;

		public PolygonCollider2D colFieldTotal;
		private List<Vector2> listFieldTriangles = new List<Vector2>();

		public Rect rtFieldSpace;

		public Clipper clipper { get; private set; } = new Clipper();

		public void Init()
		{
			InitPlayerCharacterPos();
		}

		private void InitPlayerCharacterPos()
		{
			Battle_CharacterPlayer charPlayer = SceneMain_Battle.Single.charPlayer;

			// 초기 사냥터에 플레이어 위치 설정
			Battle_HZone hzFirst = SceneMain_Battle.Single.mcsHZone.zoneStart;

			Vector2 vec2MinPos = new Vector2();
			Vector2 vec2MaxPos = new Vector2();
			Vector2 vec2IntervalHarf;
			foreach (Battle_HPoint hlp in hzFirst.lineEdge.listPoint)
			{
				Vector2 vec2Position = hlp.PosWorld;

				vec2MinPos.x = Mathf.Min(vec2Position.x, vec2MinPos.x);
				vec2MinPos.y = Mathf.Min(vec2Position.y, vec2MinPos.y);
			
				vec2MaxPos.x = Mathf.Max(vec2Position.x, vec2MaxPos.x);
				vec2MaxPos.y = Mathf.Max(vec2Position.y, vec2MaxPos.y);
			}

			vec2IntervalHarf = (vec2MaxPos - vec2MinPos) / 2f;

			float fScale = 0.85f;

			charPlayer.transform.position = new Vector3(
				vec2MinPos.x + vec2IntervalHarf.x + Random.Range(-vec2IntervalHarf.x, vec2IntervalHarf.x) * fScale,
				vec2MinPos.y + vec2IntervalHarf.y + Random.Range(-vec2IntervalHarf.y, vec2IntervalHarf.y) * fScale,
				0 );
		}

		public void ApplyCreateZone(Battle_HZone hZone)
		{
			clipper.Clear();

			clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(colFieldTotal), PolyType.ptSubject, true);
			clipper.AddPaths(GlobalUtility.Clipper.GetPathToPolygon(hZone.lineEdge.colPoly), PolyType.ptClip, true);

			List<List<IntPoint>> pathsResult = new List<List<IntPoint>>();
			clipper.Execute(ClipType.ctDifference, pathsResult);

			GlobalUtility.Clipper.SetPolygonToPath(pathsResult, colFieldTotal);

			colFieldTotal.GenerateMeshInfo(out Mesh mesh, out listFieldTriangles);
		}

		public Vector2 GetOutsideRandomPos(Transform tran = null)
		{
			return GlobalUtility.Trigonometric.GetRandomPointInPolygon(listFieldTriangles, tran);
		}
	}
}
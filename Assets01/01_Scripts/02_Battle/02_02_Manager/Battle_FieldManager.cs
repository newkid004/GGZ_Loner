using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	using GlobalDefine;
	using Habrador_Computational_Geometry;
	using Habrador_Computational_Geometry.MeshAlgorithms;

	[System.Serializable]
	public class Battle_FieldManager
	{
		public bool IsDebug;

		public Transform trCurrentBattleField;

		public Battle_BaseField fldCurrentField;

		public PolygonCollider2D colFieldTotal;

		public Rect rtFieldSpace;

		public void Init()
		{
			InitPlayerCharacter();
		}

		private void InitPlayerCharacter()
		{
			Battle_CharacterPlayer charPlayer = SceneMain_Battle.Single.charPlayer;

			// 초기 사냥터에 플레이어 위치 설정
			Battle_HuntZone hzFirst = SceneMain_Battle.Single.mcsHuntZone.hzStart;

			float fScale = 0.85f;
			Vector2 vec2MinPos = new Vector2();
			Vector2 vec2MaxPos = new Vector2();
			foreach (Battle_HuntLinePoint hlp in hzFirst.hlcEdge.listLinePoint)
			{
				Vector2 vec2Position = hlp.vec2LocalPosition;

				vec2MinPos.x = Mathf.Min(vec2Position.x, vec2MinPos.x) * fScale;
				vec2MinPos.y = Mathf.Min(vec2Position.y, vec2MinPos.y) * fScale;
			
				vec2MaxPos.x = Mathf.Max(vec2Position.x, vec2MaxPos.x) * fScale;
				vec2MaxPos.y = Mathf.Max(vec2Position.y, vec2MaxPos.y) * fScale;
			}

			charPlayer.transform.localPosition = new Vector3(
				Random.Range(vec2MinPos.x, vec2MaxPos.x) + hzFirst.transform.localPosition.x,
				Random.Range(vec2MinPos.y, vec2MaxPos.y) + hzFirst.transform.localPosition.y,
				0 );
		}

		public Vector2 GetWorldToField(Vector2 vec2) => trCurrentBattleField.InverseTransformPoint(vec2);
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public class SceneMain_Battle : MonoBehaviour
	{
		public static SceneMain_Battle Single { get; private set; }

		[Header("----- Manager -----")]
		[SerializeField] private Battle_MonsterManager _mcsMonster;
		[SerializeField] private Battle_HuntLineManager _mcsHuntLine;
		[SerializeField] private Battle_HuntZoneManager _mcsHuntZone;
		[SerializeField] private Battle_FieldManager _mcsField;

		public Battle_MonsterManager mcsMonster { get => _mcsMonster; }
		public Battle_HuntLineManager mcsHuntLine { get => _mcsHuntLine; }
		public Battle_HuntZoneManager mcsHuntZone { get => _mcsHuntZone; }
		public Battle_FieldManager mcsField { get => _mcsField; }

		[Header("----- Character -----")]
		[SerializeField] private Transform trCharParent;

		[Header("----- InGame -----")]
		[SerializeField] public Camera camMain;

		public Battle_CharacterPlayer charPlayer;

		private void Awake()
		{
			Single = this;

			Init();
		}

		private void Init()
		{
			CustomRoutine.Init();
			_mcsMonster.Init();
			_mcsHuntLine.Init();

			_mcsHuntZone.Init();
			_mcsField.Init();
		}

		public void SpawnMonsterTest()
		{
			Battle_TestMonster mon = _mcsMonster.PopObj<Battle_TestMonster>(trCharParent);

			mon.transform.position += new Vector3(
				Random.Range(-3.0f, 3.0f),
				Random.Range(-3.0f, 3.0f),
				0);
		}

		private void OnDrawGizmos()
		{
			//*/ // Draw Field
			if (_mcsField.IsDebug && (_mcsField.fldCurrentField != null && _mcsField.trCurrentBattleField != null))
			{
				OnDrawGizmosField();
			}
			//*/
		}

		private void OnDrawGizmosField()
		{
			Rect rtDrawSpace = _mcsField.rtFieldSpace;
			Vector3 vec3Pos = _mcsField.trCurrentBattleField.position;

			rtDrawSpace.position = new Vector2(
				rtDrawSpace.x + vec3Pos.x,
				rtDrawSpace.y + vec3Pos.y);

			Gizmos.color = new Color(1, 1, 0, 0.5f);
			Gizmos.DrawCube(rtDrawSpace.position, rtDrawSpace.size);
			Gizmos.color = new Color(1, 0, 0, 0.5f);
			Gizmos.DrawSphere(rtDrawSpace.position, 0.1f);
		}
	}
}

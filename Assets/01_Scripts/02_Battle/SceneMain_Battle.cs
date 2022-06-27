using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class SceneMain_Battle : MonoBehaviour
	{
		public static SceneMain_Battle Single { get; private set; }

		[Header("----- Manager -----")]
		[SerializeField] private Battle_MonsterManager		_mcsMonster;
		[SerializeField] private Battle_HLineManager		_mcsHLine;
		[SerializeField] private Battle_HZoneManager		_mcsHZone;
		[SerializeField] private Battle_FieldManager		_mcsField;
		[SerializeField] private Battle_BulletManager		_mcsBullet;
		[SerializeField] private Battle_SkillManager		_mcsSkill;
		[SerializeField] private Battle_BuffManager			_mcsBuff;
		[SerializeField] private Battle_PatternManager		_mcsPattern;

		public Battle_MonsterManager	mcsMonster	{ get => _mcsMonster; }
		public Battle_HLineManager		mcsHLine	{ get => _mcsHLine; }
		public Battle_HZoneManager		mcsHZone	{ get => _mcsHZone; }
		public Battle_FieldManager		mcsField	{ get => _mcsField; }
		public Battle_BulletManager		mcsBullet	{ get => _mcsBullet; }
		public Battle_SkillManager		mcsSkill	{ get => _mcsSkill; }
		public Battle_BuffManager		mcsBuff		{ get => _mcsBuff; }
		public Battle_PatternManager	mcsPattern	{ get => _mcsPattern; }

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
			_mcsHLine.Init();

			_mcsHZone.Init();
			_mcsField.Init();

			mcsBullet.Init();
		}

		private void FixedUpdate()
		{
			_mcsBuff.FixedUpdate(Time.fixedDeltaTime);
		}

		// Test
		public void SpawnMonsterTest()
		{
			Battle_TestMonster mon = _mcsMonster.PopObj<Battle_TestMonster>(trCharParent);
			mon.transform.position = mcsField.GetOutsideRandomPos(mon.transform);
		}

		public bool isAllyBullet = false;
		public void SpawnBullet(int iID)
		{
			isAllyBullet = !isAllyBullet;
			// Battle_BaseCharacter Owner = isAllyBullet ?
			// 	charPlayer : (Battle_BaseCharacter)mcsMonster.GetRandom();
			
			Battle_BaseCharacter Owner = mcsMonster.GetRandom();

			Battle_BaseBullet bullet = _mcsBullet.Create(iID, Owner);
			bullet.transform.position = mcsField.GetOutsideRandomPos(bullet.transform);
			bullet.TargetTo(charPlayer.transform.position);

			bullet.rdrSplite.color = isAllyBullet ?
				Color.blue : Color.red;
		}

#if _debug
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
#endif
}
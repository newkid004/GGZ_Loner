
#pragma warning disable 0414	// 사용되지 않는 변수 경고 무시

namespace GGZ
{
	public static class CSVData
	{
		public static class Battle
		{
			public static class Anim
			{
				public class AnimationData : CSVObjectBase<AnimationData, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public string[] SpriteNames;
					public float[] TimeLengths;
				}

				public class AnimationGroup : CSVObjectBase<AnimationGroup, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public string Naming;
					public int Type;
					public int UnitID;
					public int[] DataIDs;
				}
			}

			public static class Stage
			{
				public class Theme : CSVObjectBase<Theme, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public string Name;
					public int BossID;
					public int LevelID;
				}

				public class Level : CSVObjectBase<Level, int>
				{
					public override int GetKey() => ID;

					// Easy, Normal, Hard
					public int ID;
					public string[] Names;
					public int[] FieldIDs;
				}

				public class Field : CSVObjectBase<Field, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public int[] SpawnGroupIDs;
				}

				public class SpawnGroup : CSVObjectBase<SpawnGroup, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public int[] SpawnInfoIDs;
				}

				public class SpawnInfo : CSVObjectBase<SpawnInfo, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public int MonsterID;
					public int SpawnType;
					public int[] Params;

					/*
					 * SpawnType
					 * 
					 * 0 : 즉시 {0}마리 생성
					 */
				}
			}

			public static class Status
			{
				public class Boss : CSVObjectBase<Boss, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public string Name;

					public int Health;
					public int Attack;
					public int Defend;
				}

				public class Unit : CSVObjectBase<Unit, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public string Name;

					public int Health;
					public int Attack;
					public int Defend;

					public int AttackSpeed;
					public int MoveSpeed;

					public int Weight;
					public int AirHold;
				}

				public class Monster : CSVObjectBase<Monster, int>
				{
					public override int GetKey() => ID;

					public int ID;

					public int BehaviourGroupID;

					public float AttackRadius;

					public float AlertRadius;
					public float AlertTime;

					public float AggressiveMoveSpeed;

					public float MoveTime;
					public float WaitAfterMove;
				}
			}

			public static class UnitBehaviour
			{
				public class PatternData : CSVObjectBase<PatternData, int>
				{
					public override int GetKey() => ID;

					public int ID;

					public int Type;

					public float[] Params;
				}

				public class PatternGroup : CSVObjectBase<PatternGroup, int>
				{
					public override int GetKey() => ID;

					public int ID;

					public int[] Datas;
				}
			}

			public static class Skill
			{
				public class TargetingPreset : CSVObjectBase<TargetingPreset, int>
				{
					public override int GetKey() => ID;

					public int ID;

					/* Flag
					 * < 캐릭터 >
					 * 0 : 자기 자신
					 * 1 : 아군 (상대적)
					 * 2 : 적군 (상대적)
					 * 3 : 플레이어
					 * 4 : 몬스터
					 * 5 : 보스
					 * 
					 * < 지형 >
					 * 6 : 사냥터 ( 이미 제작된 사냥터 )
					 * 7 : 사냥선 ( 제작 중인 사냥선 )
					 * 
					 * < 기타 >
					 * 8 : 다른 탄환
					 */
					public int Flag;		// Digit

					public float HitInterval;
				}

				public class SkillActive : CSVObjectBase<SkillActive, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public int TargetingID;

					/* ActiveType
					 * 0 : Bullet 생성			- [BulletID / ]
					 * 1 : 버프 적용			- [BuffID / Time, Tick, Value] - Floats는 비율로 적용
					 * 2 : 또다른 Skill 발동	- [SkillID, .. / ]
					 */
					public int ActiveType;

					public int[] ParamInts;
					public float[] ParamFloats;
				}

				public class BulletInfo : CSVObjectBase<BulletInfo, int>
				{
					public override int GetKey() => ID;

					public int ID;
					public int TargetingID;
					public int SkillActiveID;

					public int LifeTime;

					/* MoveType
					 * 0 : 선형
					 */
					public int MoveType;
					public float Radius;
					public float Speed;

					public float Atk;
				}

				public class BuffInfo : CSVObjectBase<BuffInfo, int>
				{
					public override int GetKey() => ID;

					public int ID;

					public int BelongID;

					/* ActiveType - 각 수치가 적용되는 형식 분류
					 * 
					 * 0 : 단발 작용
					 * 1 : Time동안 Tick시간에 따라 작용
					 * 2 : 총 계수가 Time, Tick에 나누어 작용
					 * 3 : Tick이 Time을 비율로 나누어 고정된 횟수로 작용
					 */
					public int ActiveType;

					/* UpdateType - BelongID 에 따라 같은 버프가 적용되었을 때 갱신 형식 분류
					 * 
					 * 0 : 유일			- 갱신하지 않음
					 * 1 : 유일			- 새로운 버프로 대체
					 * 2 : 스택			- 넘을 경우 추가하지 않음
					 * 3 : 스택			- 넘을 경우 오래된 것 부터 제거
					 * 4 : 유일			- 가산 ( Time )
					 * 5 : 유일			- 가산 ( Value )
					 * 6 : 유일			- 가산 ( Value ), 갱신 ( Time )
					 * 7 : 유일			- 가산 ( Value ), 가산 ( Time )
					 */
					public int UpdateType;

					public int ActiveSkillIDBegin;
					public int ActiveSkillIDTick;
					public int ActiveSkillIDEnd;

					public int isHarm;			// 버프, 디버프 구분 ( 0 : 버프 / 1 : 디버프 )
					public int MaximumStack;

					public float ActValue;
					public float ActTime;
					public float ActTick;
				}
			}
		}

		public static class Main
		{
			public class String : CSVObjectBase<String, string>
			{
				public override string GetKey() => ID;

				public string ID;

				public string Kor;

				public string Current => Kor;
			}
		}
	}
}

#pragma warning restore 0414
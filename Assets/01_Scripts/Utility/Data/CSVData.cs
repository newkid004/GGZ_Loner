
#pragma warning disable 0414	// ������ �ʴ� ���� ��� ����

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
					 * 0 : ��� {0}���� ����
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
					 * < ĳ���� >
					 * 0 : �ڱ� �ڽ�
					 * 1 : �Ʊ� (�����)
					 * 2 : ���� (�����)
					 * 3 : �÷��̾�
					 * 4 : ����
					 * 5 : ����
					 * 
					 * < ���� >
					 * 6 : ����� ( �̹� ���۵� ����� )
					 * 7 : ��ɼ� ( ���� ���� ��ɼ� )
					 * 
					 * < ��Ÿ >
					 * 8 : �ٸ� źȯ
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
					 * 0 : Bullet ����			- [BulletID / ]
					 * 1 : ���� ����			- [BuffID / Time, Tick, Value] - Floats�� ������ ����
					 * 2 : �Ǵٸ� Skill �ߵ�	- [SkillID, .. / ]
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
					 * 0 : ����
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

					/* ActiveType - �� ��ġ�� ����Ǵ� ���� �з�
					 * 
					 * 0 : �ܹ� �ۿ�
					 * 1 : Time���� Tick�ð��� ���� �ۿ�
					 * 2 : �� ����� Time, Tick�� ������ �ۿ�
					 * 3 : Tick�� Time�� ������ ������ ������ Ƚ���� �ۿ�
					 */
					public int ActiveType;

					/* UpdateType - BelongID �� ���� ���� ������ ����Ǿ��� �� ���� ���� �з�
					 * 
					 * 0 : ����			- �������� ����
					 * 1 : ����			- ���ο� ������ ��ü
					 * 2 : ����			- ���� ��� �߰����� ����
					 * 3 : ����			- ���� ��� ������ �� ���� ����
					 * 4 : ����			- ���� ( Time )
					 * 5 : ����			- ���� ( Value )
					 * 6 : ����			- ���� ( Value ), ���� ( Time )
					 * 7 : ����			- ���� ( Value ), ���� ( Time )
					 */
					public int UpdateType;

					public int ActiveSkillIDBegin;
					public int ActiveSkillIDTick;
					public int ActiveSkillIDEnd;

					public int isHarm;			// ����, ����� ���� ( 0 : ���� / 1 : ����� )
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
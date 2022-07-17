
#pragma warning disable 0414	// 사용되지 않는 변수 경고 무시

namespace Proto_00_N
{
	public static class CSVData
	{
		public static class Battle
		{
			public static class Stage
			{
				public class Theme : CSVObjectBase<Theme>
				{
					public int ID;
					public int FieldID;
					public int Hardness;
					public int BossIndex;
					public string Name;
					public int[] EncounterArrayID;
				}

				public class Field : CSVObjectBase<Field>
				{
					public int ID;
					public string Name;
				}

				public class Encounter : CSVObjectBase<Encounter>
				{
					public int ID;
					public int MonsterID;
					public int SpawnType;
					public int[] Params;
				}

				public class Hardness : CSVObjectBase<Hardness>
				{
					public int ID;
					public string Name;
				}
			}

			public static class Status
			{
				public class Boss : CSVObjectBase<Boss>
				{
					int ID;
					string Name;

					int Health;
					int Attack;
					int Defend;
				}

				public class Monster : CSVObjectBase<Monster>
				{
					int ID;
					string Name;

					int Health;
					int Attack;
					int Defend;

					int AttackSpeed;
					int MoveSpeed;

					int Weight;
					int AirHold;
				}
			}
		}
	}
}

#pragma warning restore 0414
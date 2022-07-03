using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class Battle_PatternManager
	{
		public static Battle_PatternManager Single { get => SceneMain_Battle.Single.mcsPattern; }

		[SerializeField] private ObjectPool<Battle_BaseBehaviour> oPool = new ObjectPool<Battle_BaseBehaviour>();

		public void Init()
		{
			oPool.Init();
		}

		private Battle_BaseBehaviour PopToID(Battle_BaseMonster objMonster)
		{
			Battle_BaseBehaviour objResult;

			switch (objMonster.iCharacterID)
			{
				case 0: objResult = oPool.Pop<Battle_BehaviourPlayer>(objMonster.transform); break;
				default:
				{
					if (objMonster.iCharacterID < 10000)
					{
						objResult = oPool.Pop<Battle_BehaviourMonster>(objMonster.transform);
						// objResult = oPool.Pop<Battle_BehaviourWander>(objMonster.transform);
					}
					else
					{
						objResult = oPool.Pop(objMonster.transform);
					}
				}
				break;
			}

			return objResult;
		}

		public Battle_BehaviourMonster SetMonsterBehaviour(Battle_BaseMonster objMonster)
		{
			Battle_BehaviourMonster objResult = (Battle_BehaviourMonster)PopToID(objMonster);

			objResult.characterOwn = objMonster;
			objMonster.behaviorOwn = objResult;

			objResult.csvPatternGroup = CSVData.Battle.UnitBehaviour.PatternGroup.Manager.Get(objMonster.csvMonster.BehaviourGroupID);

			return objResult;
		}
	}
}
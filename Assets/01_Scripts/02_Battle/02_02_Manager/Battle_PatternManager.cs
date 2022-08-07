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
			objResult.InitPatternState();

			return objResult;
		}

		public void ProcessBeginPattern(Battle_BehaviourMonster bhvMonster, int iStateIndex, bool isChangeMoveState)
		{
			var csvPatternGroup = bhvMonster.csvPatternGroup;
			bhvMonster.csvPatternData = CSVData.Battle.UnitBehaviour.PatternData.Manager.Get(csvPatternGroup.Datas[iStateIndex]);

			switch (bhvMonster.csvPatternData.Type)
			{
				// ������ ����
				// 0 : PatternDataIndex - Min
				// 1 : PatternDataIndex - Max 
				case 0:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[Random.Range(0 , bhvMonster.csvPatternData.Params.Length)], true);
				}
				break;

				// ���
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 1:
				{
					bhvMonster.fStateTimeBasic = bhvMonster.csvPatternData.Params[0];
					bhvMonster.fStateTime = bhvMonster.fStateTimeBasic;

					if (isChangeMoveState)
					{
						bhvMonster.eMoveState = Battle_BehaviourMonster.EMoveState.Wait;
					}
				}
				break;

				// �̵�
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 2:
				{
					bhvMonster.fStateTimeBasic = bhvMonster.csvPatternData.Params[0];
					bhvMonster.fStateTime = bhvMonster.fStateTimeBasic;

					if (isChangeMoveState)
					{
						bhvMonster.eMoveState = Battle_BehaviourMonster.EMoveState.Move;
					}
				}
				break;

				// �ൿ
				// 0 : �ൿ ���� AniDataSeqenceIndex
				// 1 : �ൿ �ð� ���� ( 0.0 ~ 1.0 )
				// 2 : �ൿ ��� �� SkillID
				// 3 : �ൿ �Ϸ� �� PatternDataIndex
				case 3:
				{
					if (isChangeMoveState)
					{
						bhvMonster.eMoveState = Battle_BehaviourMonster.EMoveState.Wait;
					}
				}
				break;

				// Ž��
				// 0 : �ð�
				// 1 : Ž�� ���� �� PatternDataIndex
				// 2 : ���� ���� �� PatternDataIndex
				// 3 : Ž�� ���� �� PatternDataIndex
				case 4:
				{
					bhvMonster.fStateTimeBasic = bhvMonster.csvPatternData.Params[0];
					bhvMonster.fStateTime = bhvMonster.fStateTimeBasic;
				}
				break;
			}
		}

		public void ProcessAggressivePattern(Battle_BehaviourMonster bhvMonster)
		{
			bhvMonster.eMoveState = Battle_BehaviourMonster.EMoveState.Aggressive;
			bhvMonster.fAlertTime = bhvMonster.characterOwn.csStatMonster.fAlertTime;

			switch (bhvMonster.csvPatternData.Type)
			{
				// ���
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 1:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[2], false);
				}
				break;

				// �̵�
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 2:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[2], false);
				}
				break;

				// �ൿ
				// 0 : �ൿ ���� AniDataSeqenceIndex
				// 1 : �ൿ �ð� ���� ( 0.0 ~ 1.0 )
				// 2 : �ൿ ��� �� SkillID
				// 3 : �ൿ �Ϸ� �� PatternDataIndex
				case 3:
				break;

				// Ž��
				// 0 : �ð�
				// 1 : Ž�� ���� �� PatternDataIndex
				// 2 : ���� ���� �� PatternDataIndex
				// 3 : Ž�� ���� �� PatternDataIndex
				case 4:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[1], false);
				}
				break;
			}
		}

		public void ProcessAttackPattern(Battle_BehaviourMonster bhvMonster)
		{
			switch (bhvMonster.csvPatternData.Type)
			{
				// ���
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 1:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[3], true);
				}
				break;

				// �̵�
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 2:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[3], true);
				}
				break;

				// �ൿ
				// 0 : �ൿ ���� AniDataSeqenceIndex
				// 1 : �ൿ �ð� ���� ( 0.0 ~ 1.0 )
				// 2 : �ൿ ��� �� SkillID
				// 3 : �ൿ �Ϸ� �� PatternDataIndex
				case 3:
				break;

				// Ž��
				// 0 : �ð�
				// 1 : Ž�� ���� �� PatternDataIndex
				// 2 : ���� ���� �� PatternDataIndex
				// 3 : Ž�� ���� �� PatternDataIndex
				case 4:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[2], true);
				}
				break;
			}
		}

		public void ProcessComplatePattern(Battle_BehaviourMonster bhvMonster)
		{
			switch (bhvMonster.csvPatternData.Type)
			{
				// ������ ����
				// 0 : PatternDataIndex - Min
				// 1 : PatternDataIndex - Max 
				case 0:
				break;

				// ���
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 1:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[2], true);
				}
				break;

				// �̵�
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				// 3 : ���� ���� �� PatternDataIndex
				case 2:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[2], true);
				}
				break;

				// �ൿ
				// 0 : �ൿ ���� AniDataSeqenceIndex
				// 1 : �ൿ �ð� ���� ( 0.0 ~ 1.0 )
				// 2 : �ൿ ��� �� SkillID
				// 3 : �ൿ �Ϸ� �� PatternDataIndex
				case 3:
				{
					bhvMonster.eSubState = Battle_BehaviourMonster.ESubState.None;
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[3], true);
				}
				break;

				// Ž��
				// 0 : �ð�
				// 1 : Ž�� ���� �� PatternDataIndex
				// 2 : ���� ���� �� PatternDataIndex
				// 3 : Ž�� ���� �� PatternDataIndex
				case 4:
				{
					ProcessBeginPattern(bhvMonster, (int)bhvMonster.csvPatternData.Params[3], true);
				}
				break;
			}
		}
	}
}
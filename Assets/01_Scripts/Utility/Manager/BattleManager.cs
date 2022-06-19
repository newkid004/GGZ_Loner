using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class BattleManager : SingletonBase<BattleManager>
	{
		private CSVData.Battle.Stage.Theme csStageTheme;
		private int iHardness;

		private int iNowSpawnGroupIndex;
		private int iMonsterTotalCount;

		private CSVData.Battle.Stage.Level nowLevel;
		private CSVData.Battle.Stage.Field nowField;

		public void Init(CSVData.Battle.Stage.Theme theme, int iHardness)
		{
			this.csStageTheme = theme;
			this.iHardness = iHardness;

			iNowSpawnGroupIndex = 0;

			nowLevel = CSVData.Battle.Stage.Level.Manager.Get(theme.LevelID);
			nowField = CSVData.Battle.Stage.Field.Manager.Get(nowLevel.FieldIDs[this.iHardness]);

			ProcessSpawnMonster();
		}

		/// <summary> ���� Field, SpawnGroup �ܰ迡 �´� ���� ���� </summary>
		public void ProcessSpawnMonster()
		{
			iMonsterTotalCount = 0;

			if (iNowSpawnGroupIndex < nowField.SpawnGroupIDs.Length)
			{
				// �Ϲ� ��������
				var nowSpawnGroup = CSVData.Battle.Stage.SpawnGroup.Manager.Get(nowField.SpawnGroupIDs[iNowSpawnGroupIndex]);

				// �������� ȹ��
				PolygonCollider2D colSpawnRange = SceneMain_Battle.Single.mcsField.colFieldTotal;
				colSpawnRange.GenerateMeshInfo(out Mesh mesh, out List<Vector2> listPolygonTriangle);

				nowSpawnGroup.SpawnInfoIDs.ForEach(id =>
				{
					var nowSpawnInfo = CSVData.Battle.Stage.SpawnInfo.Manager.Get(id);

					switch (nowSpawnInfo.SpawnType)
					{
						case 0:
							{
								for (int i = 0; i < nowSpawnInfo.Params[0]; ++i)
								{
									Battle_BaseMonster mon = SceneMain_Battle.Single.mcsMonster.CreateMonster(nowSpawnInfo.MonsterID);
									mon.transform.position = GlobalUtility.Trigonometric.GetRandomPointInPolygon(listPolygonTriangle, mon.transform);

									if (GlobalUtility.Digit.Include(mon.iObjectType, GlobalDefine.ObjectData.ObjectType.ciBoss))
									{
										// ����
									}
									else
									{
										// �Ϲ�
										mon.InitToCSV(nowSpawnInfo.MonsterID);
									}
								}

								iMonsterTotalCount += nowSpawnInfo.Params[0];
							}
							break;
					}
				});
			}
			else
			{
				// ���� ��������

			}

			++iNowSpawnGroupIndex;
		}

		public void ProcessVictory()
		{
			// ���� �� ȣ�� �� ���� ����
			AsyncOperation asyncOperSceneLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("01_Main", UnityEngine.SceneManagement.LoadSceneMode.Single);
			asyncOperSceneLoad.completed += (asyncOper) =>
			{
				// �������� ȭ��
				SceneMain_Main.Single.mcsPage.OpenPage(Main_PageBase.EID.Stage);

				// ���� �˾� ���
				Main_PopupRewardAfterStageComplate pu = (Main_PopupRewardAfterStageComplate)SceneMain_Main.Single.mcsPage.pgCurrent.AddPopup(Main_PopupBase.EType.RewardAfterStageComplate);

				// Ŭ���� ���� ����
				pu.Init(nowLevel, iHardness);
			};
		}

		public void OnCharacterDead(Battle_BaseCharacter dead, Battle_BaseCharacter killer)
		{
			if (GlobalUtility.Digit.Include(dead.iObjectType, GlobalDefine.ObjectData.ObjectType.ciPlayer))
			{
				// �÷��̾�
			}
			else
			{
				// ����
				if (GlobalUtility.Digit.Include(dead.iObjectType, GlobalDefine.ObjectData.ObjectType.ciBoss))
				{
					// ����
				}
				else
				{
					// �Ϲ�
					--iMonsterTotalCount;
				}

				// ��� ���� óġ
				if (iNowSpawnGroupIndex == nowField.SpawnGroupIDs.Length)
				{
					// ������ �ʵ�

				}	
				else
				{
					// �Ϲ� �ʵ�
					if (iMonsterTotalCount == 0)
					{
						ProcessSpawnMonster();
					}
				}
			}
		}
	}
}
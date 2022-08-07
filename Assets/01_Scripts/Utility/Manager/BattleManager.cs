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

		/// <summary> 현재 Field, SpawnGroup 단계에 맞는 몬스터 생성 </summary>
		public void ProcessSpawnMonster()
		{
			iMonsterTotalCount = 0;

			if (iNowSpawnGroupIndex < nowField.SpawnGroupIDs.Length)
			{
				// 일반 스테이지
				var nowSpawnGroup = CSVData.Battle.Stage.SpawnGroup.Manager.Get(nowField.SpawnGroupIDs[iNowSpawnGroupIndex]);

				// 생성범위 획득
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
								Battle_BaseMonster mon = Battle_MonsterManager.Single.CreateMonster(nowSpawnInfo.MonsterID);
								mon.transform.position = GlobalUtility.Trigonometric.GetRandomPointInPolygon(listPolygonTriangle, mon.transform);

								if (GlobalUtility.Digit.Include(mon.iObjectType, GlobalDefine.ObjectData.ObjectType.ciBoss))
								{
									// 보스
								}
								else
								{
									// 일반
									mon.InitObjectDataToCSV(nowSpawnInfo.MonsterID);

									Battle_PatternManager.Single.SetMonsterBehaviour(mon);
								}
							}

							iMonsterTotalCount += nowSpawnInfo.Params[0];
							break;
						}
					}
				});
			}
			else
			{
				// 보스 스테이지

			}

			++iNowSpawnGroupIndex;
		}

		public void ProcessVictory()
		{
			// 메인 씬 호출 후 보상 지급
			AsyncOperation asyncOperSceneLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("01_Main", UnityEngine.SceneManagement.LoadSceneMode.Single);
			asyncOperSceneLoad.completed += (asyncOper) =>
			{
				// 스테이지 화면
				SceneMain_Main.Single.mcsPage.OpenPage(Main_PageBase.EID.Stage);

				// 보상 팝업 출력
				Main_PopupRewardAfterStageComplate pu = (Main_PopupRewardAfterStageComplate)SceneMain_Main.Single.mcsPage.pgCurrent.AddPopup(Main_PopupBase.EType.RewardAfterStageComplate);

				// 클리어 정보 전달
				pu.Init(nowLevel, iHardness);
			};
		}

		public void ProcessCharacterHit(ref Battle_SkillManager.stSkillProcessInfo stSkillInfo)
		{
			var charTarget = (Battle_BaseCharacter)stSkillInfo.objTarget;
			var charOwner = (Battle_BaseCharacter)stSkillInfo.objOwner;

			float fBeforeHealth = charTarget.csStatBasic.fHealthNow;
			float fAfterHealth = fBeforeHealth;

			float fDamage = Mathf.Abs(charTarget.csStatBasic.fDefendPower - charOwner.csStatBasic.fAttackPower);

			if (0 < fDamage)
			{
				fAfterHealth -= fDamage;

				charTarget.csStatBasic.fHealthNow = fAfterHealth;
				charTarget.TriggeredByTakeDamage(charOwner);

				// 체력 소진에 따른 라이프 감소
				if (charTarget.csStatBasic.fHealthNow <= 0)
				{
					charTarget.TriggeredByLifeDecrease(charOwner);

					if (0 < charTarget.csStatBasic.iLife)
					{
						// 라이프 감소에 따른 체력 회복
						charTarget.csStatBasic.fHealthNow = charTarget.csStatBasic.fHealthMax;
					}
					else
					{
						// 라이프가 모두 감소 (사망 처리는 트리거 내 기본 동작으로 구성)
						charTarget.TriggeredByLifeZero(charOwner);
					}
				}
			}
		}

		public void ProcessCharacterDead(Battle_BaseCharacter dead, Battle_BaseCharacter killer)
		{
			if (GlobalUtility.Digit.Include(dead.iObjectType, GlobalDefine.ObjectData.ObjectType.ciPlayer))
			{
				// 플레이어
			}
			else
			{
				// 몬스터
				if (GlobalUtility.Digit.Include(dead.iObjectType, GlobalDefine.ObjectData.ObjectType.ciBoss))
				{
					// 보스
				}
				else
				{
					// 일반
					--iMonsterTotalCount;
				}

				// 모든 몬스터 처치
				if (iNowSpawnGroupIndex == nowField.SpawnGroupIDs.Length)
				{
					// 마지막 필드

				}
				else
				{
					// 일반 필드
					if (iMonsterTotalCount == 0)
					{
						ProcessSpawnMonster();
					}
				}
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Main_PageStage : Main_PageBase
	{
		public override int iID => (int)EID.Stage;

		[SerializeField] private int iThemeID;					// 스테이지 ID
		[SerializeField] private int iStageHardness;			// 스테이지 난이도

		[SerializeField] private Text textStageBoss;			// 스테이지 보스 명칭
		[SerializeField] private Image imgStageBoss;			// 스테이지 보스 이미지

		[SerializeField] private Text textStageHardness;		// 스테이지 보스 명칭
		[SerializeField] private Image imgStageHardness;		// 스테이지 보스 이미지

		[SerializeField] private List<Image> imgDelegatePet;	// 스테이지 내 파견 펫 이미지
		[SerializeField] private Image imgDelegateComplete;		// 스테이지 파견 완료 이미지

		[SerializeField] private List<Color> listHardnessColor; // 난이도 별 색상

		private CSVData.Battle.Stage.Theme dataSelectedStageTheme;

		public void ChangeStageToDirection(int iDirection)
		{
			// 선택한 스테이지 변경
			iThemeID = iThemeID.ModStep(iDirection, CSVData.Battle.Stage.Theme.Manager.Count);

			// 마지막으로 선택한 난이도 획득
			iStageHardness = SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.GetSafe(iThemeID);

			RefreshSelectedStage();
		}

		private void SpreadPageToTheme(CSVData.Battle.Stage.Theme theme)
		{
			// 테스트 ( id -> field 변경 필요 )
			switch(theme.ID)
			{
				case 0: SpriteManager.Single.Apply(imgStageBoss, SpriteManager.Container.EType.Animation, (int)SpriteDefine.Animation.Hero_001_idle01); break;
				case 1: SpriteManager.Single.Apply(imgStageBoss, SpriteManager.Container.EType.Animation, (int)SpriteDefine.Animation.Monster_002_idle01); break;
				case 2: SpriteManager.Single.Apply(imgStageBoss, SpriteManager.Container.EType.Animation, (int)SpriteDefine.Animation.Monster_003_idle01); break;
			}

			switch (iStageHardness)
			{
				case 0: textStageHardness.text = "EASY"; break;
				case 1: textStageHardness.text = "NORMAL"; break;
				case 2: textStageHardness.text = "HARD"; break;
			}

			imgStageHardness.color = listHardnessColor[iStageHardness];

			textStageBoss.text = theme.Name;

			dataSelectedStageTheme = theme;
		}

		public void ChangeHardnessToDirection(Vector2 vec2Distance)
		{
			CSVData.Battle.Stage.Level csvLevel = CSVData.Battle.Stage.Level.Manager.Get(dataSelectedStageTheme.LevelID);

			int iCurrentFieldID = csvLevel.FieldIDs[iStageHardness];
			int iHardnessDirection = 0 < vec2Distance.y ? -1 : 1;
			int iHardnessSelect = iCurrentFieldID.ModStep(iHardnessDirection, csvLevel.FieldIDs.Length);

			SelectHardness(iHardnessSelect);
		}

		private void SelectHardness(int iHardness)
		{
			iStageHardness = iHardness;

			SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.SetSafe(iThemeID, iStageHardness);
			SaveData.Main.StageSelection.Main.Save(null);

			RefreshSelectedStage();
		}

		public void PressDroptablePopup()
		{

		}

		public void PressDelegatePopup()
		{

		}

		// 버튼 : 전투 시작
		public void ProcessBattle()
		{
			// 전투 씬 호출
			AsyncOperation asyncOperSceneLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("02_Battle", UnityEngine.SceneManagement.LoadSceneMode.Single);
			asyncOperSceneLoad.completed += (asyncOper) =>
			{
				BattleManager.Single.Init(dataSelectedStageTheme, iStageHardness);
			};
		}

		public override void OnForward()
		{
			base.OnForward();

			iThemeID = SaveData.Main.StageSelection.Main.iLastSelectedFieldID;
			iStageHardness = SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.GetSafe(iThemeID);

			RefreshSelectedStage();
		}

		private void RefreshSelectedStage()
		{
			// 선택했던 스테이지 정보 최신화
			CSVData.Battle.Stage.Theme stageTheme = CSVData.Battle.Stage.Theme.Manager.Get(iThemeID);

			if (stageTheme != null)
			{
				SpreadPageToTheme(stageTheme);
			}
		}

		private void RefreshDelegatePetList()
		{
			// 선택한 스테이지에 파견된 펫 정보 최신화
		}

		public override void OnDisablePage()
		{
			SaveData.Main.StageSelection.Main.iLastSelectedFieldID = iThemeID;
			SaveData.Main.StageSelection.Main.Save(null);

			base.OnDisablePage();
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Proto_00_N
{
	public class Main_PageStage : Main_PageBase
	{
		public override int iID => (int)EID.Stage;

		[SerializeField] private int iStageFieldID;				// 스테이지 ID
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
			iStageFieldID = iStageFieldID.ModStep(iDirection, CSVData.Battle.Stage.Field.Manager<int>.Count);

			// 마지막으로 선택한 난이도 획득
			iStageHardness = SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.GetSafe(iStageFieldID);

			RefreshSelectedStage();
		}

		private void SpreadPageToTheme(CSVData.Battle.Stage.Theme theme)
		{
			// 테스트 ( id -> field 변경 필요 )
			switch(theme.FieldID)
			{
				case 0: SpriteManager.Single.Apply(imgStageBoss, SpriteManager.Container.EType.CharIdle, "Hero_001_idle01"); break;
				case 1: SpriteManager.Single.Apply(imgStageBoss, SpriteManager.Container.EType.CharAnimation, "Monster_002_idle01"); break;
				case 2: SpriteManager.Single.Apply(imgStageBoss, SpriteManager.Container.EType.CharAnimation, "Monster_002_AttactReady03"); break;
			}

			SelectHardness(theme.Hardness);

			textStageBoss.text = theme.Name;

			dataSelectedStageTheme = theme;
		}

		public void ChangeHardnessToDirection(Vector2 vec2Distance)
		{
			int iHardnessDirection = 0 < vec2Distance.y ? -1 : 1;
			int iHardnessSelect = iStageHardness.ModStep(iHardnessDirection, CSVData.Battle.Stage.Hardness.Manager<int>.Count);

			SelectHardness(iHardnessSelect);
		}

		private void SelectHardness(int iHardness)
		{
			iStageHardness = iHardness;

			textStageHardness.text = CSVData.Battle.Stage.Hardness.Manager<int>.Get(iHardness).Name;
			imgStageHardness.color = listHardnessColor[iHardness];

			SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.SetSafe(iStageFieldID, iStageHardness);
			SaveData.Main.StageSelection.Main.Save();
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
			BattleManager.stInitData stInitData = new BattleManager.stInitData()
			{
				ID_Stage = dataSelectedStageTheme.ID
			};

			BattleManager.Single.StartBattle(ref stInitData);
		}

		public override void OnForward()
		{
			base.OnForward();

			iStageFieldID = SaveData.Main.StageSelection.Main.iLastSelectedFieldID;
			iStageHardness = SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.GetSafe(iStageFieldID);

			RefreshSelectedStage();
		}

		private void RefreshSelectedStage()
		{
			// 선택했던 스테이지 정보 최신화
			CSVData.Battle.Stage.Theme stageTheme =
				CSVData.Battle.Stage.Theme.Manager<int>.Search(
					(theme) => theme.FieldID == iStageFieldID && theme.Hardness == iStageHardness);

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
			SaveData.Main.StageSelection.Main.iLastSelectedFieldID = iStageFieldID;
			SaveData.Main.StageSelection.Main.Save();

			base.OnDisablePage();
		}
	}
}
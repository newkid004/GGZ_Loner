using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Main_PageStage : Main_PageBase
	{
		public override int iID => (int)EID.Stage;

		[SerializeField] private int iThemeID;					// �������� ID
		[SerializeField] private int iStageHardness;			// �������� ���̵�

		[SerializeField] private Text textStageBoss;			// �������� ���� ��Ī
		[SerializeField] private Image imgStageBoss;			// �������� ���� �̹���

		[SerializeField] private Text textStageHardness;		// �������� ���� ��Ī
		[SerializeField] private Image imgStageHardness;		// �������� ���� �̹���

		[SerializeField] private List<Image> imgDelegatePet;	// �������� �� �İ� �� �̹���
		[SerializeField] private Image imgDelegateComplete;		// �������� �İ� �Ϸ� �̹���

		[SerializeField] private List<Color> listHardnessColor; // ���̵� �� ����

		private CSVData.Battle.Stage.Theme dataSelectedStageTheme;

		public void ChangeStageToDirection(int iDirection)
		{
			// ������ �������� ����
			iThemeID = iThemeID.ModStep(iDirection, CSVData.Battle.Stage.Theme.Manager.Count);

			// ���������� ������ ���̵� ȹ��
			iStageHardness = SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.GetSafe(iThemeID);

			RefreshSelectedStage();
		}

		private void SpreadPageToTheme(CSVData.Battle.Stage.Theme theme)
		{
			// �׽�Ʈ ( id -> field ���� �ʿ� )
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

		// ��ư : ���� ����
		public void ProcessBattle()
		{
			// ���� �� ȣ��
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
			// �����ߴ� �������� ���� �ֽ�ȭ
			CSVData.Battle.Stage.Theme stageTheme = CSVData.Battle.Stage.Theme.Manager.Get(iThemeID);

			if (stageTheme != null)
			{
				SpreadPageToTheme(stageTheme);
			}
		}

		private void RefreshDelegatePetList()
		{
			// ������ ���������� �İߵ� �� ���� �ֽ�ȭ
		}

		public override void OnDisablePage()
		{
			SaveData.Main.StageSelection.Main.iLastSelectedFieldID = iThemeID;
			SaveData.Main.StageSelection.Main.Save(null);

			base.OnDisablePage();
		}
	}
}
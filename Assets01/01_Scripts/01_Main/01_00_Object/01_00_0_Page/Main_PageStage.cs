using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Proto_00_N
{
	public class Main_PageStage : Main_PageBase
	{
		public override int iID => (int)EID.Stage;

		[SerializeField] private int iStageFieldID;				// �������� ID
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
			iStageFieldID = iStageFieldID.ModStep(iDirection, CSVData.Battle.Stage.Field.Manager<int>.Count);

			// ���������� ������ ���̵� ȹ��
			iStageHardness = SaveData.Main.StageSelection.Main.dictSelectedFieldHardness.GetSafe(iStageFieldID);

			RefreshSelectedStage();
		}

		private void SpreadPageToTheme(CSVData.Battle.Stage.Theme theme)
		{
			// �׽�Ʈ ( id -> field ���� �ʿ� )
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

		// ��ư : ���� ����
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
			// �����ߴ� �������� ���� �ֽ�ȭ
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
			// ������ ���������� �İߵ� �� ���� �ֽ�ȭ
		}

		public override void OnDisablePage()
		{
			SaveData.Main.StageSelection.Main.iLastSelectedFieldID = iStageFieldID;
			SaveData.Main.StageSelection.Main.Save();

			base.OnDisablePage();
		}
	}
}
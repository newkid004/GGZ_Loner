using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public class Loading_PageSaveLoading : Loading_PageBase
	{
		public override void ProcessLoad()
		{
			base.ProcessLoad();

			LoadData();
			InitSingleton();

			ProcessLoadComplate();
		}

		// ���� CSV Loading �� ���� ���÷����� ���� �ϰ� ���� ����
		private void LoadData()
		{
			SaveData.Main.StageSelection.Main.Load();
		}

		private void InitSingleton()
		{
			SaveData.Main.StageSelection.Main.JustCall();
		}
	}
}
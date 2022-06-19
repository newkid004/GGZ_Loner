using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
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
			SaveData.Main.StageSelection.Main.Load(null);
		}

		private void InitSingleton()
		{
			SaveData.Main.StageSelection.Main.JustCall();
		}
	}
}
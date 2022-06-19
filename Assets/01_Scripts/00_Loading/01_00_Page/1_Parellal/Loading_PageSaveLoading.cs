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

		// 차후 CSV Loading 과 같이 리플렉션을 통한 일괄 실행 적용
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
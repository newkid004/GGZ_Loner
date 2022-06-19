using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Main_PopupRewardAfterStageComplate : Main_PopupBase
	{
		public override int iType => (int)EType.RewardAfterStageComplate;

		public void Init(CSVData.Battle.Stage.Level csvLevel, int iHardness)
		{

		}
	}
}

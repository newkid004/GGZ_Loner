using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

#pragma warning disable 0414	// 사용되지 않는 변수 경고 무시

namespace Proto_00_N
{
	public static class SaveData
	{
		public static string strPathSave => $"{Application.persistentDataPath}/save";
		public const string strFileExtension = "sav";

		public static class Main
		{
			[Serializable]
			public class StageSelection : SaveObjectBase<StageSelection>
			{
				public int iLastSelectedFieldID = 0;
				public Dictionary<int, int> dictSelectedFieldHardness = new Dictionary<int, int>();
			}
		}

		// public static class Battle
		// {
		// 	[Serializable]
		// 	public class PlayerStatus
		// 	{
		// 
		// 	}
		// }
	}
}

#pragma warning restore 0414
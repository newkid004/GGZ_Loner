using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

#pragma warning disable 0414	// ������ �ʴ� ���� ��� ����

namespace GGZ
{
	public static class SaveData
	{
		public static class Main
		{
			[Serializable]
			public class StageSelection : SaveSingleObjectBase<StageSelection>
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
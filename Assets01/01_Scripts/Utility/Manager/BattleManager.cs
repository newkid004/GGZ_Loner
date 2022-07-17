using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public class BattleManager : SingletonBase<BattleManager>
	{
		public struct stInitData
		{
			public int ID_Stage;
		}

		private AsyncOperation asyncOperSceneLoad;

		public void StartBattle(ref stInitData data)
		{
			// ÀüÅõ ¾À È£Ãâ
			asyncOperSceneLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("02_Battle", UnityEngine.SceneManagement.LoadSceneMode.Single);
			asyncOperSceneLoad.completed += (asyncOper) =>
			{

			};
		}
	}
}
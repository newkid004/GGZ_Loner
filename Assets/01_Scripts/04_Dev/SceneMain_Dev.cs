using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class SceneMain_Dev : MonoBehaviour
	{
		[SerializeField] GameObject goRootEditor;
		[SerializeField] GameObject goRootPageMain;
		[SerializeField] GameObject goRootPageUI;

		public void Awake()
		{
			if (false == AnimationManager.isInit)
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene("00_Loading", UnityEngine.SceneManagement.LoadSceneMode.Single);

				CustomRoutine.CallLate(0, () =>
				{
					SceneMain_Loading.Single.isLoadDevScene = true;
				});

				return;
			}
		}
	}
}

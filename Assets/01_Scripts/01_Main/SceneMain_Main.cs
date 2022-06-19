using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class SceneMain_Main : MonoBehaviour
	{
		public static SceneMain_Main Single { get; private set; }

		[Header("----- Manager -----")]
		[SerializeField] private Main_PageManager	_mcsPage;
		[SerializeField] private Main_PopupManager	_mcsPopup;

		public Main_PageManager		mcsPage		=> _mcsPage;
		public Main_PopupManager	mcsPopup	=> _mcsPopup;

		private void Awake()
		{
			Single = this;

			mcsPage.Init();
			mcsPopup.Init();
		}

		public void CloseTopPage() => _mcsPage.CloseTopPage();
	}
}

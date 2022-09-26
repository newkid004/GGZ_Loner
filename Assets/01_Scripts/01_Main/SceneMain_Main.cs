using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class SceneMain_Main : MonoBehaviour
	{
		public static SceneMain_Main Single { get; private set; }

		[Header("----- Manager -----")]
		[SerializeField] private Main_PageManager				_mcsPage;
		[SerializeField] private Main_PopupManager				_mcsPopup;
		[SerializeField] private Main_FieldObjectManager		_mcsFieldObject;

		public Main_PageManager				mcsPage			=>	_mcsPage;
		public Main_PopupManager			mcsPopup		=>	_mcsPopup;
		public Main_FieldObjectManager		mcsFieldObject	=>	_mcsFieldObject;

		public Main_FieldCharacterPlayer fcPlayer;
		public UI_DirectionController uiDirectionContoller;
		public UI_DirectionDragPanel uiDirectionDragPanel;

		public Camera camMain;
		public Camera camUI;

		private void Awake()
		{
			Single = this;

			mcsPage.Init();
			mcsPopup.Init();
			mcsFieldObject.Init();

			uiDirectionContoller.idcConnect = fcPlayer;
			uiDirectionDragPanel.idcConnect = fcPlayer;
		}

		public void CloseTopPage() => _mcsPage.CloseTopPage();
	}
}

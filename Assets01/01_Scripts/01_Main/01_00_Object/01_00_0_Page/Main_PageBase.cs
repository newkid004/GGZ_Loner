using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public abstract class Main_PageBase : MonoBehaviour
	{
		[SerializeField]
		public enum EID
		{
			None				= 0,
			Preview				= 1,		// 로딩 완료 후 보여지는 Press To Start
			Main				= 2,		// 메인 화면
			Dictionary			= 100,		// 도감
			Shop				= 200,		// 상점
			CastleOfInfinity	= 300,		// 무한의 탑
			MyHome				= 400,		// 마이홈
			Stage				= 500,		// 스테이지
			Max,
		}

		public abstract int iID { get; } // => (int)EID.None;

		protected Dictionary<int, List<Main_PopupBase>> dictActivePopupList = new Dictionary<int, List<Main_PopupBase>>();

		public GameObject goCanvasObject;

		public void AddPopup(Main_PopupBase.EType eType) 
		{
			Main_PopupBase popup = SceneMain_Main.Single.mcsPopup.Pop(eType, this.transform);

			List<Main_PopupBase> listActivePopup = dictActivePopupList.GetSafe((int)eType);
			listActivePopup.Add(popup);

			OnAddPopup(popup);
			popup.OnEnablePopup(this);
		}

		public void DelPopup(Main_PopupBase popup)
		{
			List<Main_PopupBase> listActivePopup = dictActivePopupList.GetSafe(popup.iType);

			if (listActivePopup.Remove(popup))
			{
				OnDelPopup(popup);
				popup.OnDisablePopup(this);

				SceneMain_Main.Single.mcsPopup.Push(popup);
			}
#if _debug
			else
			{
				Debug.LogAssertion($"Error (Invalid Remove Popup) : {popup}");
			}
#endif
		}

		// Awake - Delete
		public virtual void OnEnablePage() { }						// 페이지를 활성화
		public virtual void OnDisablePage() 						// 페이지를 비활성화
		{
			dictActivePopupList.ForEach((i, list) =>
			{
				list.ForEach(popup => DelPopup(popup));
			});

			dictActivePopupList.Clear();
		}

		// Enable - Disable (Once)
		public virtual void OnOpenPage() { }						// 페이지를 염
		public virtual void OnClosePage()							// 페이지를 닫음
		{
			gameObject.SetActive(false);
			goCanvasObject?.SetActive(false);
		}

		public virtual void OnForward()								// 페이지가 최상단으로 위치
		{
			goCanvasObject?.SetActive(true);
			gameObject.SetActive(true);
		}

		public virtual void OnCovered(Main_PageBase pgOther)		// 페이지가 다른 페이지에 의해 덮임
		{
			gameObject.SetActive(false);
			goCanvasObject?.SetActive(false);
		}

		// Popup
		public virtual void OnAddPopup(Main_PopupBase popup) { }	// 팝업을 염
		public virtual void OnDelPopup(Main_PopupBase popup) { }	// 팝업을 닫음
	}
}
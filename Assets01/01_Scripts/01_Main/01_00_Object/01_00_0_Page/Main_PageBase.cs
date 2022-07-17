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
			Preview				= 1,		// �ε� �Ϸ� �� �������� Press To Start
			Main				= 2,		// ���� ȭ��
			Dictionary			= 100,		// ����
			Shop				= 200,		// ����
			CastleOfInfinity	= 300,		// ������ ž
			MyHome				= 400,		// ����Ȩ
			Stage				= 500,		// ��������
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
		public virtual void OnEnablePage() { }						// �������� Ȱ��ȭ
		public virtual void OnDisablePage() 						// �������� ��Ȱ��ȭ
		{
			dictActivePopupList.ForEach((i, list) =>
			{
				list.ForEach(popup => DelPopup(popup));
			});

			dictActivePopupList.Clear();
		}

		// Enable - Disable (Once)
		public virtual void OnOpenPage() { }						// �������� ��
		public virtual void OnClosePage()							// �������� ����
		{
			gameObject.SetActive(false);
			goCanvasObject?.SetActive(false);
		}

		public virtual void OnForward()								// �������� �ֻ������ ��ġ
		{
			goCanvasObject?.SetActive(true);
			gameObject.SetActive(true);
		}

		public virtual void OnCovered(Main_PageBase pgOther)		// �������� �ٸ� �������� ���� ����
		{
			gameObject.SetActive(false);
			goCanvasObject?.SetActive(false);
		}

		// Popup
		public virtual void OnAddPopup(Main_PopupBase popup) { }	// �˾��� ��
		public virtual void OnDelPopup(Main_PopupBase popup) { }	// �˾��� ����
	}
}
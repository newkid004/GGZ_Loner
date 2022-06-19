using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class Main_PopupManager
	{
		[Header("Resource")]
		[SerializeField] private int iPooledCount;
		[SerializeField] private List<Main_PopupBase> listInputPopup;

		private Dictionary<int, ObjectPool<Main_PopupBase>> dictPopupPool = new Dictionary<int, ObjectPool<Main_PopupBase>>();

		public void Init()
		{
			InitInput();
		}

		private void InitInput()
		{
			int iCount = listInputPopup.Count;

			for (int i = 0; i < iCount; ++i)
			{
				Main_PopupBase puAddition = listInputPopup[i];

				if (puAddition == null || puAddition.iType == (int)Main_PopupBase.EType.None)
				{
#if _debug
					Debug.LogAssertion($"Init Error (Invalid Popup iType) : listInputPopup[{iCount}]");
#endif
					continue;
				}

				int iID = puAddition.iType;
				ObjectPool<Main_PopupBase> opAddition = new ObjectPool<Main_PopupBase>(puAddition);
				opAddition.Init();

				dictPopupPool.Add(puAddition.iType, opAddition);
			}
		}

		public Main_PopupBase Pop(Main_PopupBase.EType eType, Transform trParent)
		{
			Main_PopupBase puResult = null;

			ObjectPool<Main_PopupBase> opPopup = dictPopupPool.GetDef((int)eType);

			if (opPopup != null)
			{
				puResult = opPopup.Pop(trParent);
			}
#if _debug
			else
			{
				Debug.LogAssertion($"Pop Error (Invalid Pop eType) : {eType}");
			}
#endif
			return puResult;
		}

		public void Push(Main_PopupBase popup)
		{
			if (popup != null)
			{
				ObjectPool<Main_PopupBase> opPopup = dictPopupPool.GetDef(popup.iType);

				if (opPopup != null)
				{
					opPopup.Push(popup);
				}
#if _debug
				else
				{
					Debug.LogAssertion($"Push Error (Invalid Push iType) : {(Main_PopupBase.EType)popup.iType}");
				}
#endif
			}
		}
	}
}
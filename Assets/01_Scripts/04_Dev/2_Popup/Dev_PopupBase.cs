using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public abstract class Dev_PopupBase : MonoBehaviour
	{
		protected static Dictionary<System.Type, Dev_PopupBase> dictPopup = new Dictionary<System.Type, Dev_PopupBase>();

		protected abstract System.Type OwnType { get; }

		private void Awake()
		{
			dictPopup.SetSafe(OwnType, this);
		}

		public static T Get<T>() where T : Dev_PopupBase
		{
			return (T)dictPopup.GetDef(typeof(T));
		}

		public static void Open<T>() where T : Dev_PopupBase
		{
			Get<T>().Open();
		}

		public virtual void Open()
		{
			if (gameObject.activeSelf)
				return;

			gameObject.SetActive(true);

			OpenWithBackground();
		}

		protected virtual void OpenWithBackground()
		{
			Dev_PopupBackground popback = Get<Dev_PopupBackground>();
			popback.hsLinkedPopup.Add(this);
			popback.Open();
		}

		public virtual void Close()
		{
			if (false == gameObject.activeSelf)
				return;

			gameObject.SetActive(false);

			CloseWithBackground();
		}

		protected virtual void CloseWithBackground()
		{
			Dev_PopupBackground popback = Get<Dev_PopupBackground>();
			popback.hsLinkedPopup.Remove(this);

			if (0 == popback.hsLinkedPopup.Count)
			{
				if (popback.gameObject.activeSelf)
				{
					popback.Close();
				}
			}
		}

		public static void CloseAll() => dictPopup.ForEach((t, p) => p.Close());
	}
}

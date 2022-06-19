using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public abstract class Dev_PageBase : MonoBehaviour
	{
		protected static Dictionary<System.Type, Dev_PageBase> dictPage = new Dictionary<System.Type, Dev_PageBase>();

		[SerializeField] private List<GameObject> listActiveLink = new List<GameObject>();

		protected virtual void Start()
		{
			dictPage.Add(this.GetType(), this);
		}

		public static T Get<T>() where T : Dev_PageBase
		{
			return (T)dictPage.GetDef(typeof(T));
		}

		public static void Open<T>() where T : Dev_PageBase
		{
			Get<T>().Open();
		}

		public virtual void Open()
		{
			dictPage.ForEach((type, page) =>
			{
				if (page != this)
				{
					page.Close();
				}
			});

			gameObject.SetActive(true);
			listActiveLink.ForEach(go => go.SetActive(true));
		}

		public virtual void Close()
		{
			gameObject.SetActive(false);
			listActiveLink.ForEach(go => go.SetActive(false));
		}
	}
}


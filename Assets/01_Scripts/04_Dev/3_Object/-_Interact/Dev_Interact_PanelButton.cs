using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Dev_Interact_PanelButton : MonoBehaviour
	{
		private static Dictionary<GameObject, List<Dev_Interact_PanelButton>> dictParent =
			new Dictionary<GameObject, List<Dev_Interact_PanelButton>>();

		private static Dev_Interact_PanelButton pbActive = null;

		public bool isInteract = false;

		[SerializeField]
		private GameObject objParent;

		[SerializeField]
		private List<GameObject> listInverseLinkedObject = new List<GameObject>();

		private void Awake()
		{
			dictParent.GetSafe(objParent).Add(this);
		}

		private void Start()
		{
			isInteract = true;
		}

		public void OnEnable()
		{
			ProcessEnableElse(false);
		}

		public void OnDisable()
		{
			ProcessEnableElse(true);
		}

		private void ProcessEnableElse(bool isEnable)
		{
			if (isInteract)
			{
				if (pbActive == null)
				{
					pbActive = this;
				}

				var listBelong = dictParent.GetDef(objParent);

				foreach (var child in listBelong)
				{
					if (child != pbActive && child != this && child.gameObject.activeInHierarchy == false)
					{
						child.gameObject.SetActive(true);
					}
				}

				foreach (var objLink in listInverseLinkedObject)
				{
					objLink.SetActive(isEnable);
				}

				if (pbActive == this)
				{
					pbActive = null;
				}
			}
		}
	}
}
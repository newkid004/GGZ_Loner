using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Dev_PopupAlert : Dev_PopupBase
	{
		protected override System.Type OwnType => typeof(Dev_PopupAlert);

		public struct Info
		{
			public string strTitle;
			public string strDescription;

			public System.Action actOK;
			public System.Action actNo;
		}

		public Text txtTitle;
		public Text txtDescription;

		private System.Action actOK;

		public Dev_PopupAlert Init(Info info)
		{
			txtTitle.text = info.strTitle;
			txtDescription.text = info.strDescription;

			actOK = info.actOK;

			return this;
		}

		public void OnOK() => actOK?.Invoke();
	}
}

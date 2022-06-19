using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Dev_PopupYorNSubmit : Dev_PopupBase
	{
		protected override System.Type OwnType => typeof(Dev_PopupYorNSubmit);

		public struct Info
		{
			public string strTitle;
			public string strDescription;

			public System.Action actYes;
			public System.Action actNo;
		}

		public Text txtTitle;
		public Text txtDescription;

		private System.Action actYes;
		private System.Action actNo;

		private bool isSelect;

		public Dev_PopupYorNSubmit Init(Info info)
		{
			txtTitle.text = info.strTitle;
			txtDescription.text = info.strDescription;

			actYes = info.actYes;
			actNo = info.actNo;

			isSelect = false;

			return this;
		}

		public void OnOK()
		{
			actYes();

			isSelect = true;
		}
		
		public void OnNo() => actNo?.Invoke();

		public override void Close()
		{
			if (false == isSelect)
			{
				OnNo();
			}

			base.Close();
		}
	}
}

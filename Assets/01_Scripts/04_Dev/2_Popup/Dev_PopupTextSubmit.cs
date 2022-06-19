using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GGZ
{
	public class Dev_PopupTextSubmit : Dev_PopupBase
	{
		protected override System.Type OwnType => typeof(Dev_PopupTextSubmit);

		public struct Info
		{
			public string strTitle;
			public string strDescription;
			public string strDefaultText;

			public System.Action<string> actSubmit;
			public System.Action actCancel;
		}

		public Text txtTitle;
		public Text txtDescription;
		public InputField ifText;

		private System.Action<string> actSubmit;
		private System.Action actCancel;

		private bool isSubmit;

		const string strDefaultText = "내용을 입력하세요...";

		public Dev_PopupTextSubmit Init(Info info)
		{
			txtTitle.text = info.strTitle;
			txtDescription.text = info.strDescription;

			actSubmit = info.actSubmit;
			actCancel = info.actCancel;

			((Text)ifText.placeholder).text = string.IsNullOrEmpty(info.strDefaultText) ? 
				strDefaultText : 
				info.strDefaultText;
			ifText.text = string.Empty;

			isSubmit = false;

			return this;
		}

		public override void Open()
		{
			base.Open();

			EventSystem.current.SetSelectedGameObject(ifText.gameObject);
		}

		public void OnSubmit()
		{
			actSubmit(ifText.text);
			isSubmit = true;
		}

		public void OnCancel()
		{
			if (false == isSubmit)
			{
				actCancel?.Invoke();
			}
		}

		public override void Close()
		{
			OnCancel();
			base.Close();
		}

		public void CheckEnter(string strCurrent)
		{
			if(strCurrent.EndsWith("\n"))
			{
				OnSubmit();
				Close();
			}
		}
	}
}
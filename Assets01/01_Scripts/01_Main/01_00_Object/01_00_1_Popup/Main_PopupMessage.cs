using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public class Main_PopupMessage : Main_PopupBase
	{
		public override int iType => (int)EType.Message;

		[SerializeField]
		private UnityEngine.UI.Text _text;
		public UnityEngine.UI.Text text 
		{
			get => _text;
			protected set => _text = value;
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			_text = GetComponent<UnityEngine.UI.Text>();
		}
	}
}

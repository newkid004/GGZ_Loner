using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Dev_PopupBackground : Dev_PopupBase
	{
		protected override System.Type OwnType => typeof(Dev_PopupBackground);

		public HashSet<Dev_PopupBase> hsLinkedPopup = new HashSet<Dev_PopupBase>();

		public override void Open()
		{
			gameObject.SetActive(true);
		}

		public override void Close()
		{
			hsLinkedPopup.ForEachSafe(item => item.Close());
			base.Close();
		}

		public void OnClickBackground()
		{
			Close();
		}
	}
}

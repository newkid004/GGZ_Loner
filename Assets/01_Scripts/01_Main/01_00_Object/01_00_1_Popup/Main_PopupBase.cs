using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public abstract class Main_PopupBase : ProtoPooledObject
	{
		public enum EType
		{
			None,

			Message,	// 일반 메세지. 터치 시 사라짐

			// ----- Battle ----- //
			RewardAfterStageComplate,	// 보상 화면

			Max,
		}

		public RectTransform RectTransform => (RectTransform)transform;
		public abstract int iType { get; }

		[Header("----- Base -----")]
		public Main_PageBase pgActive;  // 속해있는 Page

		public virtual void OnEnablePopup(Main_PageBase pg) => pgActive = pg;
		public virtual void OnDisablePopup(Main_PageBase pg) => pgActive = null;
	}
}
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

			Message,	// �Ϲ� �޼���. ��ġ �� �����

			// ----- Battle ----- //
			RewardAfterStageComplate,	// ���� ȭ��

			Max,
		}

		public RectTransform RectTransform => (RectTransform)transform;
		public abstract int iType { get; }

		[Header("----- Base -----")]
		public Main_PageBase pgActive;  // �����ִ� Page

		public virtual void OnEnablePopup(Main_PageBase pg) => pgActive = pg;
		public virtual void OnDisablePopup(Main_PageBase pg) => pgActive = null;
	}
}
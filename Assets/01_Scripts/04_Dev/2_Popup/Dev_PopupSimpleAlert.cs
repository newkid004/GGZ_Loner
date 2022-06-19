using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Dev_PopupSimpleAlert : Dev_PopupBase
	{
		protected override System.Type OwnType => typeof(Dev_PopupSimpleAlert);

		public struct Info
		{
			public string strDescription;

			public float fViewTime;
			public float fDisposeTime;
		}

		public Text txtDescription;
		public CanvasGroup cg;

		private int iRoutineIndex = -1;
		public Dev_PopupSimpleAlert Init(Info info)
		{
			txtDescription.text = info.strDescription;

			cg.alpha = 1;

			if (0 <= iRoutineIndex)
			{
				CustomRoutine.Stop(iRoutineIndex);
			}

			iRoutineIndex = CustomRoutine.CallLate(info.fViewTime, () =>
			{
				iRoutineIndex = CustomRoutine.CallInTime(info.fDisposeTime, (fLerp) =>
				{
					cg.alpha = 1f - fLerp;
				},
				() =>
				{
					Close();
				});
			});

			return this;
		}

		// Do Nothing
		protected override void OpenWithBackground() { }
		protected override void CloseWithBackground() { }
	}
}

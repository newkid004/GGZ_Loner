using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Proto_00_N
{
	public class Loading_PageLogo : Loading_PageBase
	{
		[Header("Image Set")]
		public Image imgBackGround;
		public Image imgFrontLogo;

		[Header("Option")]
		[Range(0.0f, 10.0f)] public float fWaitToStartSecond;
		[Range(0.0f, 10.0f)] public float fFadeInSecond;
		[Range(0.0f, 10.0f)] public float fWaitToShowSecond;
		[Range(0.0f, 10.0f)] public float fFadeOutSecond;
		[Range(0.0f, 10.0f)] public float fWaitToEndSecond;

		private int iCurrentLogoRountinIndex;

		public override void ProcessLoad()
		{
			base.ProcessLoad();

			ProcessLogo();
		}

		private void ProcessLogo()
		{
			Color clrLogoSourceeColor = imgFrontLogo.color;
			imgFrontLogo.color = new Color(imgFrontLogo.color.r, imgFrontLogo.color.g, imgFrontLogo.color.b, 0f);

			iCurrentLogoRountinIndex = CustomRoutine.CallLate(fWaitToStartSecond, () =>
			{
				imgFrontLogo.DOColor(clrLogoSourceeColor, fFadeInSecond);
				iCurrentLogoRountinIndex = CustomRoutine.CallLate(fFadeInSecond + fWaitToShowSecond, () =>
				{
					imgFrontLogo.DOFade(0, fFadeOutSecond);
					iCurrentLogoRountinIndex = CustomRoutine.CallLate(fFadeOutSecond + fWaitToEndSecond, ProcessLoadComplate);
				});
			});
		}

		protected override void OnComplate()
		{
			imgFrontLogo.gameObject.SetActive(false);
			base.OnComplate();
		}

		public void SkipLogoProcess()
		{
			CustomRoutine.Stop(iCurrentLogoRountinIndex);
			ProcessLoadComplate();
		}
	}
}
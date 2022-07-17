using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Proto_00_N
{
	using DG.Tweening;

	public class Main_PagePreview : Main_PageBase
	{
		public override int iID => (int)EID.Preview;

		[Header("UI")]
		[SerializeField] private Text textPress;

		private bool isPress = false;

		public override void OnForward()
		{
			base.OnForward();
			isPress = false;
			SceneMain_Main.Single.mcsPage.FadeIn(3.0f);
		}

		private void FixedUpdate()
		{
			Color colorText = textPress.color;

			colorText.a = (1f + Mathf.Sin(Time.time * Mathf.PI /
				(isPress ? 0.1f : 1f))) / 2f;

			textPress.color = colorText;
		}

		public void PressBackground()
		{
			if (isPress == false)
			{
				isPress = true;
				SceneMain_Main.Single.mcsPage.FadeOut(2.0f, () => { SceneMain_Main.Single.mcsPage.OpenPage(EID.Main); });
			}
		}
	}
}

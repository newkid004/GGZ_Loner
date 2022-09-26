using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GGZ
{
	[System.Serializable]
	public class Main_PageManager
	{
		public static Main_PageManager Single => SceneMain_Main.Single.mcsPage;

		// Prev Input Resource
		[Header("Resource")]
		[SerializeField] private List<Main_PageBase> listInputPage;

		// Managed Resource
		private Dictionary<int, Main_PageBase> dictPage = new Dictionary<int, Main_PageBase>();

		// Managed Object
		private Dictionary<Main_PageBase, int> dictPageEnableCount = new Dictionary<Main_PageBase, int>();
		private Stack<Main_PageBase> stkPage = new Stack<Main_PageBase>();

		public Main_PageBase pgCurrent { get; private set; }

		// Utility
		[SerializeField] private UnityEngine.UI.RawImage imgFade;

		public void Init()
		{
			imgFade.gameObject.SetActive(false);

			InitInput();

			// OpenPage(Main_PageBase.EID.Preview);
		}

		private void InitInput()
		{
			int iCount = listInputPage.Count;

			for (int i = 0; i < iCount; ++i)
			{
				Main_PageBase pgAddition = listInputPage[i];

				if (pgAddition == null || pgAddition.iID == (int)Main_PageBase.EID.None)
				{
#if _debug
					Debug.LogAssertion($"Init Error (Invalid Page iID) : listInputPage[{iCount}]");
#endif
					continue;
				}

				pgAddition.gameObject.SetActive(false);
				pgAddition.goCanvasObject?.SetActive(false);

				dictPage.Add(pgAddition.iID, pgAddition);
			}
		}

		public Main_PageBase GetPage(Main_PageBase.EID eID) => dictPage.GetDef((int)eID);

		public Main_PageBase OpenPage(Main_PageBase.EID ePageID)
		{
			Main_PageBase pg = GetPage(ePageID);

			if (pg == null)
			{
#if _debug
				Debug.LogAssertion($"Error (OpenPage - Page is NULL)");
#endif
				return null;
			}

			stkPage.Push(pg);

			dictPageEnableCount.ActSafe(pg, (isInsert, i) =>
			{
				++dictPageEnableCount[pg];

				if (isInsert)
				{
					pg.OnEnablePage();
				}
			});

			pgCurrent?.OnCovered(pg);

			pgCurrent = pg;
			pgCurrent.OnOpenPage();
			pgCurrent.OnForward();

			return pg;
		}

		public void CloseTopPage()
		{
			Main_PageBase pgTop = stkPage.Pop();

			if (pgTop == null)
			{
#if _debug
				Debug.LogAssertion($"Error (CloseTopPage - Not Stacked Page)");
#endif
				return;
			}

			pgTop.OnClosePage();

			dictPageEnableCount.ActSafe(pgTop, (isInsert, i) =>
			{
				int iCount = --dictPageEnableCount[pgTop];

				if (iCount == 0)
				{
					dictPageEnableCount.Remove(pgTop);
					pgTop.OnDisablePage();
				}
			});

			pgCurrent = stkPage.Peek();
			pgCurrent.OnForward();
		}

		public void FadeIn(float fFadeTime, System.Action actOnEnd = null)
		{
			imgFade.gameObject.SetActive(true);
			imgFade.raycastTarget = false;

			imgFade.color = Color.black;
			imgFade.DOColor(Color.clear, fFadeTime);
			CustomRoutine.CallLate(fFadeTime, () =>
			{
				imgFade.gameObject.SetActive(false);
				actOnEnd?.Invoke();
			});
		}

		public void FadeOut(float fFadeTime, System.Action actOnEnd = null)
		{
			imgFade.gameObject.SetActive(true);
			imgFade.raycastTarget = true;

			imgFade.color = Color.clear;
			imgFade.DOColor(Color.black, fFadeTime);
			CustomRoutine.CallLate(fFadeTime, () =>
			{
				imgFade.gameObject.SetActive(false);
				actOnEnd?.Invoke();
			});
		}
	}
}
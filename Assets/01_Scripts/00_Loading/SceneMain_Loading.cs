using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Threading.Tasks;

namespace GGZ
{
	public class SceneMain_Loading : MonoBehaviour
	{
		public static SceneMain_Loading Single { get; private set; }

		[Header("Managed")]
		[SerializeField] private List<Loading_PageBase>	listSerialPage;		// 직렬 로딩
		[SerializeField] private List<Loading_PageBase>	listParallelPage;	// 병렬 로딩
		[SerializeField] private List<Loading_PageBase> listCollectPage;	// 일괄 로딩

		private bool[] isComplatePage = new bool[(int)Loading_PageBase.EAsyncType.Max];

		private int iPageCountParallel;
		private int iPageCountCollect;

		private object objSwapLockParallel = new object();

#if _debug
		public const string CstrSceneMain = "01_Main";
		public const string CstrSceneDev = "04_Dev";

		[Header("For Dev")]
		public bool isLoadDevScene = false;

		// Dinostics
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
#endif

		// Start is called before the first frame update
		private void Awake() => Single = this;
		private void OnDestroy() => Single = null;

		void Start()
		{
			PreLoading();

			iPageCountParallel = listParallelPage.Count;
			iPageCountCollect = listCollectPage.Count;

			// 로딩페이지 초기화
			int i;

			i = 0;
			listSerialPage.ForEach(p => { p.Init(i++, Loading_PageBase.EAsyncType.Serial); p.gameObject.SetActive(false); });
			
			i = 0;
			listCollectPage.ForEach(p => { p.Init(i++, Loading_PageBase.EAsyncType.Collect); p.gameObject.SetActive(false); });

			i = 0;
			listParallelPage.ForEach(p => { p.Init(i++, Loading_PageBase.EAsyncType.Parallel); p.gameObject.SetActive(false); });

			// 로딩페이지 실행
#if _debug
			sw.Start();
#endif
			if (0 < listSerialPage.Count)
			{
#if _debug
				Debug.Log($"Load Start : Serial, Time : {sw.ElapsedMilliseconds}");
#endif
				listSerialPage[0].ProcessLoad();
			}
			else
			{
				isComplatePage[(int)Loading_PageBase.EAsyncType.Serial] = true;
			}

			if (0 < listParallelPage.Count)
			{
#if _debug
				Debug.Log($"Load Start : Parallel, Time : {sw.ElapsedMilliseconds}");
#endif
				listParallelPage.ForEach(p => p.ProcessLoad());
			}
			else
			{
				isComplatePage[(int)Loading_PageBase.EAsyncType.Parallel] = true;
			}

			if (0 < listCollectPage.Count)
			{
#if _debug
				Debug.Log($"Load Start : Collect, Time : {sw.ElapsedMilliseconds}");
#endif
				listCollectPage.ForEach(p => p.ProcessLoad());
			}
			else
			{
				isComplatePage[(int)Loading_PageBase.EAsyncType.Collect] = true;
			}
		}

		private void PreLoading()
		{
			// 성능에 영향이 크지 않은, 간단한 객체 초기화
			CustomRoutine.Init();
		}

		private void PostLoading()
		{
			// 기본 로딩 완료 이후, 객체 초기화
			AnimationManager.Single.JustCall();
			BattleManager.Single.JustCall();
			SpriteManager.Single.JustCall();
			FieldGroundManager.Single.JustCall();

			ItemManager.Single.JustCall();

			MainManager.Single.JustCall();
		}

		public void OnComplatedPage(Loading_PageBase lPage, Loading_PageBase.EAsyncType eAsyncType)
		{
			switch (eAsyncType)
			{
				case Loading_PageBase.EAsyncType.Serial:
					{
						int iNextIndex = lPage.iPageIndex + 1;
						if (iNextIndex < listSerialPage.Count)
						{
							listSerialPage[iNextIndex].ProcessLoad();
						}
						else
						{
							isComplatePage[(int)Loading_PageBase.EAsyncType.Serial] = true;
#if _debug
							Debug.Log($"Load Complate : Serial, Time : {sw.ElapsedMilliseconds}");
#endif
						}
					}
					break;

				case Loading_PageBase.EAsyncType.Collect:
					{
						if (--iPageCountCollect == 0)
						{
							isComplatePage[(int)Loading_PageBase.EAsyncType.Collect] = true;
#if _debug
							Debug.Log($"Load Complate : Collect, Time : {sw.ElapsedMilliseconds}");
#endif
						}
					}
					break;

				case Loading_PageBase.EAsyncType.Parallel:
					{
						lock (objSwapLockParallel)
						{
							if (--iPageCountParallel == 0)
							{
							isComplatePage[(int)Loading_PageBase.EAsyncType.Parallel] = true;
#if _debug
							Debug.Log($"Load Complate : Parallel, Time : {sw.ElapsedMilliseconds}");
#endif
							}
						}
					}
					break;
			}

			// 모든 로딩 페이지 완료 여부 확인
			bool isComplatePageAll = true;
			for (int i = 0; i < isComplatePage.Length; ++i)
			{
				isComplatePageAll = isComplatePageAll && isComplatePage[i];
			}

			if (isComplatePageAll)
			{
				PostLoading();

				LoadComplate();
			}
		}

		private void LoadComplate()
		{
#if _debug
			Debug.Log($"Load Complate!!!, Time : {sw.ElapsedMilliseconds}");
			sw.Stop();
#endif

			this.gameObject.SetActive(false);
#if _debug
			// 이후 메인 씬 호출
			UnityEngine.SceneManagement.SceneManager.LoadScene(isLoadDevScene ? CstrSceneDev : CstrSceneMain, 
				UnityEngine.SceneManagement.LoadSceneMode.Single);
#else
			// 이후 메인 씬 호출
			UnityEngine.SceneManagement.SceneManager.LoadScene("01_Main", UnityEngine.SceneManagement.LoadSceneMode.Single);
#endif
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Threading.Tasks;

namespace Proto_00_N
{
	public class SceneMain_Loading : MonoBehaviour
	{
		public static SceneMain_Loading Single { get; private set; }

		[Header("Managed")]
		[SerializeField] private List<Loading_PageBase>	listSerialPage;		// ���� �ε�
		[SerializeField] private List<Loading_PageBase>	listParallelPage;	// ���� �ε�
		[SerializeField] private List<Loading_PageBase> listCollectPage;	// �ϰ� �ε�

		private bool[] isComplatePage = new bool[(int)Loading_PageBase.EAsyncType.Max];

		private int iPageCountParallel;
		private int iPageCountCollect;

		private object objSwapLockParallel = new object();

#if _debug
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

			// �ε������� �ʱ�ȭ
			int i;

			i = 0;
			listSerialPage.ForEach(p => { p.Init(i++, Loading_PageBase.EAsyncType.Serial); p.gameObject.SetActive(false); });
			
			i = 0;
			listCollectPage.ForEach(p => { p.Init(i++, Loading_PageBase.EAsyncType.Collect); p.gameObject.SetActive(false); });

			i = 0;
			listParallelPage.ForEach(p => { p.Init(i++, Loading_PageBase.EAsyncType.Parallel); p.gameObject.SetActive(false); });

			// �ε������� ����
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
			// ���ɿ� ������ ũ�� ����, ������ ��ü �ʱ�ȭ
			CustomRoutine.Init();

			BattleManager.Single.JustCall();
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

			// ��� �ε� ������ �Ϸ� ���� Ȯ��
			bool isComplatePageAll = true;
			for (int i = 0; i < isComplatePage.Length; ++i)
			{
				isComplatePageAll = isComplatePageAll && isComplatePage[i];
			}

			if (isComplatePageAll)
			{
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

			// ���� ���� �� ȣ��
			UnityEngine.SceneManagement.SceneManager.LoadScene("01_Main", UnityEngine.SceneManagement.LoadSceneMode.Single);
		}
	}
}
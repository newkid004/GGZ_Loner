using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Loading_PageBase : MonoBehaviour
	{
		public enum EAsyncType
		{
			Serial,		// 직렬
			Parallel,	// 병렬
			Collect,	// 일괄
			Max,
		}

		public int iPageIndex { get; private set; }
		public EAsyncType eAsyncType { get; private set; }

		public bool isComplate { get; private set; }

		public virtual void Init(int iPageIndex, EAsyncType eAsyncType)
		{
			this.iPageIndex = iPageIndex;
			this.eAsyncType = eAsyncType;

			isComplate = false;
		}

		public virtual void ProcessLoad()
		{
			gameObject.SetActive(true);
		}

		private void PreComplate()
		{
			isComplate = true;
		}

		private void NextComplate()
		{
			gameObject.SetActive(false);
			SceneMain_Loading.Single.OnComplatedPage(this, eAsyncType);
		}

		protected virtual void OnComplate() { }

		// 상속된 객체에서 반드시 호출 필요
		protected void ProcessLoadComplate()
		{
			PreComplate();
			OnComplate();
			NextComplate();
		}
	}
}
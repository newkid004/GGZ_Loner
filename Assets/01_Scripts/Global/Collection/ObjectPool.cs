using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class PooledObject : MonoBehaviour
	{
		public int iOwnSequenceID { get; set; }			// Pool 제외 설정 불가
		public ObjectPoolBase opParent { get; set; }
		public System.Type typeOwn { get; set; }

		public void Push()
		{
			opParent.Push(this);
		}

		public virtual void OnPopedFromPool() { }	// 상속받는 객체는 해당 함수를 반드시 먼저 호출
		public virtual void OnPushedToPool() { }	// 상속받는 객체는 해당 함수를 반드시 마지막에 호출

		public override string ToString()
		{
			return $"{typeOwn} : {iOwnSequenceID}";
		}
	}

	[System.Serializable]
	public abstract class ObjectPoolBase
	{
		[SerializeField]
		protected Transform trContainRoot;

		public Queue<PooledObject> qPooledObject { get; protected set; }
		public HashSet<PooledObject> hsActiveObject { get; protected set; }

		// 자식 객체에서 초기화
		protected ObjectPoolBase opRoot;
		public int iSequenceID { get; protected set; }
		public Dictionary<int, PooledObject> dictTotalObject { get; protected set; }

		protected ObjectPoolBase()
		{
			iSequenceID = 0;
			qPooledObject = new Queue<PooledObject>();
			hsActiveObject = new HashSet<PooledObject>();
		}

		public void IncreaseSequenceID() => ++iSequenceID;

		public abstract void Push(PooledObject objPooled);
	}

	[System.Serializable]
	public class ObjectPool<T> : ObjectPoolBase
		where T : PooledObject
	{
		private Dictionary<System.Type, ObjectPoolBase> dictDerivedPool;

		[SerializeField] private int iPrePoolingCount;
		[SerializeField] private T _tOrigin;
		private Transform trOrigin;

		public int iPooledCount { get => qPooledObject.Count; }
		public int iUsingCount { get => hsActiveObject.Count; }
		public T tOrigin 
		{
			get => _tOrigin;
			protected set
			{
				_tOrigin = value;
				if (null != _tOrigin)
				{
					trOrigin = _tOrigin.transform;
				}
			}
		}

		public ObjectPool(T tOrigin = null) : base()
		{
			dictDerivedPool = new Dictionary<System.Type, ObjectPoolBase>();

			if (tOrigin != null)
			{
				_tOrigin = tOrigin;
			}
		}

		public void Init()
		{
			if (null != _tOrigin)
			{
				base.iSequenceID = 0;
				base.opRoot = this;
				base.dictTotalObject = new Dictionary<int, PooledObject>();

				tOrigin.gameObject.SetActive(false);
				trOrigin = _tOrigin.transform;

				if (0 < iPrePoolingCount)
				{
					List<T> listObject = new List<T>(iPrePoolingCount);

					for (int i = 0; i < iPrePoolingCount; ++i)
					{
						listObject.Add(Pop());
					}
					for (int i = 0; i < iPrePoolingCount; ++i)
					{
						listObject[i].Push();
					}
				}
			}
		}

		public T Pop(Transform trParent = null)
		{
			return Pop(trParent, this);
		}

		protected T Pop(Transform trParent, ObjectPoolBase oPoolParent)
		{
			PooledObject objResult;

			if (0 < qPooledObject.Count)
			{
				// 풀링된 객체가 있을 때
				objResult = qPooledObject.Dequeue();
			}
			else
			{
				// 풀링된 객체가 없을 때 : 생성
				objResult = tOrigin != null ?
					Object.Instantiate(tOrigin.gameObject).GetComponent<T>() :
					new GameObject().AddComponent<T>();

				if (oPoolParent == null)
				{
					objResult.opParent = this;
				}
				else
				{
					objResult.opParent = oPoolParent;
				}

				opRoot.IncreaseSequenceID();
				objResult.iOwnSequenceID = opRoot.iSequenceID;
				objResult.typeOwn = typeof(T);

				// 전체 풀링 관리목록에 추가
				opRoot.dictTotalObject.Add(objResult.iOwnSequenceID, objResult);
			}

			if (trParent != null)
			{
				objResult.transform.SetParent(trParent, false);
			}
			else if (this.trContainRoot != null)
			{
				objResult.transform.SetParent(this.trContainRoot, false);
			}

			objResult.OnPopedFromPool();
			objResult.gameObject.SetActive(true);

			hsActiveObject.Add(objResult);

			return (T)objResult;
		}

		public TDerived Pop<TDerived>(Transform trParent = null) where TDerived : T
		{
			if (typeof(T) == typeof(TDerived))
			{
				return (TDerived)Pop(trParent, this);
			}
			else
			{
				ObjectPoolBase oPoolParent = dictDerivedPool.GetDef(typeof(TDerived));
				if (null == oPoolParent)
				{
					oPoolParent = RegistDerivePool<TDerived>();
				}

				return ((ObjectPool<TDerived>)oPoolParent).Pop(trParent, oPoolParent);
			}
		}

		protected ObjectPool<TDerived> RegistDerivePool<TDerived>() where TDerived : T
		{
			ObjectPool<TDerived> oPoolDerived = new ObjectPool<TDerived>();
			Component cDerivedOrigin = tOrigin.GetType() == typeof(TDerived) ?
				tOrigin : 
				Object.Instantiate(tOrigin);

			GameObject objDerivedOrigin = cDerivedOrigin.gameObject;

			// 기존값 제거
			Object.DestroyImmediate(cDerivedOrigin);

			// 변경된 Derived 컴포넌트 추가
			cDerivedOrigin = objDerivedOrigin.AddComponent<TDerived>();

			// 값 입력
			cDerivedOrigin.GetCopyOf(this.tOrigin);

			oPoolDerived.opRoot = this.opRoot;
			oPoolDerived.trContainRoot = this.trContainRoot;
			oPoolDerived.tOrigin = (TDerived)cDerivedOrigin;

			objDerivedOrigin.gameObject.SetActive(false);

			dictDerivedPool.SetSafe(typeof(TDerived), oPoolDerived);

			return oPoolDerived;
		}

		// GameObject의 Active 비활성화 시 자동 호출
		public override void Push(PooledObject objPooled)
		{
			if (false == objPooled.gameObject.activeSelf)
				return;

			if (typeof(T) == objPooled.typeOwn)
			{
				objPooled.OnPushedToPool();
				objPooled.transform.SetParent(trOrigin.parent, false);
				objPooled.gameObject.SetActive(false);
				qPooledObject.Enqueue(objPooled);
				hsActiveObject.Remove(objPooled);
			}
			else
			{
				dictDerivedPool.GetDef(objPooled.typeOwn).Push(objPooled);
			}
		}

		public T GetObject(int iSequenceID)
		{
			return (T)opRoot.dictTotalObject.GetDef(iSequenceID);
		}

		public TDerived GetRandomObjectInActive<TDerived>() where TDerived : T
		{
			List<PooledObject> listUsing;
			if (typeof(T) == typeof(TDerived))
			{
				listUsing = new List<PooledObject>(hsActiveObject);
			}
			else
			{
				ObjectPool<TDerived> oPoolDerived = (ObjectPool<TDerived>)dictDerivedPool.GetDef(typeof(TDerived));
				listUsing = new List<PooledObject>(oPoolDerived.hsActiveObject);
			}

			return (TDerived)listUsing[Random.Range(0, listUsing.Count)];
		}

		public void LoopOnActive(System.Action<T> act)
		{
			List<PooledObject> listUsing = new List<PooledObject>(hsActiveObject);

			listUsing.ForEach(obj => act((T)obj));
		}

		public void LoopOnActive<TDerived>(System.Action<TDerived> act) where TDerived : T
		{
			ObjectPool<TDerived> dictDerived = (ObjectPool<TDerived>)dictDerivedPool.GetDef(typeof(TDerived));

			if (dictDerived != null)
			{
				dictDerived.LoopOnActive(act);
			}
		}

		public void LoopOnActiveTotal(System.Action<T> act)
		{
			List<PooledObject> listUsing = new List<PooledObject>(hsActiveObject);
			foreach (var pair in dictDerivedPool)
			{
				listUsing.AddRange(pair.Value.hsActiveObject);
			}

			listUsing.ForEach(obj => act((T)obj));
		}

		public void LoopOnActive(System.Func<T, bool> act)
		{
			List<PooledObject> listUsing = new List<PooledObject>(hsActiveObject);

			int iCount = listUsing.Count;
			for (int i = 0; i < iCount; ++i)
			{
				if (act((T)listUsing[i]) == false)
					break;
			}
		}

		public void LoopOnActive<TDerived>(System.Func<TDerived, bool> act) where TDerived : T
		{
			ObjectPool<TDerived> dictDerived = (ObjectPool<TDerived>)dictDerivedPool.GetDef(typeof(TDerived));

			if (dictDerived != null)
			{
				dictDerived.LoopOnActive(act);
			}
		}

		public void LoopOnActiveTotal(System.Func<T, bool> act)
		{
			List<PooledObject> listUsing = new List<PooledObject>(hsActiveObject);
			foreach (var pair in dictDerivedPool)
			{
				listUsing.AddRange(pair.Value.hsActiveObject);
			}

			int iCount = listUsing.Count;
			for (int i = 0; i < iCount; ++i)
			{
				if (act((T)listUsing[i]) == false)
					break;
			}
		}

		public void CollectAllObject() => LoopOnActive(obj => obj.Push());
	}
}
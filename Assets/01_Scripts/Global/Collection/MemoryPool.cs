using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class PooledMemory
	{
		public int iOwnSequenceID { get; set; }			// Pool 제외 설정 불가
		public MemoryPoolBase opParent { get; set; }
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
	public abstract class MemoryPoolBase
	{
		[SerializeField]
		protected Queue<PooledMemory> qPooledObject;
		protected HashSet<PooledMemory> hsActiveObject;

		// 자식 객체에서 초기화
		protected MemoryPoolBase opRoot;
		public int iSequenceID { get; protected set; }
		public Dictionary<int, PooledMemory> dictTotalObject { get; protected set; }

		protected MemoryPoolBase()
		{
			iSequenceID = 0;
			qPooledObject = new Queue<PooledMemory>();
			hsActiveObject = new HashSet<PooledMemory>();
		}

		public void IncreaseSequenceID() => ++iSequenceID;

		public abstract void Push(PooledMemory objPooled);
	}

	[System.Serializable]
	public class MemoryPool<T> : MemoryPoolBase
		where T : PooledMemory, new()
	{
		private Dictionary<System.Type, MemoryPoolBase> dictDerivedPool;

		[SerializeField] private int iPrePoolingCount;

		public int iPooledCount { get => qPooledObject.Count; }
		public int iUsingCount { get => hsActiveObject.Count; }

		public MemoryPool() : base()
		{
			dictDerivedPool = new Dictionary<System.Type, MemoryPoolBase>();
		}

		public void Init()
		{
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

		public T Pop()
		{
			return Pop(this);
		}

		protected T Pop(MemoryPoolBase oPoolParent)
		{
			PooledMemory objResult;

			if (0 < qPooledObject.Count)
			{
				// 풀링된 객체가 있을 때
				objResult = qPooledObject.Dequeue();
			}
			else
			{
				// 풀링된 객체가 없을 때 : 생성
				objResult = new T();

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

			objResult.OnPopedFromPool();

			hsActiveObject.Add(objResult);

			return (T)objResult;
		}

		public TDerived Pop<TDerived>() where TDerived : T, new()
		{
			if (typeof(T) == typeof(TDerived))
			{
				return (TDerived)Pop(this);
			}
			else
			{
				MemoryPoolBase oPoolParent = dictDerivedPool.GetDef(typeof(TDerived));
				if (null == oPoolParent)
				{
					oPoolParent = RegistDerivePool<TDerived>();
				}

				return ((MemoryPool<TDerived>)oPoolParent).Pop(oPoolParent);
			}
		}

		protected MemoryPool<TDerived> RegistDerivePool<TDerived>() where TDerived : T, new()
		{
			MemoryPool<TDerived> oPoolDerived = new MemoryPool<TDerived>();

			oPoolDerived.opRoot = this.opRoot;
			dictDerivedPool.SetSafe(typeof(TDerived), oPoolDerived);

			return oPoolDerived;
		}

		// GameObject의 Active 비활성화 시 자동 호출
		public override void Push(PooledMemory objPooled)
		{
			if (typeof(T) == objPooled.typeOwn)
			{
				objPooled.OnPushedToPool();
				qPooledObject.Enqueue(objPooled);
				hsActiveObject.Remove(objPooled);
			}
			else
			{
				dictDerivedPool.GetDef(objPooled.typeOwn).Push(objPooled);
			}
		}

		public T GetMemory(int iSequenceID)
		{
			return (T)opRoot.dictTotalObject.GetDef(iSequenceID);
		}

		public TDerived GetRandomObjectInActive<TDerived>() where TDerived : T, new()
		{
			List<PooledMemory> listUsing;
			if (typeof(T) == typeof(TDerived))
			{
				listUsing = new List<PooledMemory>(hsActiveObject);
			}
			else
			{
				MemoryPool<TDerived> oPoolDerived = (MemoryPool<TDerived>)dictDerivedPool.GetDef(typeof(TDerived));
				listUsing = new List<PooledMemory>(oPoolDerived.hsActiveObject);
			}

			return (TDerived)listUsing[Random.Range(0, listUsing.Count)];
		}

		public void LoopOnActive(System.Action<T> act)
		{
			List<PooledMemory> listUsing = new List<PooledMemory>(hsActiveObject);

			listUsing.ForEach(obj => act((T)obj));
		}

		public void LoopOnActive<TDerived>(System.Action<TDerived> act) where TDerived : T, new()
		{
			MemoryPool<TDerived> dictDerived = (MemoryPool<TDerived>)dictDerivedPool.GetDef(typeof(TDerived));

			if (dictDerived != null)
			{
				dictDerived.LoopOnActive(act);
			}
		}

		public void CollectAllObject() => LoopOnActive(obj => obj.Push());

	}
}


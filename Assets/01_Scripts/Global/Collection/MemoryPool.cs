using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class PooledMemory
	{
		public int iOwnSequenceID { get; set; }			// Pool ���� ���� �Ұ�
		public MemoryPoolBase opParent { get; set; }
		public System.Type typeOwn { get; set; }

		public void Push()
		{
			opParent.Push(this);
		}

		public virtual void OnPopedFromPool() { }	// ��ӹ޴� ��ü�� �ش� �Լ��� �ݵ�� ���� ȣ��
		public virtual void OnPushedToPool() { }	// ��ӹ޴� ��ü�� �ش� �Լ��� �ݵ�� �������� ȣ��

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

		// �ڽ� ��ü���� �ʱ�ȭ
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
				// Ǯ���� ��ü�� ���� ��
				objResult = qPooledObject.Dequeue();
			}
			else
			{
				// Ǯ���� ��ü�� ���� �� : ����
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

				// ��ü Ǯ�� ������Ͽ� �߰�
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

		// GameObject�� Active ��Ȱ��ȭ �� �ڵ� ȣ��
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


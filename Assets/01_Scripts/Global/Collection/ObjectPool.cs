using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class PooledObject : MonoBehaviour
	{
		public int iOwnSequenceID { get; set; }			// Pool ���� ���� �Ұ�
		public ObjectPoolBase opParent { get; set; }
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
	public abstract class ObjectPoolBase
	{
		[SerializeField]
		protected Transform trContainRoot;

		public Queue<PooledObject> qPooledObject { get; protected set; }
		public HashSet<PooledObject> hsActiveObject { get; protected set; }

		// �ڽ� ��ü���� �ʱ�ȭ
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
				// Ǯ���� ��ü�� ���� ��
				objResult = qPooledObject.Dequeue();
			}
			else
			{
				// Ǯ���� ��ü�� ���� �� : ����
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

				// ��ü Ǯ�� ������Ͽ� �߰�
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

			// ������ ����
			Object.DestroyImmediate(cDerivedOrigin);

			// ����� Derived ������Ʈ �߰�
			cDerivedOrigin = objDerivedOrigin.AddComponent<TDerived>();

			// �� �Է�
			cDerivedOrigin.GetCopyOf(this.tOrigin);

			oPoolDerived.opRoot = this.opRoot;
			oPoolDerived.trContainRoot = this.trContainRoot;
			oPoolDerived.tOrigin = (TDerived)cDerivedOrigin;

			objDerivedOrigin.gameObject.SetActive(false);

			dictDerivedPool.SetSafe(typeof(TDerived), oPoolDerived);

			return oPoolDerived;
		}

		// GameObject�� Active ��Ȱ��ȭ �� �ڵ� ȣ��
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
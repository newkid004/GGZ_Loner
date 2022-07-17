using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public class ProtoPooledObject : PooledObject
	{
		private static int iObjectSequenceID = 0;

		public int iObjectID { get; protected set; }

		private void Awake()
		{
			Init();
		}

		protected virtual void Init()
		{
			iObjectID = iObjectSequenceID++;
		}

		public virtual void ReconnectRefSelf()
		{
		}

		public override void OnPopedFromPool()
		{
			base.OnPopedFromPool();
			iObjectID = iObjectSequenceID++;
		}

		public override void OnPushedToPool()
		{
			iObjectID = 0;
			base.OnPushedToPool();
		}
	}
}

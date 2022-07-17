using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public class SingletonBase<T> where T : SingletonBase<T>, new()
	{
		protected SingletonBase() { }

		private static T _Single;

		public static T Single
		{
			get
			{
				if (_Single == null)
				{
					_Single = new T();
					_Single.Init();
				}

				return _Single;
			}
		}

		protected virtual void Init() { }

		public void JustCall() { }
	}
}
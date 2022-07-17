using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;

namespace Proto_00_N
{
	public abstract class CSVObjectBase<T> where T : CSVObjectBase<T>, new()
	{
		public static class Manager<TKey>
		{
			private static Dictionary<TKey, T> dictContainer = new Dictionary<TKey, T>();

			public static void Init(List<Dictionary<string, object>> objCSV)
			{
				dictContainer.Clear();

				FieldInfo fi = typeof(T).GetField("ID",
					BindingFlags.NonPublic |
					BindingFlags.Public |
					BindingFlags.Static |
					BindingFlags.Instance);

				foreach (var item in objCSV)
				{
					T tItem = new T();
					tItem.WriteToCSVObject(item);
					dictContainer.Add((TKey)fi.GetValue(tItem), tItem);
				}
			}

			public static int Count => dictContainer.Count;

			public static T Get(TKey key) => dictContainer.GetDef(key);
			public static void ForEach(Action<TKey, T> actLoop) => dictContainer.ForEach(actLoop);

			public static T Search(Predicate<T> predicate)
			{
				T result = default(T);

				foreach (var item in dictContainer)
				{
					if (predicate(item.Value))
					{
						result = item.Value;
					}
				}

				return result;
			}
		}

		private void WriteToCSVObject(Dictionary<string, object> dictObject)
		{
			FieldInfo[] arrFI = typeof(T).GetFields(
					BindingFlags.NonPublic |
					BindingFlags.Public |
					BindingFlags.Static |
					BindingFlags.Instance);

			foreach (var item in arrFI)
			{
#if _debug
				try
				{
#endif
					item.SetValue(this, dictObject[item.Name]);
#if _debug
				}
				catch
				{
					Debug.LogAssertion($"CSVObjectBase.WriteToCSVObject : Not Match Member\n" +
						$": {typeof(T).Name}, {item.Name}({item.FieldType.Name}) <- {dictObject[item.Name]}");
				}
#endif
			}
		}
	}
}

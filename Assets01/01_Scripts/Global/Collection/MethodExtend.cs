using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class MethodExtend
{
	#region // ----- Array ----- //

	public static T Back<T>(this T[] arr, RequireClass<T> PleaseIgnoreThisParameter = null) where T : class
	{
		return 0 < arr.Length ? arr[arr.Length - 1] : null;
	}

	public static T Back<T>(this T[] arr, RequireStruct<T> PleaseIgnoreThisParameter = null) where T : struct
	{
		return 0 < arr.Length ? arr[arr.Length - 1] : new T();
	}

	#endregion

	#region // ----- List ----- //

	public static int CountActive(this List<GameObject> list)
	{
		for (int i = 0; i < list.Count; ++i)
		{
			if (!list[i].activeSelf)
				return i;
		}
		return list.Count;
	}

	public static int CountActive<T>(this List<T> list) where T : Component
	{
		for (int i = 0; i < list.Count; ++i)
		{
			if (!list[i].gameObject.activeSelf)
				return i;
		}
		return list.Count;
	}

	public static void Shuffle<T>(this List<T> list)
	{
		int nCount = list.Count;

		for (int i = 0; i < nCount; ++i)
		{
			int nIdxRandom = Random.Range(0, nCount);

			T temp = list[i];
			list[i] = list[nIdxRandom];
			list[nIdxRandom] = temp;
		}
	}

	public static void Shuffle<T>(this List<T> list, int nRangeMin = -1, int nRangeMax = -1)
	{
		int nCount = list.Count;

		for (int i = 0; i < nCount; ++i)
		{
			int nIdxRandom = Random.Range(0, nCount);

			T temp = list[i];
			list[i] = list[nIdxRandom];
			list[nIdxRandom] = temp;
		}
	}

	// public static T GetRandomItem<T>(this List<T> list)
	// {
	// 	if (list.Count <= 0)
	// 		return default(T);
	// 
	// 	return list[GC.GetRandom(0, list.Count)];
	// }

	public static void Resize<T>(this List<T> list, int size, System.Func<T> initFunc)
	{
		int cur = list.Count;
		if (size < cur)
			list.RemoveRange(size, cur - size);
		else if (size > cur)
		{
			if (size > list.Capacity)
				list.Capacity = size;
			int addCount = size - cur;
			for (int i = 0; i < addCount; ++i)
			{
				list.Add(initFunc());
			}
		}
	}

	public static void Resize<T>(this List<T> list, int size) where T : new()
	{
		Resize(list, size, () => new T());
	}

	/// <summary> ���� ��ȸ ��, ���� ���̶�� �߰� �� true ��ȯ </summary>
	public static bool AddUnique<T>(this List<T> list, T obj)
	{
		if (list.Contains(obj))
			return false;

		list.Add(obj);

		return true;
	}

	public static T Back<T>(this List<T> list) where T : class
	{
		return 0 < list.Count ? list[list.Count - 1] : null;
	}

	public static T GetSafe<T>(this List<T> list, int index) where T : new()
	{
		if (index <= list.Count)
		{
			list.Resize(index + 1);
		}

		return list[index];
	}

	public static void RemoveToSwitchBack<T>(this List<T> list, int iRemoveIndex)
	{
		if (iRemoveIndex <= list.Count)
		{
			list[iRemoveIndex] = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
		}
	}

	#endregion

	#region // ----- Dictionary ----- //

	#region Dictionary.LoopLinear - Dictionary enumerator �� foreach�� ���� boxing, unboxing ���� �޼ҵ�, 4.x���� �̻��

	public static void LoopLinear<TKey, TValue>(this IDictionary<TKey, TValue> dict, System.Action<TValue> act)
	{
		List<TValue> listValue = new List<TValue>(dict.Values);
		for (int i = 0; i < listValue.Count; ++i)
			act(listValue[i]);
	}

	/// <summary> bool�� ��ȯ�ϴ� Func�� �ݺ��� ���� ���� </summary>
	/// <returns> break�� ���� �ߴܵǾ��� ��� false </returns>
	public static bool LoopLinear<TKey, TValue>(this IDictionary<TKey, TValue> dict, System.Func<TValue, bool> act, bool isBreak = true)
	{
		List<TValue> listValue = new List<TValue>(dict.Values);
		for (int i = 0; i < listValue.Count; ++i)
		{
			if (!act(listValue[i]))
			{
				if (isBreak)
					return false;
				else
					continue;
			}
		}
		return true;
	}

	public static void LoopLinear<TKey, TValue>(this IDictionary<TKey, TValue> dict, System.Action<TKey> act)
	{
		List<TKey> listKey = new List<TKey>(dict.Keys);
		for (int i = 0; i < listKey.Count; ++i)
			act(listKey[i]);
	}

	/// <summary> bool�� ��ȯ�ϴ� Func�� �ݺ��� ���� ���� </summary>
	/// <returns> break�� ���� �ߴܵǾ��� ��� false </returns>
	public static bool LoopLinear<TKey, TValue>(this IDictionary<TKey, TValue> dict, System.Func<TKey, bool> act, bool isBreak = true)
	{
		List<TKey> listKey = new List<TKey>(dict.Keys);
		for (int i = 0; i < listKey.Count; ++i)
		{
			if (!act(listKey[i]))
			{
				if (isBreak)
					return false;
				else
					continue;
			}
		}
		return true;
	}

	public static void LoopLinear<TKey, TValue>(this IDictionary<TKey, TValue> dict, System.Action<TKey, TValue> action)
	{
		LoopLinear(dict, key => action(key, dict[key]));
	}

	/// <summary> bool�� ��ȯ�ϴ� Func�� �ݺ��� ���� ���� </summary>
	/// <returns> break�� ���� �ߴܵǾ��� ��� false </returns>
	public static bool LoopLinear<TKey, TValue>(this IDictionary<TKey, TValue> dict, System.Func<TKey, TValue, bool> act, bool isBreak = true)
	{
		return LoopLinear(dict, key => act(key, dict[key]), isBreak);
	}

	public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dict, System.Action<TKey, TValue> action)
	{
		foreach (KeyValuePair<TKey, TValue> pair in dict)
			action(pair.Key, pair.Value);
	}

	#endregion

	// ref : https://stackoverflow.com/questions/2974519/generic-constraints-where-t-struct-and-where-t-class
	public class RequireStruct<T> where T : struct { private RequireStruct() { } }
	public class RequireClass<T> where T : class { private RequireClass() { } }

	#region Dictionary.GetSafe - �� ȹ��, ���� �� �߰�

	public static TDerive GetSafe<TKey, TValue, TDerive>(this IDictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TDerive> PleaseIgnoreThisParameter = null)
		where TValue : class
		where TDerive : class, TValue, new()
	{
		TValue value;
		if (!dict.TryGetValue(key, out value))
			dict.Add(key, defValue == null ? (value = new TDerive()) : defValue);

		return (TDerive)value;
	}

	public static TValue GetSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defValue = default(TValue), RequireStruct<TValue> PleaseIgnoreThisParameter = null)
		where TValue : struct
	{
		TValue value;
		if (!dict.TryGetValue(key, out value))
			dict.Add(key, value = defValue);

		return value;
	}

	public static TValue GetSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TValue> PleaseIgnoreThisParameter = null)
		where TValue : class, new()
	{
		TValue value;
		if (!dict.TryGetValue(key, out value))
			dict.Add(key, defValue == null ? (value = new TValue()) : defValue);

		return value;
	}

	public static TValue[] GetSafe<TKey, TValue>(this IDictionary<TKey, TValue[]> dict, TKey key, TValue[] defValue = default(TValue[]), RequireStruct<TValue> PleaseIgnoreThisParameter = null)
		where TValue : struct
	{
		TValue[] value;
		if (!dict.TryGetValue(key, out value))
			dict.Add(key, value = defValue);

		return value;
	}

	#endregion

	#region Dictionary.GetDef - �� ȹ��, ���� �� Default ��ȯ

	public static TDerive GetDef<TKey, TValue, TDerive>(this IDictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TDerive> PleaseIgnoreThisParameter = null)
		where TValue : class
		where TDerive : class, TValue
	{
		TValue value;
		if (!dict.TryGetValue(key, out value))
			return (TDerive)defValue;

		return (TDerive)value;
	}

	public static TValue GetDef<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defValue = default(TValue), RequireStruct<TValue> PleaseIgnoreThisParameter = null)
		where TValue : struct
	{
		if (!dict.ContainsKey(key))
			return defValue;

		return dict[key];
	}

	public static TValue GetDef<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defValue = null, RequireClass<TValue> PleaseIgnoreThisParameter = null)
		where TValue : class
	{
		TValue value;
		if (!dict.TryGetValue(key, out value))
			return defValue;

		return value;
	}

	#endregion

	#region Dictionary.SetSafe - �� ����, ���� �� �߰�

	public static TDerive SetSafe<TKey, TValue, TDerive>(this IDictionary<TKey, TValue> dict, TKey key, TValue value, RequireClass<TDerive> PleaseIgnoreThisParameter = null)
		where TValue : class
		where TDerive : class, TValue
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			return (TDerive)value;
		}

		return (TDerive)(dict[key] = value);
	}

	public static TValue SetSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value, RequireStruct<TValue> PleaseIgnoreThisParameter = null)
		where TValue : struct
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			return value;
		}

		return dict[key] = value;
	}

	public static TValue SetSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value, RequireClass<TValue> PleaseIgnoreThisParameter = null)
		where TValue : class
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			return value;
		}

		return dict[key] = value;
	}

	#endregion

	#region Dictionary.ActSafe - �� ����, ���� �� �߰�

	public static TValue ActSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, System.Action<bool, TValue> act, RequireStruct<TValue> PleaseIgnoreThisParameter = null)
		where TValue : struct
	{
		bool isInsert = false;

		TValue value;
		if (!dict.TryGetValue(key, out value))
		{
			dict.Add(key, value = new TValue());
			isInsert = true;
		}
		act(isInsert, value);

		return value;
	}

	public static TValue ActSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, System.Action<bool, TValue> act, RequireClass<TValue> PleaseIgnoreThisParameter = null)
		where TValue : class, new()
	{
		bool isInsert = false;

		TValue value;
		if (!dict.TryGetValue(key, out value))
		{
			dict.Add(key, value = new TValue());
			isInsert = true;
		}
		act(isInsert, value);

		return value;
	}

	#endregion

	#endregion

	#region // ----- HashSet ----- //

	public static void ForEach<T>(this ISet<T> hs, System.Action<T> act) { foreach (T item in hs) act(item); }

	#endregion

	// ----- GameObject ----- //
	public static T GetCopyOf<T>(this Component comp, T other) where T : Component
	{
		System.Type type = comp.GetType();
		if (false == other.GetType().IsSubclassOf(type)) return null; // type mis-match
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
		PropertyInfo[] pinfos = type.GetProperties(flags);
		foreach (var pinfo in pinfos)
		{
			if (pinfo.CanWrite)
			{
				try
				{
					pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
				}
				catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
			}
		}
		FieldInfo[] finfos = type.GetFields(flags);
		foreach (var finfo in finfos)
		{
			finfo.SetValue(comp, finfo.GetValue(other));
		}
		return comp as T;
	}

	// ----- Vector3 ----- //
	public static Vector2 Vec2(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}

	// ----- Vector2 ----- //
	public static Vector2 Rotate(this Vector2 v, float degrees)
	{
		// Ref : https://answers.unity.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html
		float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
		float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (sin * tx) + (cos * ty);
		return v;
	}

	// ----- Transform ----- //
	public static void MoveOnlyThisParent(this Transform tr, Vector3 vec3Distance)
	{
		Vector3 vec3SourPosition = tr.position;

		int iChildCount = tr.childCount;
		for (int i = 0; i < iChildCount; ++i)
		{
			Transform trChild = tr.GetChild(i);
			trChild.localPosition -= vec3Distance;
		}

		tr.position = vec3SourPosition + vec3Distance;
	}

	public static void PositionOnlyThisParent(this Transform tr, Vector3 vec3Position)
	{
		tr.MoveOnlyThisParent(vec3Position - tr.position);
	}

	// ----- int ----- //
	public static int ModStep(this int i, int iStep, int iMod)
	{
		return (i + iStep + iMod) % iMod;
	}

	public static int ModStep(this int i, int iStep, int iMin, int iMod)
	{
		int iRealMod = iMod - iMin;
		int iRealValue = i - iMin;
		return ((iRealValue + iStep + iRealMod) % iRealMod) + iMin;
	}

	// ----- float, double ----- //
	public static float LerpTo(this float FromA, float ToB, float Alpha) { return FromA + (ToB - FromA) * Alpha; }
	public static double LerpTo(this double FromA, double ToB, float Alpha) { return FromA + (ToB - FromA) * Alpha; }

	// ----- Array ----- //
	public static T[] SubArray<T>(this T[] arr, int iStart, int iEnd)
	{
		int iCount = iEnd - iStart;
		T[] arrResult = new T[iCount];

		for (int i = 0; i < iCount; ++i)
		{
			arrResult[i] = arr[iStart + i];
		}

		return arrResult;
	}

	// ----- Generic ----- //
	public static void Swap<T>(this ref T tSource, ref T tInput) where T : struct
	{
		T tTemp = tSource;
		tInput = tTemp;
		tSource = tTemp;
	}
}

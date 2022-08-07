using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace GGZ
{
	public abstract class CSVObjectBase<T, TKey> where T : CSVObjectBase<T, TKey>, new()
	{
		protected CSVObjectBase() { }

		public abstract TKey GetKey();

		public static class Manager
		{
			private static Dictionary<TKey, T> dictContainer = new Dictionary<TKey, T>();

			private static FieldInfo[] arrTypeFieldInfo = typeof(T).GetFields(
					BindingFlags.NonPublic |
					BindingFlags.Public |
					BindingFlags.Static |
					BindingFlags.Instance);

			public static void Init(List<Dictionary<string, object>> objCSV)
			{
				dictContainer.Clear();

				foreach (var csvItem in objCSV)
				{
					T tItem = new T();

					foreach (var fi in arrTypeFieldInfo)
					{
#if _debug
						try
						{
							fi.SetValue(tItem, csvItem[fi.Name]);
						}
						catch
						{
							Debug.LogAssertion($"CSVObjectBase.SetValueToCSVObject : Not Match Member\n" +
								$": {typeof(T).Name}, {fi.Name}({fi.FieldType.Name}) <- {csvItem.GetDef(fi.Name)}");
						}
#else

						fi.SetValue(tItem, csvItem[fi.Name]);
#endif
					}

					dictContainer.Add(tItem.GetKey(), tItem);
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

			public static void Add(T value) => dictContainer.Add(value.GetKey(), value);

			// Interact In Game
			public static readonly string cstrBasicPath = $"{Application.dataPath}/Resources/R_02_CSV";
			public static readonly string cstrResourcePath = "R_02_CSV";
			public static string GetTypeFilePath()
			{
				Type tDerive = typeof(T);
				string[] strSplitTypeName = tDerive.FullName.Split('+');
				return string.Join("/", strSplitTypeName, 1, strSplitTypeName.Length - 1);
			}

			public static void Save()	// To File
			{
				string strFilePath = $"{cstrBasicPath}/{GetTypeFilePath()}.csv";
				string strFilePathTemp = strFilePath.Replace(".csv", "__TEMP.csv");

				using (FileStream fs = File.Create(strFilePathTemp))
				{
					using (StreamWriter sw = new StreamWriter(fs, GlobalDefine.Environment.DefaultEncoding))
					{
						// Write Title
						string strTitle = string.Empty;
						arrTypeFieldInfo.ForEach(item => strTitle += item.Name + ',');
						strTitle = strTitle.Remove(strTitle.Length - 1);

						sw.WriteLine(strTitle);

						// Write Value
						StringBuilder sbLine = new StringBuilder();
						var data = new SortedList<TKey, T>(dictContainer);

						data.ForEach((key, value) =>
						{
							arrTypeFieldInfo.ForEach(item =>
							{
								if (false == item.FieldType.IsArray)
								{
									sbLine.Append(
										GetValidString(item, value, 
										(fi) => fi.FieldType == typeof(float), 
										(fi, obj) => fi.GetValue(obj)));
								}
								else
								{
									Array arr = item.GetValue(value) as Array;
									if (arr.Length == 1)
									{
										sbLine.Append("|" + 
											GetValidString(item, value,
											(fi) => fi.FieldType.GetElementType() == typeof(float),
											(fi, obj) => arr.GetValue(0)));
									}
									else
									{
										int iCount = arr.Length;
										for (int i = 0; i < iCount; ++i)
										{
											sbLine.Append(
												GetValidString(item, value,
												(fi) => fi.FieldType.GetElementType() == typeof(float),
												(fi, obj) => arr.GetValue(i)) + "|");
										}
										sbLine.Remove(sbLine.Length - 1, 1);
									}
								}
								sbLine.Append(",");
							});

							sw.WriteLine(sbLine.Remove(sbLine.Length - 1, 1).ToString());
							sbLine.Clear();
						});
					}
				}

				File.Delete(strFilePath);
				File.Move(strFilePathTemp, strFilePath);
			}

			private static string GetValidString(FieldInfo info, object obj, Func<FieldInfo, bool> funcCheckFloat, Func<FieldInfo, object, object> funcGetter)
			{
				if (funcCheckFloat(info))
				{
					string strFloat = funcGetter(info, obj).ToString();
					if (false == strFloat.Contains("."))
					{
						return $"{strFloat}.0";
					}
					else
					{
						return strFloat;
					}
				}
				else
				{
					return funcGetter(info, obj).ToString();
				}
			}

			public static void Load()	// To CSV
			{
				string strFilePath = $"{cstrResourcePath}/{GetTypeFilePath()}";
				CSVReader.ReadAsync(strFilePath, (dictResource) => Init(dictResource));
			}

			public static void Clear() => dictContainer.Clear();
		}
	}
}

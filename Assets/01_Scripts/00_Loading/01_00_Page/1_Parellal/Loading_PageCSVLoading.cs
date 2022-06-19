using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEditor;

namespace GGZ
{
	[CustomEditor(typeof(Loading_PageCSVLoading))]
	public class E_Loading_PageCSVLoading : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Reimport CSV Resource"))
			{
				Loading_PageCSVLoading scReimporter = (Loading_PageCSVLoading)target;
				scReimporter.Reimport();
			}

			base.OnInspectorGUI();
		}
	}

	public class Loading_PageCSVLoading : Loading_PageBase
	{
		[System.Serializable]
		public struct stInputCsvData
		{
			public string strPath;
			public List<string> listFile;
		}

		[SerializeField] private List<stInputCsvData> listInput;

		private Dictionary<string, Type> dictInputType;
		private int iNeedComplateCount;

		public override void ProcessLoad()
		{
			base.ProcessLoad();

			InitComplateCondition();
			InitInputReflection();

			LoadDataCSV();
		}

		private void InitComplateCondition()
		{
			iNeedComplateCount = 0;
			listInput.ForEach(data => iNeedComplateCount += data.listFile.Count );
		}

		private void InitInputReflection()
		{
			dictInputType = new Dictionary<string, Type>();
			InputCSVType(typeof(CSVData), typeof(CSVData).GetNestedTypes(BindingFlags.Public));
		}

		private void InputCSVType(Type t, Type[] arrInner)
		{
			if (arrInner.Length == 0)
			{
				dictInputType.Add(t.ToString(), t);
			}
			else
			{
				foreach (Type tInner in arrInner)
				{
					InputCSVType(tInner, tInner.GetNestedTypes(BindingFlags.Public));
				}
			}
		}

		private void LoadDataCSV()
		{
			string strBasicPath = "R_02_CSV";
			Type tRaw = typeof(CSVObjectBase<,>);

			ResourceRequest resReq;

			listInput.ForEach(data =>
			{
				data.listFile.ForEach(strCSV =>
				{
					// Type È¹µæ
					string strKey = $"GGZ.CSVData+{data.strPath.Replace("/", "+")}+{strCSV}";
					Type tMatch = dictInputType.GetDef(strKey);
#if _debug
					if (tMatch == null)
					{
						Debug.LogAssertion($"Loading_PageCSVLoading.LoadDataCSV : Type is Not Defined ({strCSV})");
					}
#endif
					resReq = CSVReader.ReadAsync($"{strBasicPath}/{data.strPath}/{strCSV}", (dictResource) =>
					{
						// GetKey ÇÁ·ÎÆÛÆ¼ È¹µæ
						MethodInfo miGetKey = tMatch.GetMethod("GetKey", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

						// Generic ÀÔ·Â
						Type tGen = tRaw.MakeGenericType(tMatch, miGetKey.ReturnType);

						// Manager Type È¹µæ
						Type tManager = tGen.GetNestedTypes(BindingFlags.Public)[0];
						Type tGenManager = tManager.MakeGenericType(tMatch, miGetKey.ReturnType);

						// Init ÇÔ¼ö È¹µæ ¹× È£Ãâ
						tGenManager.GetMethod("Init").Invoke(null, new object[] { dictResource });
					});

					// Request ¿Ï·á ½Ã ·Îµù¿Ï·á È®ÀÎ
					resReq.completed += (asyncOper) =>
					{
						if (Interlocked.Decrement(ref iNeedComplateCount) == 0)
						{
							ProcessLoadComplate();
						}
					};
				});
			});
		}

		public void InputLoadResourcePath(Dictionary<string, List<string>> dictPath)
		{
			listInput.Clear();

			dictPath.ForEach((key, value) =>
			{
				listInput.Add(new stInputCsvData()
				{
					strPath = key,
					listFile = value
				});
			});
		}

		public void Reimport()
		{
			string[] arrPathResource = new string[] { "Assets", "Resources", "R_02_CSV" };

			string strResourcePath = string.Join("/", arrPathResource);
			string[] strAssets = AssetDatabase.FindAssets("t:TextAsset", new string[] { strResourcePath });

			Dictionary<string, List<string>> dictInputPath = new Dictionary<string, List<string>>();

			int iCount = strAssets.Length;
			for (int i = 0; i < iCount; ++i)
			{
				AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(strAssets[i]));
				string[] strAssetPath = importer.assetPath.Split('/');

				if (strAssetPath[strAssetPath.Length - 1].EndsWith("csv"))
				{
					if (strAssetPath[strAssetPath.Length - 1].StartsWith("_"))
					{
						continue;
					}

					string[] strDirectoryPath = strAssetPath.SubArray(arrPathResource.Length, strAssetPath.Length - 1);
					string strCSVFilePath = strAssetPath[strAssetPath.Length - 1].Split('.')[0];

					dictInputPath.GetSafe(string.Join("/", strDirectoryPath)).Add(strCSVFilePath);
				}
			}

			Loading_PageCSVLoading pgLoading = gameObject.GetComponent<Loading_PageCSVLoading>();
			pgLoading.InputLoadResourcePath(dictInputPath);
		}
	}
}
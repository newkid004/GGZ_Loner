using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace GGZ
{
	[CustomEditor(typeof(EnumDefineBuilder))]
	public class E_Dev_EnumDefineBuilder : Editor
	{
		public override void OnInspectorGUI()
		{
			EnumDefineBuilder obj = (EnumDefineBuilder)target;

			if (GUILayout.Button("Build")) obj.Build(obj.runData);

			base.OnInspectorGUI();
		}
	}

	public class EnumDefineBuilder : MonoBehaviour
	{
		public abstract class BuildDataBase : MonoBehaviour
		{
			public class InnerData
			{
				public bool isLeafBranch;
				public string name;
				public List<InnerData> listInnerData = new List<InnerData>();
			}

			public TextAsset asset;
			public abstract List<InnerData> GetEnumList();
		}

		public BuildDataBase runData;

		/// <summary>
		/// 최하위 항목은 enum으로 감싸되
		/// 그 위 상위 항목은 static class로 감싸짐
		/// </summary>
		public void Build(BuildDataBase data)
		{
			string strFileName = data.asset.name;
			string strFilePath = AssetDatabase.GetAssetPath(data.asset);

			using (StreamWriter sw = new StreamWriter(strFilePath, false, GlobalDefine.Environment.DefaultEncoding))
			{
				int iDepth = 1;
				List<BuildDataBase.InnerData> listData = data.GetEnumList();

				sw.WriteLine("namespace GGZ");
				sw.WriteLine("{");
				{
					sw.WriteLine($"\tpublic static class {strFileName}");

					++iDepth;

					sw.WriteLine("\t{");
					listData.ForEach(d => CalcLeaf(d));
					listData.ForEach(d => WriteLineToData(sw, d, iDepth));
					sw.WriteLine("\t}");
				}
				sw.WriteLine("}");
			}

			Debug.Log("Build Complate");
		}

		private bool CalcLeaf(BuildDataBase.InnerData data)
		{
			bool bResult = false;

			if (0 < data.listInnerData.Count)
			{
				data.isLeafBranch = true;
				bResult = true;
			}
			else
			{
				data.listInnerData.ForEach(d =>
				{
					if (CalcLeaf(d))
					{
						data.isLeafBranch = true;
					}
				});
				bResult = false;
			}

			return bResult;
		}

		private void WriteLineToData(StreamWriter sw, BuildDataBase.InnerData data, int iDepth)
		{
			string strTab = new string('\t', iDepth);

			if (0 < data.listInnerData.Count)
			{
				if (data.isLeafBranch)
				{
					sw.WriteLine($"{strTab}public enum {data.name}");
				}
				else
				{
					sw.WriteLine($"{strTab}public static class {data.name}");
				}
				sw.WriteLine(strTab + "{");

				data.listInnerData.ForEach(d => WriteLineToData(sw, d, iDepth + 1));
				sw.WriteLine("\n" + strTab + "\tMAX");

				sw.WriteLine(strTab + "}\n");
			}
			else
			{
				if (false == (data.name.Contains("//") || string.IsNullOrEmpty(data.name)))
				{
					sw.WriteLine($"{strTab}{data.name},");
				}
				else
				{
					sw.WriteLine($"{data.name}");
				}
			}
		}
	}
}
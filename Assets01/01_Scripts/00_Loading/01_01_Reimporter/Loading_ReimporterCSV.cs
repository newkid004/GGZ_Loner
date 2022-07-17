using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Proto_00_N
{
	[CustomEditor(typeof(Loading_ReimporterCSV))]
	public class E_Loading_ReimportCSVResource : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Reimport CSV Resource"))
			{
				Loading_ReimporterCSV scReimporter = (Loading_ReimporterCSV)target;
				scReimporter.Reimport();
			}
		}
	}

	[RequireComponent(typeof(Loading_PageCSVLoading))]
	public class Loading_ReimporterCSV : MonoBehaviour
	{
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

				string[] strDirectoryPath = strAssetPath.SubArray(arrPathResource.Length, strAssetPath.Length - 1);
				string strCSVFilePath = strAssetPath[strAssetPath.Length - 1].Split('.')[0];

				dictInputPath.GetSafe(string.Join("/", strDirectoryPath)).Add(strCSVFilePath);
			}

			Loading_PageCSVLoading pgLoading = gameObject.GetComponent<Loading_PageCSVLoading>();
			pgLoading.InputLoadResourcePath(dictInputPath);
		}
	}
}


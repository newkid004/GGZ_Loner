using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace GGZ
{
	[CustomEditor(typeof(EnumDefineModifier))]
	public class E_Dev_EnumDefineModifier : Editor
	{
		public override void OnInspectorGUI()
		{
			EnumDefineModifier obj = (EnumDefineModifier)target;

			if (GUILayout.Button("Save : Editor -> File")) obj.SaveTotal();
			if (GUILayout.Button("Load : File -> Editor")) obj.LoadTotal();
			if (GUILayout.Button("Collect : Code -> Editor")) obj.CollectTotal();
			if (GUILayout.Button("Spread : Editor -> Code")) obj.SpreadTotal();

			base.OnInspectorGUI();
		}
	}

	public class EnumDefineModifier : MonoBehaviour
	{
		[System.Serializable]
		public class InputData
		{
			public TextAsset csFile;
			public List<string> listEnum = new List<string>();
		}

		[System.Serializable]
		public class ClassData : SaveMultiObjectBase<ClassData>
		{
			public override string strPathSave => $"Assets/Editor/04_Dev";

			[HideInInspector]
			public string strPath;

			public string className;
			public List<EnumData> listEnum = new List<EnumData>();
		}

		[System.Serializable]
		public class EnumData
		{
			public string enumName;
			public List<string> listEnum = new List<string>();
		}

		// Init in inspector only
		[SerializeField] private List<InputData> listCodeAsset;

		// Init in code only
		public List<ClassData> listClassData = new List<ClassData>();

		public void SaveTotal()
		{
			listClassData.ForEach(data => ClassData.Save(data.className, ref data));

			Debug.Log("Save Complate");
		}

		public int LoadTotal()
		{
			List<string> listError = new List<string>();

			for (int i = 0; i < listClassData.Count; ++i)
			{
				ClassData cData = listClassData[i];

				if (false == ClassData.Load(cData.className, ref cData))
				{
					listError.Add(cData.className);
				}
				else
				{
					listClassData[i] = cData;
				}
			}

			if (0 < listError.Count)
			{
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				listError.ForEach(str => strError += $"{str}\n");
				Debug.LogAssertion(strError);
			}
			else
			{
				Debug.Log("Load Complate");
			}

			return listError.Count;
		}

		public void CollectTotal()
		{
			listClassData.Clear();
			listClassData.Resize(listCodeAsset.Count);

			int iCount = listClassData.Count;
			for (int i = 0; i < iCount; ++i)
			{
				InputData iData = listCodeAsset[i];
				TextAsset tAsset = iData.csFile;

				ClassData cData = new ClassData();

				string strFileName = tAsset.name;
				if (false == ClassData.Load(strFileName, ref cData))
				{
					cData.className = strFileName;
					cData.strPath = AssetDatabase.GetAssetPath(tAsset);
					iData.listEnum.ForEach(strEnumName => cData.listEnum.Add(new EnumData() { enumName = strEnumName }));

					LoadClassData(cData);
					ClassData.Save(strFileName, ref cData);
				}

				listClassData[i] = cData;
			}

			Debug.Log("Collect Complate");
		}

		public int SpreadTotal()
		{
			List<string> listError = new List<string>();

			listClassData.ForEach(data =>
			{
				if (0 < SpreadClassData(data))
				{
					listError.Add(data.className);
				}
			});

			if (0 < listError.Count)
			{
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				listError.ForEach(str => strError += $"{str}\n");
				Debug.LogAssertion(strError);
			}
			else
			{
				Debug.Log("Spread Complate");
			}

			return listError.Count;
		}

		public int LoadClassData(ClassData classData)
		{
			List<EnumData> listTempEnum = new List<EnumData>(classData.listEnum);

			using (StreamReader sr = new StreamReader(classData.strPath, GlobalDefine.Environment.DefaultEncoding))
			{
				EnumData eData = null;
				string strEnumSearchTocken = $"enum ";

				for (int i = 0; false == sr.EndOfStream; ++i)
				{
					string strLine = sr.ReadLine();

					if (null == eData)
					{
						int iTextContainIndex = strLine.IndexOf(strEnumSearchTocken);
						if (0 < iTextContainIndex)
						{
							eData = listTempEnum.Find(data => strLine.EndsWith(data.enumName));
						}
					}
					else
					{
						if (strLine.Contains("}"))
						{
							listTempEnum.Remove(eData);
							eData = null;
						}
						else if(false == strLine.Contains("{"))
						{
							eData.listEnum.Add(strLine.TrimStart('\t').TrimEnd(','));
						}
					}
				}
			}

			if (0 < listTempEnum.Count)
			{
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				listTempEnum.ForEach(str => strError += $"{str}\n");
				Debug.LogAssertion(strError);
			}

			return listTempEnum.Count;
		}

		public int SpreadClassData(ClassData classData)
		{
			List<EnumData> listTempEnum = new List<EnumData>(classData.listEnum);

			string strFileNameSour = classData.strPath;
			string strFileNameCopy = strFileNameSour.Replace(".cs", "__COPY.cs");
			string strFileNameTemp = strFileNameSour.Replace(".cs", "__TEMP.cs");

			File.Copy(strFileNameSour, strFileNameCopy, true);

			using (FileStream fs = File.Create(strFileNameTemp))
			{
				using (StreamWriter sw = new StreamWriter(fs, GlobalDefine.Environment.DefaultEncoding))
				{
					string[] strTotalLine = File.ReadAllLines(strFileNameCopy, GlobalDefine.Environment.DefaultEncoding);

					EnumData eData = null;
					string strEnumSearchTocken = "enum ";

					for (int i = 0; i < strTotalLine.Length; ++i)
					{
						string strLine = strTotalLine[i];

						sw.WriteLine(strLine);

						if (null == eData)
						{
							int iTextContainIndex = strLine.IndexOf(strEnumSearchTocken);
							if (0 < iTextContainIndex)
							{
								eData = listTempEnum.Find(data => strLine.EndsWith(data.enumName));

								// 에디터 내 enum 입력
								int iTabCount = strLine.Count('\t');

								string strTab = new string('\t', iTabCount);

								sw.WriteLine(strTab + '{');

								foreach (string strEnum in eData.listEnum)
								{
									if (string.IsNullOrEmpty(strEnum) || strEnum.StartsWith("//"))
									{
										sw.WriteLine($"{strTab}\t{strEnum}");
									}
									else
									{
										sw.WriteLine($"{strTab}\t{strEnum},");	// 구분 쉼표 추가
									}
								}

								sw.WriteLine(strTab + '}');

								// 나머지 구문 ReadLine
								while (null != eData)
								{
									string strSourLine = strTotalLine[++i];

									if (strSourLine.Contains("}"))
									{
										listTempEnum.Remove(eData);
										eData = null;
									}
								}
							}
						}
					}
				}
			}

			File.Delete(strFileNameCopy);
			File.Delete(strFileNameSour);
			File.Move(strFileNameTemp, strFileNameSour);

			return listTempEnum.Count;
		}
	}
}

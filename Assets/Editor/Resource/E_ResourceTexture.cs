using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GGZ
{
	public class E_ResourceTexture : AssetPostprocessor
	{
		public enum ETextureType
		{
			BaseImage,
			Character,
			Effect,
			Object,
			Stage,
			Field,
			UI,
			MAX
		}

		public enum EImportSetting
		{
			// TextureImporter
			TextureCompression,
			M_TextureImporter,

			// TextureImporterSettings
			FilterMode,
			SpritePixelsPerUnit,
			SpriteMeshType,
			M_TextureImporterSettings,

			// PlatformTextureSettings
			MaxTextureSize,
			ResizeAlgorithm,
			CompressionQuality,
			M_PlatformTextureSettings,

			MAX
		}

		private delegate bool delImportSettingFunc(ref object obj);

		private static bool isInit = false;
		private static string[] arrPathResource;
		private static List<List<string>> listTexurePath;
		private static List<HashSet<string>> listHsIgnoreFileName;
		private static List<List<delImportSettingFunc>> listImportSettingFunc;		// Result : isDirty;

		private static Queue<ETextureType> qSearchKey;

		public void OnPostprocessTexture(Texture2D texture)
		{
			TextureImporter texImporter = (TextureImporter)assetImporter;
			string[] strPath = texImporter.assetPath.Split('/');

			// png 타입만 확인
			if (false == strPath.Back().EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
				return;

			E_ResourceTexture.Init();

			ETextureType eImportedType = E_ResourceTexture.CheckTextureImport(strPath);

			if (eImportedType != ETextureType.BaseImage)
			{
				E_ResourceTexture.OnTextureAdded(texImporter, eImportedType, strPath.Back());
			}
		}

		private static void Init()
		{
			if (true == E_ResourceTexture.isInit)
				return;

			isInit = true;

			arrPathResource = new string[] { "Assets", "Resources" };

			listTexurePath = new List<List<string>>();
			listTexurePath.Add(new List<string>(arrPathResource));

			List<string> listPathImage = listTexurePath.Back();
			listPathImage.Add("R_00_Image");

			listTexurePath.Add(new List<string>(listPathImage));
			listTexurePath.Back().Add("Char");

			listTexurePath.Add(new List<string>(listPathImage));
			listTexurePath.Back().Add("Effect");

			listTexurePath.Add(new List<string>(listPathImage));
			listTexurePath.Back().Add("Object");

			listTexurePath.Add(new List<string>(listPathImage));
			listTexurePath.Back().Add("Stage");

			listTexurePath.Add(new List<string>(listPathImage));
			listTexurePath.Back().Add("Field");

			listTexurePath.Add(new List<string>(listPathImage));
			listTexurePath.Back().Add("UI");

			InitIgnoreFileList();
			InitImportSetting();

			qSearchKey = new Queue<ETextureType>();
		}

		private static void InitIgnoreFileList()
		{
			listHsIgnoreFileName = new List<HashSet<string>>();
			listHsIgnoreFileName.Resize((int)ETextureType.MAX);

			listHsIgnoreFileName[(int)ETextureType.Stage].Add("Stage_001_Background.png");
			listHsIgnoreFileName[(int)ETextureType.Stage].Add("Stage_001_Field.png");
		}

		private static void InitImportSetting()
		{
			listImportSettingFunc = new List<List<delImportSettingFunc>>();
			listImportSettingFunc.Resize((int)ETextureType.MAX);

			for (int i = 0; i < listImportSettingFunc.Count; ++i)
			{
				listImportSettingFunc[i].Resize((int)EImportSetting.MAX, () => null);
			}

			var listImport = listImportSettingFunc[(int)ETextureType.BaseImage];

			listImport[(int)EImportSetting.TextureCompression] = (ref object obj) =>
			{
				TextureImporter ti = (TextureImporter)obj;

				if (ti.textureCompression != TextureImporterCompression.Uncompressed)
				{
					ti.textureCompression = TextureImporterCompression.Uncompressed;
					return true;
				}
				return false;
			};
			listImport[(int)EImportSetting.FilterMode] = (ref object obj) =>
			{
				TextureImporterSettings ti = (TextureImporterSettings)obj;

				if (ti.filterMode != FilterMode.Point)
				{
					ti.filterMode = FilterMode.Point;
					return true;
				}
				return false;
			};
			listImport[(int)EImportSetting.SpritePixelsPerUnit] = (ref object obj) =>
			{
				TextureImporterSettings ti = (TextureImporterSettings)obj;

				if (ti.spritePixelsPerUnit != 20)
				{
					ti.spritePixelsPerUnit = 20;
					return true;
				}
				return false;
			};
			listImport[(int)EImportSetting.SpriteMeshType] = (ref object obj) =>
			{
				TextureImporterSettings ti = (TextureImporterSettings)obj;

				if (ti.spriteMeshType != SpriteMeshType.FullRect)
				{
					ti.spriteMeshType = SpriteMeshType.FullRect;
					return true;
				}
				return false;
			};
			listImport[(int)EImportSetting.MaxTextureSize] = (ref object obj) =>
			{
				TextureImporterPlatformSettings ti = (TextureImporterPlatformSettings)obj;

				if (ti.maxTextureSize != 512)
				{
					ti.maxTextureSize = 512;
					return true;
				}
				return false;
			};
			listImport[(int)EImportSetting.ResizeAlgorithm] = (ref object obj) =>
			{
				TextureImporterPlatformSettings ti = (TextureImporterPlatformSettings)obj;
				
				if (ti.resizeAlgorithm != TextureResizeAlgorithm.Bilinear)
				{
					ti.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
					return true;
				}
				return false;
			};
			listImport[(int)EImportSetting.CompressionQuality] = (ref object obj) =>
			{
				TextureImporterPlatformSettings ti = (TextureImporterPlatformSettings)obj;

				if (ti.compressionQuality != 0)
				{
					ti.compressionQuality = 0;
					return true;
				}
				return false;
			};

			listImport = listImportSettingFunc[(int)ETextureType.Field];
			listImport[(int)EImportSetting.MaxTextureSize] = (ref object obj) =>
			{
				TextureImporterPlatformSettings ti = (TextureImporterPlatformSettings)obj;

				if (ti.maxTextureSize != 1024)
				{
					ti.maxTextureSize = 1024;
					return true;
				}
				return false;
			};
			listImport[(int)EImportSetting.SpritePixelsPerUnit] = (ref object obj) =>
			{
				TextureImporterSettings ti = (TextureImporterSettings)obj;

				if (ti.spritePixelsPerUnit != 20)
				{
					ti.spritePixelsPerUnit = 20;
					return true;
				}
				return false;
			};
		}

		private static ETextureType CheckTextureImport(string[] strPath)
		{
			qSearchKey.Clear();
			for (ETextureType iter = ETextureType.Character; iter < ETextureType.MAX; ++iter)
			{
				qSearchKey.Enqueue(iter);
			}

			ETextureType eLastCompareKey = ETextureType.BaseImage;
			int iPathCount = strPath.Length;
			iPathCount--;

			for (int iCompareIndex = 0; iCompareIndex < iPathCount && 0 < qSearchKey.Count; ++iCompareIndex)
			{
				bool isFirstSearch = true;
				ETextureType eFirstSearchKey = qSearchKey.Peek();

				bool bLoopComplete = false;
				while (false == bLoopComplete && 1 < qSearchKey.Count)
				{
					ETextureType eSearchKey = qSearchKey.Peek();

					if (eFirstSearchKey == eSearchKey)
					{
						if (true == isFirstSearch)
						{
							isFirstSearch = false;
						}
						else
						{
							bLoopComplete = true;
							continue;
						}
					}

					List<string> listRefSearchPath = listTexurePath[(int)eSearchKey];

					if (iCompareIndex < listRefSearchPath.Count)
					{
						string strCurrentPath = strPath[iCompareIndex];
						string strSearchPath = listRefSearchPath[iCompareIndex];

						if (0 == strCurrentPath.CompareTo(strSearchPath))
						{
							eLastCompareKey = eSearchKey;
							qSearchKey.Enqueue(qSearchKey.Dequeue());
						}
						else
						{
							qSearchKey.Dequeue();
						}
					}
					else
					{
						qSearchKey.Dequeue();
					}
				}
			}

			return eLastCompareKey;
		}

		private static bool OnTextureAdded(TextureImporter importer, ETextureType eType, string strFileName)
		{
			bool isDirty = false;

			if (listHsIgnoreFileName[(int)eType].Contains(strFileName))
			{
				return isDirty;
			}

			TextureImporterSettings tSetting = new TextureImporterSettings();
			importer.ReadTextureSettings(tSetting);

			var tPlatformSetting = importer.GetDefaultPlatformTextureSettings();

			var listImportBase = listImportSettingFunc[(int)ETextureType.BaseImage];
			var listImportTarget = listImportSettingFunc[(int)eType];

			for (int i = 0; i < listImportTarget.Count; ++i)
			{
				EImportSetting eSetting = (EImportSetting)i;
				delImportSettingFunc funcSetter = listImportTarget[(int)eSetting];

				if (funcSetter == null)
				{
					funcSetter = listImportBase[(int)eSetting];

					if (funcSetter == null)
					{
						continue;
					}
				}

				object objSetting = null;
				if (eSetting < EImportSetting.M_TextureImporter)
				{
					objSetting = importer;
				}
				else if (eSetting < EImportSetting.M_TextureImporterSettings)
				{
					objSetting = tSetting;
				}
				else if (eSetting < EImportSetting.M_PlatformTextureSettings)
				{
					objSetting = tPlatformSetting;
				}

				isDirty = funcSetter(ref objSetting) || isDirty;
			}	

			if (isDirty)
			{
				importer.SetTextureSettings(tSetting);
				importer.SetPlatformTextureSettings(tPlatformSetting);

				EditorUtility.SetDirty(importer);
				importer.SaveAndReimport();
			}

			return isDirty;
		}

		[MenuItem("Tools/Project P/Reimport All Textures")]
		public static void ReimportAllTextures()
		{
			E_ResourceTexture.Init();

			string strLog = "Reimport All Texture.\n";
			int iTotalCount = 0;

			long lProcessTime = GlobalUtility.Diagnostics.CheckTimeMS(() =>
			{
				for (ETextureType iter = ETextureType.Character; iter < ETextureType.MAX; ++iter)
				{
					long lSubProcessTime = GlobalUtility.Diagnostics.CheckTimeMS(() =>
					{
						// 각 디렉토리의 텍스쳐 에셋 추출
						string strPath = string.Join<string>("/", listTexurePath[(int)iter]);
						string[] strAssets = AssetDatabase.FindAssets("t:texture2D", new string[] { strPath });

						int iCount = strAssets.Length;
						iTotalCount += iCount;
						for (int i = 0; i < iCount; ++i)
						{
							TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(strAssets[i]));

							if (OnTextureAdded(importer, iter, importer.assetPath.Split('/').Back()))
							{
								EditorUtility.SetDirty(importer);
								importer.SaveAndReimport();
							}
						}
						strLog += $"{iter} : {strAssets.Length}";
					});
					strLog += $" / {lSubProcessTime}ms\n";
				}
			});

			strLog += $"ProcessTime : {iTotalCount} / {lProcessTime}ms\n";
			Debug.Log(strLog);
		}
	}
}

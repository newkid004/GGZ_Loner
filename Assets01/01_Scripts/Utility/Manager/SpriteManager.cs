using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Proto_00_N
{
	using GlobalUtility;

	public class SpriteManager : SingletonBase<SpriteManager>
	{
		[System.Serializable]
		public struct stInputTextureData
		{
			public string strSpriteType;	   // TexureManager.Container.EType 참조
			public List<Sprite> listSprite;
		}

		private int dgLoadedSpriteType = 0;
		private List<Container> listContainer = new List<Container>();
		private Dictionary<Sprite, int> dictRefCount = new Dictionary<Sprite, int>();

		public class Container
		{
			public class SpriteItem
			{
				public string strPathSprite { get; private set; }
				public Sprite sprite { get; private set; }

				public void Set(string strPath, out ResourceRequest resReqResult)
				{
					if (sprite == null)
					{
						strPathSprite = strPath;

						ResourceRequest resReq = Resources.LoadAsync(strPath);
						resReq.completed += (resOper) => sprite = (Sprite)resReq.asset;

						resReqResult = resReq;
						ChangeRefCount(1);
					}
					else
					{
						resReqResult = null;
#if _debug
						Debug.LogAssertion("SpriteManager.Container.SpriteItem.Set\n" +
							"Already Set Sprite");
#endif
					}
				}

				public void Set(Sprite sprite)
				{
					if (this.sprite == null)
					{
						string strTotalPath = AssetDatabase.GetAssetPath(sprite);
						this.strPathSprite = strTotalPath.Substring(17, strTotalPath.Length - 17 - 4); // "Assets/Resources/", ".png" 제거
						this.sprite = sprite;
						ChangeRefCount(1);
					}
					else
					{
#if _debug
						Debug.LogAssertion("SpriteManager.Container.SpriteItem.Set\n" +
							"Already Set Sprite");
#endif
					}
				}

				public void Apply(Image image)
				{
					if (sprite != null)
					{
						image.sprite = this.sprite;
					}
					else
					{
						Set(this.strPathSprite, out ResourceRequest resReq);
						resReq.completed += (resOper) => image.sprite = (Sprite)resReq.asset;
					}
				}

				public void Apply(RawImage image)
				{
					if (sprite != null)
					{
						image.texture = this.sprite.texture;
					}
					else
					{
						Set(this.strPathSprite, out ResourceRequest resReq);
						resReq.completed += (resOper) => image.texture = ((Sprite)resReq.asset).texture;
					}
				}

				public void Load()
				{
					if (sprite == null)
					{
						Set(strPathSprite, out ResourceRequest resReq);
					}
					else
					{
#if _debug
						Debug.LogAssertion("SpriteManager.Container.SpriteItem.Set\n" +
							"Sprite is already Load");
#endif
					}
				}

				public void Unload()
				{

					if (sprite != null)
					{
						ChangeRefCount(-1);
						sprite = null;
					}
					else
					{
#if _debug
						Debug.LogAssertion("SpriteManager.Container.SpriteItem.Set\n" +
							"Sprite is already null");
#endif
					}
				}

				private void ChangeRefCount(int iCount)
				{
					if (sprite == null)
						return;

					if (0 < iCount)
					{
						Single.dictRefCount.ActSafe(sprite, (bInsert, iRefCount) => ++Single.dictRefCount[sprite]);
					}
					else
					{
						Single.dictRefCount.ActSafe(sprite, (bInsert, iRefCount) =>
						{
							if (iRefCount == 1)
							{
								Single.dictRefCount.Remove(sprite);
								Resources.UnloadAsset(sprite);
							}
						});
					}
				}
			}

			public enum EType
			{
				CharIdle,
				CharIcon,
				CharAnimation,

				Equipment,

				Effect,

				Background_Loading,
				Background_Main,
				Background_Battle,

				UI,

				MAX
			}

			public bool isLoaded { get; private set; }
			public EType eType { get; private set; }
			public Dictionary<string, SpriteItem> dictTexture { get; private set; } = new Dictionary<string, SpriteItem>();

			public void Init(EType eType, List<Sprite> listTexture)
			{
				this.eType = eType;

				listTexture.ForEach(sprite =>
				{
					SpriteItem item = new SpriteItem();
					item.Set(sprite);
					dictTexture.SetSafe(sprite.name, item);
				});

				isLoaded = true;
			}

			public void Load()
			{
				if (isLoaded)
					return;

				dictTexture.ForEach((strName, item) => item.Load());
				isLoaded = true;
			}

			public void Unload()
			{
				if (isLoaded == false)
					return;

				dictTexture.ForEach((strName, item) => item.Unload());

				isLoaded = false;
			}
		}

		protected override void Init()
		{
			base.Init();

			listContainer.Resize((int)Container.EType.MAX);
		}

		public void Init(List<stInputTextureData> listInput)
		{
			listInput.ForEach(InputData =>
			{
				if (System.Enum.TryParse(InputData.strSpriteType, out Container.EType eType))
				{
					Container cont = listContainer[(int)eType];
					cont.Init(eType, InputData.listSprite);
				}
#if _debug
				else
				{
					Debug.LogAssertion("SpriteManager.Init\n" +
						$"Invalid Type string : {InputData.strSpriteType}");
				}
#endif
			});
		}

		public void ApplyLoadState(int dgLoadType, int dgUnloadType)
		{
			int dgRealLoad = Digit.PICK(dgLoadType, dgLoadedSpriteType);
			int dgRealUnload = Digit.AND(dgLoadedSpriteType, dgUnloadType);

			// Unload 할 텍스쳐는 Load 대상 제외
			dgRealLoad = Digit.PICK(dgRealLoad, dgRealUnload);
		}

		public Sprite Get(Container.EType eType, string strSpriteName) => listContainer[(int)eType].dictTexture.GetDef(strSpriteName)?.sprite;
		public void Apply(Image img, Container.EType eType, string strSpriteName) => listContainer[(int)eType].dictTexture.GetDef(strSpriteName)?.Apply(img);
		public void Apply(RawImage img, Container.EType eType, string strSpriteName) => listContainer[(int)eType].dictTexture.GetDef(strSpriteName)?.Apply(img);
	}
}
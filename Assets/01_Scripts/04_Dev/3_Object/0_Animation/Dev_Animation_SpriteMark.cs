using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GGZ
{
	public class Dev_Animation_SpriteMark : PooledObject, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		[HideInInspector] public Image img;
		public int iSpriteIndex;

		public Image imgSimple;
		public Image imgText;
		public Text txtMark;

		public AnimationModule.Data.Pair aniDataPair;

		private static Dev_Animation_SpriteMark tempDragMark;

		static Dev_PageAnimation aniPage;
		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			if (null == aniPage)
				aniPage = Dev_PageBase.Get<Dev_PageAnimation>();

			if (aniPage.adSelection.listPair.Back() == aniDataPair)
				return;

			tempDragMark = aniPage.opSpriteMark.Pop();
			tempDragMark.aniDataPair = aniDataPair;
			tempDragMark.InitMark();

			var clrMark = clrNormal;
			clrMark.a = 0.5f;
			tempDragMark.img.color = clrMark;
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			if (aniPage.adSelection.listPair.Back() == aniDataPair)
				return;

			if (null == tempDragMark)
				return;

			tempDragMark.transform.position = aniPage.camUI.ScreenToWorldPoint(eventData.position);

			var rt = (RectTransform)tempDragMark.transform;
			rt.anchoredPosition3D = new Vector3(rt.anchoredPosition.x.Between(0, aniPage.rtPlaySliderBody.rect.width), 0, 0);

			if (Dev_PageAnimation.AniOptionBool.Main.isMarkVisibleText)
			{
				float fLerp = ((RectTransform)tempDragMark.transform).anchoredPosition.x / aniPage.rtPlaySliderBody.rect.width;
				float fInsertSectionTime = aniPage.adSelection.TotalLength * fLerp;

				if (Dev_PageAnimation.AniOptionBool.Main.isMarkControlAbsolute)
				{
					tempDragMark.txtMark.text = fInsertSectionTime.ToString();
				}
				else
				{
					float fInterval = fInsertSectionTime - aniDataPair.accumulate;
					tempDragMark.txtMark.text = (aniDataPair.timeLength + fInterval).ToString();
				}
			}
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
			if (aniPage.adSelection.listPair.Back() == aniDataPair)
				return;

			AnimationModule.Data aniData = aniPage.adSelection;

			float fLerp = ((RectTransform)tempDragMark.transform).anchoredPosition.x / aniPage.rtPlaySliderBody.rect.width;
			float fInsertSectionTime = aniData.TotalLength * fLerp;

			aniData.ModifySpriteSection(iSpriteIndex, fInsertSectionTime);

			// Refresh
			bool isPlayed = aniPage.aniModule.isPlay;
			float fPlaySection = aniPage.aniModule.fPlaySection;

			aniPage.iAniSelectionItemIndex = (int)Dev_PageAnimation.AniSelectionItem.EType.AniName;
			aniPage.OnChangeAniSelectionItem(aniPage.listAniSelectionItem[aniPage.iAniSelectionItemIndex].iSelectionIndex);

			if (isPlayed)
			{
				aniPage.OnClickPlayButton();
			}
			aniPage.aniModule.fPlaySection = fPlaySection;

			var clrMark = tempDragMark.img.color;
			clrMark.a = 1f;
			tempDragMark.img.color = clrMark;

			tempDragMark.Push();
		}

		public static readonly Color clrNormal = Color.yellow;
		public static readonly Color clrSelect = Color.green;
		public static readonly Color clrDisable = Color.gray;
		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (null == aniPage)
				aniPage = Dev_PageBase.Get<Dev_PageAnimation>();

			switch (eventData.button)
			{
				case PointerEventData.InputButton.Left:
				{
					if (-1 == iRoutineIndex)
					{
						OnSingleClick();
					}
					else
					{
						OnDoubleClick();
					}
					break;
				}
			}
		}

		private int iRoutineIndex = -1;
		void OnSingleClick()
		{
			iRoutineIndex = CustomRoutine.CallLate(Dev_PageAnimation.AniOptionFloat.Main.fMarkDoubleClickInterval, () =>
			{
				iRoutineIndex = -1;
			});

			if (aniPage.slistSelectMarkIndex.ContainsKey(iSpriteIndex))
			{
				RemoveSelection();
			}
			else
			{
				InsertSelection();
			}
		}

		void OnDoubleClick()
		{
			if (false == CustomRoutine.Stop(iRoutineIndex))
			{
#if _debug
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				Debug.LogError(strError + $"Invalid CustomRoutine Index ({iRoutineIndex})");
#endif
				return;
			}
			iRoutineIndex = -1;

			if (aniPage.slistSelectMarkIndex.ContainsKey(iSpriteIndex))
			{
				RemoveSelection();
			}

			Dev_PopupTextSubmit.Info initInfo = new Dev_PopupTextSubmit.Info();
			initInfo.strTitle = "항목 수정 : 애니메이션 구간";

			if (Dev_PageAnimation.AniOptionBool.Main.isMarkControlAbsolute)
			{
				initInfo.strDescription = "변경될 구간을 입력해주세요";
				initInfo.strDefaultText = aniDataPair.accumulate.ToString();
			}
			else
			{
				initInfo.strDescription = "변경될 길이를 입력해주세요";
				initInfo.strDefaultText = aniDataPair.timeLength.ToString();
			}

			initInfo.actSubmit = str =>
			{
				if (false == float.TryParse(str, out float fValue))
					return;

				if (Dev_PageAnimation.AniOptionBool.Main.isMarkControlAbsolute)
				{
					aniPage.adSelection.ModifySpriteSection(iSpriteIndex, fValue);
				}
				else
				{
					float fInterval = fValue - aniDataPair.timeLength;
					aniPage.adSelection.ModifySpriteSection(iSpriteIndex, aniDataPair.accumulate + fInterval);
				}

				aniPage.iAniSelectionItemIndex = (int)Dev_PageAnimation.AniSelectionItem.EType.AniName;
				aniPage.OnChangeAniSelectionItem(aniPage.listAniSelectionItem[aniPage.iAniSelectionItemIndex].iSelectionIndex);
			};

			Dev_PopupBase.Get<Dev_PopupTextSubmit>().Init(initInfo).Open();
		}

		void InsertSelection()
		{
			aniPage.slistSelectMarkIndex.Add(iSpriteIndex, iSpriteIndex);
			img.color = clrSelect;

			string strSpriteName = aniPage.adSelection.listPair[iSpriteIndex].sprite.name;
			aniPage.ifAniSelectionSprite.SetTextWithoutNotify(strSpriteName);

			if (System.Enum.TryParse(strSpriteName, out SpriteDefine.Animation eAniSprite))
			{
				aniPage.imgAniSelectionPlayBar.sprite = SpriteManager.Single.Get(SpriteManager.Container.EType.Animation, (int)eAniSprite);
			}
		}

		void RemoveSelection()
		{
			aniPage.slistSelectMarkIndex.Remove(iSpriteIndex);
			img.color = clrNormal;

			aniPage.ifAniSelectionSprite.text = string.Empty;
		}

		public override void OnPushedToPool()
		{
			RectTransform rt = (RectTransform)transform;
			rt.anchoredPosition3D = Vector3.zero;
			rt.anchorMin = rt.anchorMax = new Vector2(0.0f, 0.5f);

			base.OnPushedToPool();
		}

		private float fDefaultWidth = 10;
		public void InitMark()
		{
			imgSimple.gameObject.SetActive(false);
			imgText.gameObject.SetActive(false);

			RectTransform rt = transform as RectTransform;

			if (Dev_PageAnimation.AniOptionBool.Main.isMarkVisibleText)
			{
				img = imgText;

				if (Dev_PageAnimation.AniOptionBool.Main.isMarkControlAbsolute)
				{
					txtMark.text = aniDataPair.accumulate.ToString();
				}
				else
				{
					txtMark.text = aniDataPair.timeLength.ToString();
				}

				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Dev_PageAnimation.AniOptionFloat.Main.fMarkTextSize);
			}
			else
			{
				img = imgSimple;

				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fDefaultWidth);
			}

			img.gameObject.SetActive(true);
		}
	}
}
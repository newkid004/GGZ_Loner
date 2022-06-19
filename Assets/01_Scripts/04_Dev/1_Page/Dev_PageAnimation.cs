using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Dev_PageAnimation : Dev_PageBase
	{
		[System.Serializable]
		public class AniPairData
		{
			public Sprite sprite;
			public float timeLength;
		}

		[System.Serializable]
		public class AniPairList
		{
			public List<AniPairData> list = new List<AniPairData>();
		}

		[System.Serializable]
		public class AniSelectionItem
		{
			public enum EType
			{
				None,

				AniType,
				UnitID,
				AniName,
				AniData,

				MAX
			}

			public Dropdown dropdown;
			public int iSelectionIndex = 0;

			[SerializeField] public List<Selectable> listLinkedSelection = new List<Selectable>();

			public string strSelection => dropdown.options[iSelectionIndex].text;
		}

		// Selection
		[HideInInspector] public AnimationModule.Group agSelection;
		[HideInInspector] public AnimationModule.Data adSelection;

		[Header("----- Option -----")]
		public float fAniSpeed = 1.0f;

		[Header("----- Interact -----")]
		public string strAniName = null;
		[SerializeField] List<AniPairList> listSelectData = new List<AniPairList>();

		[Header("----- Dev Ref -----")]
		[SerializeField] RectTransform rtPopupRoot;

		protected override void Start()
		{
			base.Start();

			opSpriteMark.Init();
			opSpriteSelectionItem.Init();

			iAniSelectionItemIndex = 0;
			OnChangeAniSelectionItem(0);

			InitAniOption();

			OnChangeSelectionModifyInputBox(null);

			rtPopupRoot.anchoredPosition3D = Vector3.zero;
			Dev_PopupBase.CloseAll();
		}

		private void Update()
		{
			UpdatePlayControl();
		}

		#region Func : AniSetType
		[Header("// ----- For Dev ----- //")]
		[Header("Func : AniSetType")]
		[SerializeField] public List<AniSelectionItem> listAniSelectionItem;
		public int iAniSelectionItemIndex { get; set; }

		public void OnClickAddTypeItem(int iAniAddTypeSelectionIndex)
		{
			bool isPopup = true;
			Dev_PopupTextSubmit.Info initInfo = new Dev_PopupTextSubmit.Info();

			initInfo.strTitle = "항목 추가 : 애니메이션";

			switch ((AniSelectionItem.EType)iAniAddTypeSelectionIndex)
			{
				case AniSelectionItem.EType.UnitID:
				{
					initInfo.strDescription = "추가될 애니메이션 유닛 ID를 입력해주세요.\n(숫자)";
					initInfo.actSubmit = (str) =>
					{
						bool isErrorAlert = true;
						Dev_PopupAlert.Info initInfoAlert = new Dev_PopupAlert.Info();
						initInfoAlert.strTitle = "오류 - 항목 추가 : 애니메이션";

						if (int.TryParse(str, out int iUnitID))
						{
							string strUnitName = dictSelectionUnit.GetDef(iUnitID);
							if (string.IsNullOrEmpty(strUnitName))
							{
								AniSelectionItem itemType = listAniSelectionItem[(int)AniSelectionItem.EType.AniType];
								AnimationManager.Single.AddGroup((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID);

								itemType.iSelectionIndex = AnimationManager.Single.GetGroupCount((AnimationManager.EAniType)itemType.iSelectionIndex) - 1;

								iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniType;
								OnChangeAniSelectionItem(-1);
								isErrorAlert = false;
							}
							else
							{
								initInfoAlert.strDescription = $"이미 존재하는 ID 입니다.\n{iUnitID}, {strUnitName}";
							}
						}
						else
						{
							initInfoAlert.strDescription = "올바르지 않은 ID 입니다.\n" + str;
						}

						if (isErrorAlert)
						{
							Dev_PopupBase.Get<Dev_PopupAlert>().Init(initInfoAlert).Open();
						}
					};
				}
				break;

				case AniSelectionItem.EType.AniName:
				{
					initInfo.strDescription = "추가될 명칭을 입력해주세요.\n";
					initInfo.actSubmit = (str) =>
					{
						bool isErrorAlert = true;

						AniSelectionItem itemType = listAniSelectionItem[(int)AniSelectionItem.EType.AniType];
						AniSelectionItem itemUnit = listAniSelectionItem[(int)AniSelectionItem.EType.UnitID];
						AniSelectionItem itemName = listAniSelectionItem[(int)AniSelectionItem.EType.AniName];

						int iUnitID = int.Parse(itemUnit.dropdown.options[itemUnit.iSelectionIndex].text.Split(',')[0]);

						var aniGroupItem = AnimationManager.Single.GetGroupItem((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID, str);
						if (null == aniGroupItem)
						{
							var addition = AnimationManager.Single.AddGroupItem((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID, str);

							itemName.iSelectionIndex = AnimationManager.Single.GetGroupItemCount((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID) - 1;

							iAniSelectionItemIndex = (int)AniSelectionItem.EType.UnitID;
							OnChangeAniSelectionItem(-1);
							isErrorAlert = false;
						}

						if (isErrorAlert)
						{
							Dev_PopupAlert.Info initInfoAlert = new Dev_PopupAlert.Info();
							initInfoAlert.strTitle = "오류 - 항목 추가 : 애니메이션";
							initInfoAlert.strDescription = $"이미 존재하는 명칭입니다.\n{str}";
							Dev_PopupBase.Get<Dev_PopupAlert>().Init(initInfoAlert).Open();
						}
					};
				}
				break;

				case AniSelectionItem.EType.AniData:
				{
					isPopup = false; // pushback

					AniSelectionItem itemName = listAniSelectionItem[(int)AniSelectionItem.EType.AniName];
					AniSelectionItem itemData = listAniSelectionItem[(int)AniSelectionItem.EType.AniData];

					AnimationModule.Data aniData = new AnimationModule.Data();
					aniData.iID = AnimationManager.Single.GetNextAniDataSequence();

					aniData.AddSprite(SpriteManager.Single.Get(0, 0), 1);
					agSelection.AddData(aniData);

					itemData.iSelectionIndex = agSelection.listData.Count - 1;

					iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
					OnChangeAniSelectionItem(-1);
				}
				break;
			}

			if (isPopup)
			{
				Dev_PopupBase.Get<Dev_PopupTextSubmit>().Init(initInfo).Open();
			}
		}

		public void OnClickDelTypeItem(int iAniDelTypeSelectionIndex)
		{
			Dev_PopupYorNSubmit.Info initInfo = new Dev_PopupYorNSubmit.Info();

			initInfo.strTitle = "항목 제거 : 애니메이션";
			initInfo.strDescription = string.Empty;

			AniSelectionItem itemType = listAniSelectionItem[(int)AniSelectionItem.EType.AniType];
			AniSelectionItem itemUnit = listAniSelectionItem[(int)AniSelectionItem.EType.UnitID];
			AniSelectionItem itemName = listAniSelectionItem[(int)AniSelectionItem.EType.AniName];
			AniSelectionItem itemData = listAniSelectionItem[(int)AniSelectionItem.EType.AniData];

			var dictSelectionType = AnimationManager.Single.listModule[itemType.iSelectionIndex];

			switch ((AniSelectionItem.EType)iAniDelTypeSelectionIndex)
			{
				case AniSelectionItem.EType.UnitID:
				{
					initInfo.strDescription += "하위 항목도 모두 제거됩니다.\n계속 진행할까요?";
					initInfo.actYes = () =>
					{
						dictSelectionType.Remove(itemUnit.iSelectionIndex);

						iAniSelectionItemIndex = (int)AniSelectionItem.EType.UnitID - 1;
						OnChangeAniSelectionItem(-1);
					};
				}
				break;

				case AniSelectionItem.EType.AniName:
				{
					initInfo.strDescription += "하위 항목도 모두 제거됩니다.\n계속 진행할까요?";
					initInfo.actYes = () =>
					{
						int iUnitID = int.Parse(itemUnit.strSelection.Split(',')[0]);

						string str = itemName.strSelection;
						dictSelectionType[iUnitID].Remove(str);

						iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName - 1;
						OnChangeAniSelectionItem(-1);
					};
				}
				break;

				case AniSelectionItem.EType.AniData:
				{
					// 즉시 실행
					int iUnitID = int.Parse(itemUnit.strSelection.Split(',')[0]);

					string str = itemName.strSelection;
					dictSelectionType[iUnitID][str].DelData(itemData.iSelectionIndex);

					iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniData - 1;
					OnChangeAniSelectionItem(-1);
					return;
				}
			}

			Dev_PopupBase.Get<Dev_PopupYorNSubmit>().Init(initInfo).Open();
		}

		public void OnClickChgTypeItem(int iAniChgTypeSelectionIndex)
		{
			Dev_PopupTextSubmit.Info initInfo = new Dev_PopupTextSubmit.Info();

			initInfo.strTitle = "항목 수정 : 애니메이션";

			AniSelectionItem itemType = listAniSelectionItem[(int)AniSelectionItem.EType.AniType];
			AniSelectionItem itemUnit = listAniSelectionItem[(int)AniSelectionItem.EType.UnitID];
			AniSelectionItem itemName = listAniSelectionItem[(int)AniSelectionItem.EType.AniName];

			var dictSelectionType = AnimationManager.Single.listModule[itemType.iSelectionIndex];

			switch ((AniSelectionItem.EType)iAniChgTypeSelectionIndex)
			{
				case AniSelectionItem.EType.UnitID:
				{
					int iUnitIDOld = int.Parse(itemUnit.strSelection.Split(',')[0]);

					initInfo.strDescription = "수정될 애니메이션 유닛 ID를 입력해주세요.\n(숫자)";
					initInfo.actSubmit = (str) =>
					{
						bool isErrorAlert = true;
						Dev_PopupAlert.Info initInfoAlert = new Dev_PopupAlert.Info();

						if (int.TryParse(str, out int iUnitID))
						{
							var aniGroup = dictSelectionType.GetDef(iUnitID);
							if (null == aniGroup)
							{
								var aniGroupOld = dictSelectionType.GetDef(iUnitIDOld);

								AnimationManager.Single.DelGroup((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitIDOld);
								AnimationManager.Single.AddGroup((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID, aniGroupOld);

								iAniSelectionItemIndex = (int)AniSelectionItem.EType.UnitID - 1;
								OnChangeAniSelectionItem(-1);
								isErrorAlert = false;
							}
							else
							{
								initInfoAlert.strDescription = $"이미 존재하는 ID 입니다.\n{iUnitID}, {itemUnit.strSelection}";
							}
						}
						else
						{
							initInfoAlert.strDescription = "올바르지 않은 ID 입니다.\n" + str;
						}

						if (isErrorAlert)
						{
							Dev_PopupBase.Get<Dev_PopupAlert>().Init(initInfoAlert).Open();
						}
					};
				}
				break;

				case AniSelectionItem.EType.AniName:
				{
					initInfo.strDescription = "수정될 명칭을 입력해주세요.\n";
					initInfo.actSubmit = (str) =>
					{
						bool isErrorAlert = true;

						int iUnitID = int.Parse(itemUnit.strSelection.Split(',')[0]);

						var aniGroupItem = AnimationManager.Single.GetGroupItem((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID, str);
						if (null == aniGroupItem)
						{
							var aniGroupItemOld = AnimationManager.Single.GetGroupItem((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID, itemName.strSelection);

							AnimationManager.Single.DelGroupItem((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID, itemName.strSelection);
							AnimationManager.Single.AddGroupItem((AnimationManager.EAniType)itemType.iSelectionIndex, iUnitID, str, aniGroupItemOld);

							iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName - 1;
							OnChangeAniSelectionItem(-1);
							isErrorAlert = false;
						}

						if (isErrorAlert)
						{
							Dev_PopupAlert.Info initInfoAlert = new Dev_PopupAlert.Info();
							initInfoAlert.strTitle = "오류 - 항목 추가 : 애니메이션";
							initInfoAlert.strDescription = $"이미 존재하는 명칭입니다.\n{str}";
							Dev_PopupBase.Get<Dev_PopupAlert>().Init(initInfoAlert).Open();
						}
					};
				}
				break;
			}

			Dev_PopupBase.Get<Dev_PopupTextSubmit>().Init(initInfo).Open();
		}

		public void OnChangeAniSelectionItem(int iDropdownItemIndex)
		{
			// 메뉴 비활성화 ( 선택한 항목은 비활성화 X )
			for (int i = iAniSelectionItemIndex + 1; i < (int)AniSelectionItem.EType.MAX; ++i)
			{
				AniSelectionItem item = listAniSelectionItem[i];

				if (item.dropdown)
				{
					item.dropdown.ClearOptions();
					item.dropdown.interactable = false;
					item.listLinkedSelection.ForEach(item => item.interactable = false);
				}
			}

			// 각 드롭다운에서 선택한 항목 Index 저장
			if (-1 < iDropdownItemIndex)
			{
				listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex = iDropdownItemIndex;
			}

			// Local function : Dropdown 최신화
			void RefreshDropdown(AniSelectionItem.EType eType, List<string> listOption)
			{
				AniSelectionItem item = listAniSelectionItem[(int)eType];

				item.dropdown.AddOptions(listOption);
				item.dropdown.interactable = 0 < item.dropdown.options.Count;
				item.listLinkedSelection.ForEach(sel => sel.interactable = item.dropdown.interactable);
			}

			// Local function : Dropdown 옵션 검증
			bool CheckItemType(AniSelectionItem.EType eType)
			{
				return iAniSelectionItemIndex <= (int)eType && listAniSelectionItem[(int)eType].dropdown.interactable;
			}

			// 타입별 항목 초기화, 작성
			if (iAniSelectionItemIndex <= (int)AniSelectionItem.EType.None)
			{
				List<string> listOption = new List<string>();

				AnimationManager.Single.listModule.ForEach((iType, dict) =>
					listOption.Add(System.Enum.Parse(typeof(AnimationManager.EAniType), iType.ToString()).ToString()));

				RefreshDropdown(AniSelectionItem.EType.AniType, listOption);
			}

			if (CheckItemType(AniSelectionItem.EType.AniType))
			{
				AniSelectionItem itemType = listAniSelectionItem[(int)AniSelectionItem.EType.AniType];
				dictSelectionUnit.Clear();

				List<string> listOption = new List<string>();

				switch (itemType.iSelectionIndex)
				{
					case 0:
					case 2:
					AnimationManager.Single.listModule[itemType.iSelectionIndex].ForEach((iUnitID, dict) =>
					{
						string strName = CSVData.Battle.Status.Unit.Manager.Get(iUnitID).Name;
						dictSelectionUnit.Add(iUnitID, strName);
						listOption.Add($"{iUnitID},{strName}");
					});;
					break;

					case 1:
					AnimationManager.Single.listModule[itemType.iSelectionIndex].ForEach((iUnitID, dict) =>
					{
						string strName = CSVData.Battle.Status.Boss.Manager.Get(iUnitID).Name;
						dictSelectionUnit.Add(iUnitID, strName);
						listOption.Add($"{iUnitID},{strName}");
					});
					break;
				}

				RefreshDropdown(AniSelectionItem.EType.UnitID, listOption);
			}

			if (CheckItemType(AniSelectionItem.EType.UnitID))
			{
				AniSelectionItem itemType = listAniSelectionItem[(int)AniSelectionItem.EType.AniType];
				AniSelectionItem itemUnit = listAniSelectionItem[(int)AniSelectionItem.EType.UnitID];

				int iUnitID = int.Parse(itemUnit.dropdown.options[itemUnit.iSelectionIndex].text.Split(',')[0]);

				List<string> listOption = new List<string>();

				AnimationManager.Single.listModule[itemType.iSelectionIndex][iUnitID].ForEach((strGroupName, aniGroup) => listOption.Add(strGroupName));

				RefreshDropdown(AniSelectionItem.EType.AniName, listOption);
			}

			if (CheckItemType(AniSelectionItem.EType.AniName))
			{
				AniSelectionItem itemType = listAniSelectionItem[(int)AniSelectionItem.EType.AniType];
				AniSelectionItem itemUnit = listAniSelectionItem[(int)AniSelectionItem.EType.UnitID];
				AniSelectionItem itemName = listAniSelectionItem[(int)AniSelectionItem.EType.AniName];

				itemName.iSelectionIndex = Mathf.Min(itemName.dropdown.options.Count - 1, itemName.iSelectionIndex);
				itemName.dropdown.SetValueWithoutNotify(itemName.iSelectionIndex);
				
				int iUnitID = int.Parse(itemUnit.dropdown.options[itemUnit.iSelectionIndex].text.Split(',')[0]);

				strAniName = itemName.dropdown.options[itemName.iSelectionIndex].text;

				agSelection = AnimationManager.Single.listModule[itemType.iSelectionIndex][iUnitID][strAniName];

				listSelectData.Clear();
				agSelection.listData.ForEach(data =>
				{
					AniPairList listPair = new AniPairList();

					data.listPair.ForEach(pair => listPair.list.Add(new AniPairData() { sprite = pair.sprite, timeLength = pair.timeLength }));

					listSelectData.Add(listPair);
				});

				aniModule.SetGroup(agSelection);

				AniSelectionItem itemData = listAniSelectionItem[(int)AniSelectionItem.EType.AniData];

				int iSequence = 0;
				itemData.dropdown.AddOptions(agSelection.listData.ConvertAll(data => (++iSequence).ToString()));
				itemData.dropdown.interactable = 0 < itemData.dropdown.options.Count;
			}

			if (CheckItemType(AniSelectionItem.EType.AniData))
			{
				AniSelectionItem itemData = listAniSelectionItem[(int)AniSelectionItem.EType.AniData];

				itemData.iSelectionIndex = Mathf.Min(itemData.dropdown.options.Count - 1, itemData.iSelectionIndex);
				itemData.dropdown.SetValueWithoutNotify(itemData.iSelectionIndex);

				adSelection = agSelection.listData[itemData.iSelectionIndex];

				((Text)ifAniLengthFunction.placeholder).text = adSelection.TotalLength.ToString();
				slistSelectMarkIndex.Clear();

				aniModule.Play(itemData.iSelectionIndex);
				OnClickStopButton();

				SpreadAniDataMark();
			}
		}

		#endregion

		#region Func : AniPlayControl
		[Header("Func : AniPlayControl")]
		public Camera camUI;
		public AnimationModule aniModule;
		[SerializeField] Text txtAniPlay;
		[SerializeField] Toggle tglAniRepeat;
		[SerializeField] Slider sldAniSection;

		private Dictionary<int, string> dictSelectionUnit = new Dictionary<int, string>();

		[SerializeField] public ObjectPool<Dev_Animation_SpriteMark> opSpriteMark = new ObjectPool<Dev_Animation_SpriteMark>();
		[SerializeField] public RectTransform rtPlaySliderBody;
		[HideInInspector] public SortedList<int, int> slistSelectMarkIndex = new SortedList<int, int>();
		private void UpdatePlayControl()
		{
			if (aniModule.isPlay)
			{
				sldAniSection.SetValueWithoutNotify(aniModule.fPlaySection);
			}

			if (0 < slistSelectMarkIndex.Count)
			{
				int iMoveDirection = 0;

				if (Input.GetKeyDown(KeyCode.LeftArrow))
				{
					iMoveDirection = -1;
				}
				else if (Input.GetKeyDown(KeyCode.RightArrow))
				{
					iMoveDirection = 1;
				}

				if (iMoveDirection != 0)
				{
					List<AnimationModule.Data.Pair> pairList = new List<AnimationModule.Data.Pair>();
					slistSelectMarkIndex.ForEach((i, i2) => pairList.Add(adSelection.listPair[i]));

					float fInterval = AniOptionFloat.Main.fMarkMoveInterval * iMoveDirection;
					pairList.ForEach(pair =>
					{
						adSelection.ModifySpriteSection(
							adSelection.listPair.BinarySearch(pair), 
							pair.accumulate + fInterval);
					});

					iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
					OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);

					pairList.ForEach(pair =>
					{
						int i = adSelection.listPair.BinarySearch(pair);
						slistSelectMarkIndex.Add(i, i);
					});

					SpreadAniDataMark();
				}
			}
		}

		public void SpreadAniDataMark()
		{
			var listPair = adSelection.listPair;

			float fParentLength = rtPlaySliderBody.rect.width;

			int i = 0;

			opSpriteMark.CollectAllObject();
			listPair.ForEach(pair =>
			{
				var markItem = opSpriteMark.Pop();

				markItem.iSpriteIndex = i;
				markItem.aniDataPair = pair;

				float fAniLerp = pair.accumulate / adSelection.TotalLength;

				RectTransform rt = (RectTransform)markItem.transform;
				rt.anchorMin = rt.anchorMax = new Vector2(fAniLerp, 0.5f);

				markItem.InitMark();

				if (slistSelectMarkIndex.ContainsKey(i))
				{
					markItem.img.color = Dev_Animation_SpriteMark.clrSelect;
				}
				else
				{
					markItem.img.color = i != (listPair.Count - 1) ?
						Dev_Animation_SpriteMark.clrNormal :
						Dev_Animation_SpriteMark.clrDisable;
				}

				++i;
			});
		}

		public void OnClickPlayButton()
		{
			aniModule.fTimeSpeed = fAniSpeed;

			if (aniModule.isPlay)
			{
				aniModule.Pause();
				txtAniPlay.text = "▶";
			}
			else
			{
				if (0 <= aniModule.iCurrentSpriteIndex)
				{
					aniModule.Resume();
				}
				else
				{
					aniModule.Play(listAniSelectionItem[(int)AniSelectionItem.EType.AniData].iSelectionIndex);
				}

				txtAniPlay.text = "||";
			}
		}

		public void OnClickStopButton()
		{
			aniModule.Stop();
			sldAniSection.value = 0;

			txtAniPlay.text = "▶";
		}

		public void OnComplateAnimation(bool isRepeat)
		{
			if (tglAniRepeat.isOn)
			{
				if (false == isRepeat)
				{
					aniModule.Replay();
				}
			}
			else
			{
				OnClickStopButton();
			}
		}
		#endregion

		#region Func : AniOption

		[System.Serializable]
		public class AniOptionBool : SaveSingleObjectBase<AniOptionBool>
		{
			public override string strPathSave => $"Assets/Editor/Animation/Option";

			public enum EValue
			{
				MarkVisibleText,
				MarkControlAbsolute,

				MAX
			}

			public const bool default_isMarkVisibleText = true;
			public bool isMarkVisibleText = default_isMarkVisibleText;

			public const bool default_isMarkControlAbsolute = true;
			public bool isMarkControlAbsolute = default_isMarkControlAbsolute;

			public static bool GetDefault(int iValueEnum)
			{
				switch ((EValue)iValueEnum)
				{
					case EValue.MarkVisibleText:			return default_isMarkVisibleText;
					case EValue.MarkControlAbsolute:		return default_isMarkControlAbsolute;
					default: return false;
				}
			}

			public void SetDefault(int iValueEnum) => SetValue(iValueEnum, GetDefault(iValueEnum));

			public bool GetValue(int iValueEnum)
			{
				switch ((EValue)iValueEnum)
				{
					case EValue.MarkVisibleText:			return isMarkVisibleText;
					case EValue.MarkControlAbsolute:		return isMarkControlAbsolute;
					default: return false;
				}
			}

			public void SetValue(int iValueEnum, bool bValue)
			{
				switch ((EValue)iValueEnum)
				{
					case EValue.MarkVisibleText:			isMarkVisibleText			= bValue; break;
					case EValue.MarkControlAbsolute:		isMarkControlAbsolute		= bValue; break;
				}
			}

			[System.Serializable]
			public class ControlItem
			{
				public Toggle tg;
				public Text text;
			}
		}

		[Header("Func : AniOption")]
		[SerializeField] List<AniOptionBool.ControlItem> listOptionBoolControl;
		[SerializeField] public int iAniSelectionOptionBoolIndex { get; set; }

		[System.Serializable]
		public class AniOptionFloat : SaveSingleObjectBase<AniOptionFloat>
		{
			public override string strPathSave => $"Assets/Editor/Animation/Option";

			public enum EValue
			{
				MarkTextSize,
				MarkMoveInterval,
				MarkDoubleClickInterval,

				MAX
			}

			public const float default_fMarkTextSize = 30;
			public float fMarkTextSize = default_fMarkTextSize;

			public const float default_fMarkMoveInterval = 0.1f;
			public float fMarkMoveInterval = default_fMarkMoveInterval;

			public const float default_fMarkDoubleClickInterval = 0.5f;
			public float fMarkDoubleClickInterval = default_fMarkDoubleClickInterval;

			public static float GetDefault(int iValueEnum)
			{
				switch ((EValue)iValueEnum)
				{
					case EValue.MarkTextSize:				return default_fMarkTextSize;
					case EValue.MarkMoveInterval:			return default_fMarkMoveInterval;
					case EValue.MarkDoubleClickInterval:	return default_fMarkDoubleClickInterval;
					default: return 0;
				}
			}

			public void SetDefault(int iValueEnum) => SetValue(iValueEnum, GetDefault(iValueEnum));

			public float GetValue(int iValueEnum)
			{
				switch ((EValue)iValueEnum)
				{
					case EValue.MarkTextSize:				return fMarkTextSize;
					case EValue.MarkMoveInterval:			return fMarkMoveInterval;
					case EValue.MarkDoubleClickInterval:	return fMarkDoubleClickInterval;
					default: return 0;
				}
			}

			public void SetValue(int iValueEnum, float fValue)
			{
				switch ((EValue)iValueEnum)
				{
					case EValue.MarkTextSize:				fMarkTextSize				= fValue; break;
					case EValue.MarkMoveInterval:			fMarkMoveInterval			= fValue; break;
					case EValue.MarkDoubleClickInterval:	fMarkDoubleClickInterval	= fValue; break;
				}
			}
		}

		[SerializeField] List<InputField> listOptionFloatControl;
		[SerializeField] public int iAniSelectionOptionFloatIndex { get; set; }
		private void InitAniOption()
		{
			if (false == AniOptionBool.Main.Load(null))
				return;

			if (false == AniOptionFloat.Main.Load(null))
				return;

			AniOptionBool opBool = AniOptionBool.Main;
			AniOptionFloat opFloat = AniOptionFloat.Main;

			for (int i = 0; i < (int)AniOptionBool.EValue.MAX; ++i)
			{
				listOptionBoolControl[i].tg.isOn = opBool.GetValue(i);
			}

			for (int i = 0; i < (int)AniOptionFloat.EValue.MAX; ++i)
			{
				if (opFloat.GetValue(i) != AniOptionFloat.GetDefault(i) && 0 < opFloat.GetValue(i))
					listOptionFloatControl[i].text = opFloat.GetValue(i).ToString();
			}
		}

		public void OnChangeAnimationOptionBool(bool isOn)
		{
			AniOptionBool.Main.SetValue(iAniSelectionOptionBoolIndex, isOn);

			switch ((AniOptionBool.EValue)iAniSelectionOptionBoolIndex)
			{
				case AniOptionBool.EValue.MarkVisibleText:
				case AniOptionBool.EValue.MarkControlAbsolute:
				{
					SpreadAniDataMark();
				}
				break;
			}

			AniOptionBool.Main.Save(null);
		}

		public void OnChangeAnimationOptionFloat(string strValue)
		{
			if (string.IsNullOrEmpty(strValue))
			{
				AniOptionFloat.Main.SetDefault(iAniSelectionOptionFloatIndex);
				AniOptionFloat.Main.Save(null);
				return;
			}

			if (false == float.TryParse(strValue, out float fValue))
				return;

			AniOptionFloat.Main.SetValue(iAniSelectionOptionFloatIndex, fValue);

			switch ((AniOptionFloat.EValue)iAniSelectionOptionFloatIndex)
			{
				case AniOptionFloat.EValue.MarkTextSize: SpreadAniDataMark(); break;
			}

			AniOptionFloat.Main.Save(null);
		}
		#endregion

		#region Func : AniFunction
		[Header("Func : AniFunction")]
		[SerializeField] InputField ifAniSpeed;
		[SerializeField] InputField ifAniLengthFunction;

		public void OnClickAniSave()
		{
			AnimationManager.Single.SaveCSVData();

			Dev_PopupSimpleAlert.Info info = new Dev_PopupSimpleAlert.Info();
			info.fViewTime = 1;
			info.fDisposeTime = 0.5f;
			info.strDescription = "저장 완료";

			Dev_PopupBase.Get<Dev_PopupSimpleAlert>().Init(info).Open();
		}

		public void OnClickAniLoad()
		{
			CSVData.Battle.Anim.AnimationData.Manager.Load();
			CSVData.Battle.Anim.AnimationGroup.Manager.Load();
			AnimationManager.Single.LoadCSVData();

			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniType;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);

			Dev_PopupSimpleAlert.Info info = new Dev_PopupSimpleAlert.Info();
			info.fViewTime = 1;
			info.fDisposeTime = 0.5f;
			info.strDescription = "불러오기 완료";

			Dev_PopupBase.Get<Dev_PopupSimpleAlert>().Init(info).Open();
		}

		public void OnChangeAnimationSpeed(string strSpeed)
		{
			if (false == float.TryParse(strSpeed, out float fSpeed))
				return;

			fAniSpeed = fSpeed;
			aniModule.fTimeSpeed = fAniSpeed;
		}

		public void OnApplyAnimationSpeed()
		{
			adSelection.ApplySpeed(fAniSpeed);

			fAniSpeed = 1;
			aniModule.fTimeSpeed = 1;

			ifAniSpeed.SetTextWithoutNotify(string.Empty);

			bool isPlayed = aniModule.isPlay;
			float fPlaySection = aniModule.fPlaySection;
			
			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);
			
			if (isPlayed)
			{
				OnClickPlayButton();
			}
			aniModule.fPlaySection = fPlaySection;
		}

		public void OnClickAniFunctionLengthModifyOnlySelect()
		{
			if (false == float.TryParse(ifAniLengthFunction.text, out float fSetLength))
				return;



			SpreadAniDataMark();
		}

		public void OnClickAniFunctionLengthModifyTotal()
		{
			if (false == float.TryParse(ifAniLengthFunction.text, out float fSetLength))
				return;

			float fNowLength = adSelection.TotalLength;

			adSelection.ApplyTimeScale(fSetLength / fNowLength);

			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);
		}

		public void OnClickAniFunctionLengthModifyOnlyBack()
		{
			if (false == float.TryParse(ifAniLengthFunction.text, out float fSetLength))
				return;

			float fNowLength = adSelection.TotalLength;

			var pair = adSelection.listPair.Back();

			// 마지막 부분을 기존에 있던 구간보다 작게할 수 없음
			if (fSetLength < fNowLength - pair.timeLength)
				return;

			float fInterval = fSetLength - fNowLength;

			pair.timeLength += fInterval;
			pair.accumulate += fInterval;

			adSelection.RefreshLength();

			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);
		}
		#endregion

		#region Func : AniModify
		[Header("Func : AniModify")]
		public ObjectPool<Dev_Animation_SpriteSelectionItem> opSpriteSelectionItem = new ObjectPool<Dev_Animation_SpriteSelectionItem>();
		public InputField ifAniSelectionSprite;
		public Image imgAniSelectionCatalog;
		public Image imgAniSelectionPlayBar;

		public void OnChangeSelectionModifyInputBox(string str)
		{
			var listItem = SpriteManager.Single.GetContainer(SpriteManager.Container.EType.Animation).listTexture;

			opSpriteSelectionItem.CollectAllObject();

			int i = 0;
			listItem.ForEach(item =>
			{
				if (string.IsNullOrEmpty(str) || item.sprite.name.Contains(str))
				{
					var spSelectionItem = opSpriteSelectionItem.Pop();

					spSelectionItem.imgThumbnail.sprite = item.sprite;
					spSelectionItem.txtThumbnail.text = item.sprite.name;

					spSelectionItem.eDefine = (SpriteDefine.Animation)i;
				}
				++i;
			});
		}

		[HideInInspector] public Dev_Animation_SpriteSelectionItem pobjSelectionModifyItem;
		public void OnClickSelectionModifyItem(Dev_Animation_SpriteSelectionItem item)
		{
			pobjSelectionModifyItem = item;

			imgAniSelectionCatalog.sprite = item.imgThumbnail.sprite;
		}

		public void OnClickSelectionModifyAddFront()
		{
			adSelection.AddSpriteFront(pobjSelectionModifyItem.imgThumbnail.sprite, 0);

			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);
		}

		public void OnClickSelectionModifyAddBack()
		{
			adSelection.AddSprite(pobjSelectionModifyItem.imgThumbnail.sprite, 0.1f);

			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);
		}

		public void OnClickSelectionModifyCopy()
		{

		}

		public void OnClickSelectionModifyDel()
		{
			List<int> listValue = new List<int>(slistSelectMarkIndex.Values);

			int iCount = listValue.Count;
			for (int i = iCount - 1; 0 <= i; --i)
			{
				adSelection.DelSprite(listValue[i], false);
			}
			slistSelectMarkIndex.Clear();

			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);
		}

		public void OnClickSelectionModifyChg()
		{
			slistSelectMarkIndex.ForEach((i, temp) =>
			{
				adSelection.listPair[i].sprite = pobjSelectionModifyItem.imgThumbnail.sprite;
			});
			slistSelectMarkIndex.Clear();

			iAniSelectionItemIndex = (int)AniSelectionItem.EType.AniName;
			OnChangeAniSelectionItem(listAniSelectionItem[iAniSelectionItemIndex].iSelectionIndex);
		}
		#endregion
	}
}
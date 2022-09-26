using System.Collections.Generic;

namespace GGZ
{
	public class AnimationManager : SingletonBase<AnimationManager>
	{
		public enum EAniType
		{
			Unit,
			Boss,
			Effect,
			Bullet,

			MAX,
		}

#if _debug
		public static bool isInit = false;
#endif

		public List<Dictionary<int, Dictionary<string, AnimationModule.Group>>> listModule
			= new List<Dictionary<int, Dictionary<string, AnimationModule.Group>>>();

		private int iAniGroupSequence = 0;
		private int iAniDataSequence = 0;

		private int GetNextAniGroupSequence() => ++iAniGroupSequence;
		public int GetNextAniDataSequence() => ++iAniDataSequence;

		protected override void Init()
		{
			base.Init();

			LoadCSVData();
#if _debug
			isInit = true;
#endif
		}

		public void LoadCSVData()
		{
			listModule.Clear();
			listModule.Resize((int)EAniType.MAX);

			CSVData.Battle.Anim.AnimationGroup.Manager.ForEach((id, aniGroup) =>
			{
				AnimationModule.Group additionGroup = new AnimationModule.Group();
				additionGroup.iID = id;
				iAniGroupSequence = System.Math.Max(iAniGroupSequence, id);

				Dictionary<string, AnimationModule.Group> dictUnit = listModule[aniGroup.Type].GetSafe(aniGroup.UnitID);
				dictUnit.SetSafe(aniGroup.Naming, additionGroup);

				aniGroup.DataIDs.ForEach(iID =>
				{
					AnimationModule.Data additionData = new AnimationModule.Data();
					additionData.iID = iID;
					iAniDataSequence = System.Math.Max(iAniDataSequence, iID);

					additionGroup.AddData(additionData);

					var aniData = CSVData.Battle.Anim.AnimationData.Manager.Get(iID);

					int iCount = aniData.SpriteNames.Length;
					for (int i = 0; i < iCount; ++i)
					{
						if (System.Enum.TryParse(aniData.SpriteNames[i], out SpriteDefine.Animation eSprite))
						{
							additionData.AddSprite(
								SpriteManager.Single.Get(SpriteManager.Container.EType.Animation, (int)eSprite),
								aniData.TimeLengths[i]);
						}
					}
				});
			});
		}

		public void SaveCSVData()
		{
			CSVData.Battle.Anim.AnimationGroup.Manager.Clear();
			CSVData.Battle.Anim.AnimationData.Manager.Clear();

			SortedDictionary<int, CSVData.Battle.Anim.AnimationGroup> sdict = new SortedDictionary<int, CSVData.Battle.Anim.AnimationGroup>();

			int iType = 0;
			listModule.ForEach(dictType =>
			{
				dictType.ForEach((iUnitID, dictName) =>
				{
					dictName.ForEach((strAniName, aniGroup) =>
					{
						CSVData.Battle.Anim.AnimationGroup csvGroup = new CSVData.Battle.Anim.AnimationGroup();
						csvGroup.ID = aniGroup.iID;
						csvGroup.Naming = strAniName;
						csvGroup.Type = iType;
						csvGroup.UnitID = iUnitID;
						csvGroup.DataIDs = aniGroup.listData.ConvertAll(data => data.iID).ToArray();
						CSVData.Battle.Anim.AnimationGroup.Manager.Add(csvGroup);

						aniGroup.listData.ForEach(aniData =>
						{
							CSVData.Battle.Anim.AnimationData csvData = new CSVData.Battle.Anim.AnimationData();
							csvData.ID = aniData.iID;
							csvData.SpriteNames = aniData.listPair.ConvertAll(pair => pair.sprite.name).ToArray();
							csvData.TimeLengths = aniData.listPair.ConvertAll(pair => pair.timeLength).ToArray();
							CSVData.Battle.Anim.AnimationData.Manager.Add(csvData);
						});
					});
				});
				++iType;
			});

			CSVData.Battle.Anim.AnimationGroup.Manager.Save();
			CSVData.Battle.Anim.AnimationData.Manager.Save();
		}

		public int GetGroupCount(EAniType eType) => listModule[(int)eType].Count;
		public void DelGroup(EAniType eType, int id) => listModule[(int)eType].Remove(id);
		public Dictionary<string, AnimationModule.Group> GetGroup(EAniType eType, int id) => listModule[(int)eType].GetDef(id);
		public Dictionary<string, AnimationModule.Group> AddGroup(EAniType eType, int id, Dictionary<string, AnimationModule.Group> insertItem = null)
		{
			if (null == insertItem)
			{
				insertItem = new Dictionary<string, AnimationModule.Group>();
			}

			listModule[(int)eType].Add(id, insertItem);

			return insertItem;
		}

		public int GetGroupItemCount(EAniType eType, int id) => listModule[(int)eType].GetDef(id).Count;
		public void DelGroupItem(EAniType eType, int id, string strAniName) => listModule[(int)eType].GetDef(id)?.Remove(strAniName);
		public AnimationModule.Group GetGroupItem(EAniType eType, int id, string strAniName) => listModule[(int)eType].GetDef(id)?.GetDef(strAniName);
		public AnimationModule.Group AddGroupItem(EAniType eType, int id, string strAniName, AnimationModule.Group insertItem = null)
		{
			if (null == insertItem)
			{
				insertItem = new AnimationModule.Group();
				insertItem.iID = GetNextAniGroupSequence();

				// 기본 Sequence 추가
				AnimationModule.Data aniData = new AnimationModule.Data();
				aniData.iID = AnimationManager.Single.GetNextAniDataSequence();

				aniData.AddSprite(SpriteManager.Single.Get(0, 0), 1);
				insertItem.AddData(aniData);
			}

			GetGroup(eType, id).Add(strAniName, insertItem);

			return insertItem;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class ItemManager : SingletonBase<ItemManager>
	{
		private MemoryPool<Battle_BaseItem> mPool = new MemoryPool<Battle_BaseItem>();

		private Dictionary<int, Battle_BaseItem> conItem = new Dictionary<int, Battle_BaseItem>();

		protected override void Init()
		{
			base.Init();

			LoadCSVData();
		}

		public Battle_BaseItem Pop(Game.Item.ETypeMain eTypeMain)
		{
			Battle_BaseItem result = null;

			switch (eTypeMain)
			{
				case Game.Item.ETypeMain.Common: result = mPool.Pop<Battle_ItemCommon>(); break;
				case Game.Item.ETypeMain.Equipment: result = mPool.Pop<Battle_ItemEquipment>(); break;
			}

			return result;
		}

		private void LoadCSVData()
		{
			conItem.Clear();

			CSVData.Battle.Item.Define.Manager.ForEach((iID, objDefine) =>
			{
				var eType = (Game.Item.ETypeMain)objDefine.TypeMain;

				Battle_BaseItem objItem = Pop(eType);

				objItem.InitToCSV(objDefine);
				conItem.Add(iID, objItem);
			});
		}
	}
}


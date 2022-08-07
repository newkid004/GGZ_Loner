using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;
	using GlobalUtility;

	[System.Serializable]
	public class Battle_BaseItem : PooledMemory
	{
		public Game.Item.ETypeMain eTypeMain { get; protected set; }
		public int iTypeSub { get; protected set; }

		public virtual void InitToCSV(CSVData.Battle.Item.Define csvData) 
		{
			eTypeMain = (Game.Item.ETypeMain)csvData.TypeMain;
			iTypeSub = csvData.TypeSub;
		}
	}

	[System.Serializable]
	public class Battle_ItemCommon : Battle_BaseItem
	{
		public override void InitToCSV(CSVData.Battle.Item.Define csvData)
		{
			base.InitToCSV(csvData);
		}
	}

	[System.Serializable]
	public class Battle_ItemEquipment : Battle_BaseItem
	{
		public enum EStatusType
		{
			Basic,
			Effect,
			Battle,
			Player,

			MAX
		}

		public override void InitToCSV(CSVData.Battle.Item.Define csvData)
		{
			base.InitToCSV(csvData);

			csvData.Status.ForEach((iStatusID) =>
			{
				var csvStatus = CSVData.Battle.Item.Status.Manager.Get(iStatusID);
				var statObject = GetStatusObject((EStatusType)csvStatus.Type);

				statObject.Set(csvStatus.Index, statObject.Get(csvStatus.Index) + csvStatus.Value);
			});
		}

		public Game.Item.Equipment.ESlot eTypeSlot
		{
			get => (Game.Item.Equipment.ESlot)Digit.OR(iTypeSub, 0x0F);
			set => iTypeSub = Digit.OR(Digit.PICK(iTypeSub, 0x0F), (int)value);
		}

		public Game.Item.Equipment.EClass eTypeClass 
		{
			get => (Game.Item.Equipment.EClass)Digit.OR(iTypeSub, 0xF0);
			set => iTypeSub = Digit.OR(Digit.PICK(iTypeSub, 0xF0), (int)value);
		}

		public ObjectData.StatusBasic statBasic { get; protected set; } = new ObjectData.StatusBasic();
		public ObjectData.StatusEffect statEffect { get; protected set; } = new ObjectData.StatusEffect();
		public ObjectData.StatusBattle statBattle { get; protected set; } = new ObjectData.StatusBattle();
		public ObjectData.StatusPlayer statPlayer { get; protected set; } = new ObjectData.StatusPlayer();

		public ObjectData.StatusBase<float> GetStatusObject(EStatusType eType)
		{
			ObjectData.StatusBase<float> stat = null;
			switch (eType)
			{
				case EStatusType.Basic: stat = statBasic; break;
				case EStatusType.Effect: stat = statEffect; break;
				case EStatusType.Battle: stat = statBattle; break;
				case EStatusType.Player: stat = statPlayer; break;
			}
			return stat;
		}

		public float GetStatus(EStatusType eType, int iIndex)
		{
			var stat = GetStatusObject(eType);
			
			if (stat == null)
			{
#if _debug
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				Debug.LogError(strError + $"Invalid Item Status Type ({eType}, {iIndex})");
#endif
				return 0f;
			}

			return stat.Get(iIndex);
		}

		public void SetStatus(EStatusType eType, int iIndex, float fValue)
		{
			var stat = GetStatusObject(eType);

			if (stat == null)
			{
#if _debug
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				Debug.LogError(strError + $"Invalid Item Status Type ({eType}, {iIndex}, {fValue})");
#endif
				return;
			}

			stat.Set(iIndex, fValue);
		}
	}
}
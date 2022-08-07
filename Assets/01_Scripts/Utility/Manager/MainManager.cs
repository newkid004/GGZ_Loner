using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class MainManager : SingletonBase<MainManager>
	{
		[System.Serializable]
		public class Player
		{
			private Battle_ItemEquipment[] Equipment = new Battle_ItemEquipment[Game.Item.Equipment.ciSlotCount];

			public Battle_ItemEquipment GetEquipment(Game.Item.Equipment.ESlot eSlot) => Equipment[(int)eSlot];

			public Game.Item.Equipment.EClass eClass
			{
				get
				{
					var eqWeapon = GetEquipment(Game.Item.Equipment.ESlot.Weapon);
					return eqWeapon == null ? Game.Item.Equipment.EClass.Gunner : eqWeapon.eTypeClass;
				}
			}
		}

		public Player player { get; private set; } = new Player();
	}
}
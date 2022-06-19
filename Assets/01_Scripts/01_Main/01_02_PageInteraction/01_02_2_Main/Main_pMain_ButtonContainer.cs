using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Main_pMain_ButtonContainer : MonoBehaviour
	{
		[System.Serializable]
		public class ButtonItem
		{
			[SerializeField] public Main_PageBase.EID eID;

			[SerializeField] public Button button;
			[SerializeField] public Text text;
		}

		public List<ButtonItem> listButton;

		public void Init()
		{
			listButton.ForEach(item =>
			{
				switch(item.eID)
				{
					case Main_PageBase.EID.Dictionary:
						item.text.text = CSVData.Main.String.Manager.Get("Main_Home_Dictionary").Current; break;
					case Main_PageBase.EID.Shop:
						item.text.text = CSVData.Main.String.Manager.Get("Main_Home_Shop").Current; break;
					case Main_PageBase.EID.CastleOfInfinity:
						item.text.text = CSVData.Main.String.Manager.Get("Main_Home_TowerOfInfinity").Current; break;
					case Main_PageBase.EID.MyHome:
						item.text.text = CSVData.Main.String.Manager.Get("Main_Home_MyHome").Current; break;
					case Main_PageBase.EID.Stage:
						item.text.text = CSVData.Main.String.Manager.Get("Main_Home_Battle").Current; break;
				}
			});
		}
	}
}
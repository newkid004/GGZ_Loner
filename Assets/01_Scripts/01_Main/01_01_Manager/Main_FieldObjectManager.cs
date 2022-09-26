using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class Main_FieldObjectManager
	{
		public static Main_FieldObjectManager Single => SceneMain_Main.Single.mcsFieldObject;

		[SerializeField] private ObjectPool<Main_FieldObjectBase> opFieldObject = new ObjectPool<Main_FieldObjectBase>();

		public Main_FieldGround fgFieldCurrent;
		public Main_FieldGround fgFieldBuffer;

		public void Init()
		{
			opFieldObject.Init();

			InitPortalInfo();
		}

		private void InitPortalInfo()
		{
			HashSet<Main_FieldGround> hsField = new HashSet<Main_FieldGround>();
			HashSet<Main_FieldPortal> hsPortal = new HashSet<Main_FieldPortal>();

			void ProcessField(Main_FieldGround fg)
			{
				Main_FieldPortal[] arrPortal = fg.transform.GetComponentsInChildren<Main_FieldPortal>();
			}
		}

		public bool ProcessObjectPreRegist(Main_FieldCharacterPrePooled obj)
		{
			return opFieldObject.RegistObject(obj);
		}
	}
}

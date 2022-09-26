using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class FieldGroundManager : SingletonBase<FieldGroundManager>
	{
		public enum EFieldGround
		{
			None,

			Main = 1,
			Room,
			Shop,
			Achievement,

			Battle = 1000,

			MAX
		}

		public Main_FieldGround fgCurrent;
	}
}


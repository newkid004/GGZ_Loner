using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Main_FieldGround : MonoBehaviour
	{
		public List<Main_FieldObjectBase> listFieldObject = new List<Main_FieldObjectBase>();

		public virtual void OnEnableField() { }
		public virtual void OnDisableField() { }
	}
}

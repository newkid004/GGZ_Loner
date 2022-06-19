using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GGZ
{
	public class Dev_Animation_SpriteSelectionItem : PooledObject
	{
		public Image imgThumbnail;
		public Text txtThumbnail;

		[HideInInspector] public SpriteDefine.Animation eDefine;
	}
}


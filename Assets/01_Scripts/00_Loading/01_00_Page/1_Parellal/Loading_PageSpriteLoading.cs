using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GGZ
{
	public class Loading_PageSpriteLoading : Loading_PageBase
	{
		[SerializeField] private List<SpriteManager.stInputTextureData> listInput;
		public List<SpriteManager.stInputTextureData> ListInput => listInput;

		public override void ProcessLoad()
		{
			base.ProcessLoad();

			SpriteManager.Single.Init(listInput);

			ProcessLoadComplate();
		}
	}
}

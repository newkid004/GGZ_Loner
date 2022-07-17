using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	public class Loading_PageSpriteLoading : Loading_PageBase
	{
		[SerializeField] private List<SpriteManager.stInputTextureData> listInput;

		public override void ProcessLoad()
		{
			base.ProcessLoad();

			SpriteManager.Single.Init(listInput);

			ProcessLoadComplate();
		}
	}
}

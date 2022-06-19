using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Loading_PageSpriteLoadingBuilder : EnumDefineBuilder.BuildDataBase
	{
		public Loading_PageSpriteLoading page;

		public override List<InnerData> GetEnumList()
		{
			List<InnerData> listData = new List<InnerData>();

			page.ListInput.ForEach(inputData =>
			{
				InnerData innerData = new InnerData();

				innerData.name = inputData.strSpriteType;
				inputData.listSprite.ForEach(spriteData =>
				{
					InnerData enumData = new InnerData();
					enumData.name = spriteData.name;
					innerData.listInnerData.Add(enumData);
				});

				listData.Add(innerData);
			});

			return listData;
		}
	}
}
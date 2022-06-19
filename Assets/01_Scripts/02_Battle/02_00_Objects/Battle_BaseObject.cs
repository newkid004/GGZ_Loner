using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Battle_BaseObject : ProtoPooledObject
	{
		public int iObjectType { get; protected set; }
		public int iAttribute { get; protected set; }

		private void Awake()
		{
			Init();
		}

		protected override void Init()
		{
			base.Init();
			iObjectType = GlobalDefine.ObjectData.ObjectType.ciNone;
			iAttribute = GlobalDefine.ObjectData.Attribute.ciNone;
		}

		// 트리거 : 해당 오브젝트와 겹치는 곳에 사냥터가 생성됨
		public virtual void TriggeredByHuntZoneSpawnPlaced(Battle_HZone hzSpawned) { }

		// 트리거 : 해당 오브젝트와 겹치는 곳에 사냥터가 확장됨
		public virtual void TriggeredByHuntZoneExtendPlaced(Battle_HZone hzSpawned, List<Vector2> listExtendPoint) { }

		// 트리거 : 해당 오브젝트와 겹치는 곳에 사냥터가 손상됨
		public virtual void TriggeredByHuntZoneDamagedPlaced(Battle_HZone hzSpawned) { }

		// 트리거 : 해당 오브젝트와 겹치는 곳에 사냥터가 파괴됨 ( 없어짐 )
		public virtual void TriggeredByHuntZoneDestroyPlaced(Battle_HZone hzSpawned) { }
	}
}

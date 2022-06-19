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

		// Ʈ���� : �ش� ������Ʈ�� ��ġ�� ���� ����Ͱ� ������
		public virtual void TriggeredByHuntZoneSpawnPlaced(Battle_HZone hzSpawned) { }

		// Ʈ���� : �ش� ������Ʈ�� ��ġ�� ���� ����Ͱ� Ȯ���
		public virtual void TriggeredByHuntZoneExtendPlaced(Battle_HZone hzSpawned, List<Vector2> listExtendPoint) { }

		// Ʈ���� : �ش� ������Ʈ�� ��ġ�� ���� ����Ͱ� �ջ��
		public virtual void TriggeredByHuntZoneDamagedPlaced(Battle_HZone hzSpawned) { }

		// Ʈ���� : �ش� ������Ʈ�� ��ġ�� ���� ����Ͱ� �ı��� ( ������ )
		public virtual void TriggeredByHuntZoneDestroyPlaced(Battle_HZone hzSpawned) { }
	}
}

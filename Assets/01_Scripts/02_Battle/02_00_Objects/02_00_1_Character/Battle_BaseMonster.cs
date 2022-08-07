using GGZ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_BaseMonster : Battle_BaseCharacter
	{
		[Header("----- Base Monster -----")]
		[SerializeField]
		private ObjectData.StatusMonster _csStatMonster = new ObjectData.StatusMonster();
		public ObjectData.StatusMonster csStatMonster { get => _csStatMonster; }

		public CSVData.Battle.Status.Monster csvMonster = null;
		
		public AnimationManager.EAniType EAniType 
		{ 
			get
			{
				AnimationManager.EAniType eAniType;

				if (GlobalUtility.Digit.Include(iObjectType, GlobalDefine.ObjectData.ObjectType.ciBoss))
				{
					eAniType = AnimationManager.EAniType.Boss;
				}
				else
				{
					eAniType = AnimationManager.EAniType.Unit;
				}

				return eAniType;
			}
		}

		public void InitObjectDataToCSV(int iMonsterID)
		{
			csvMonster = CSVData.Battle.Status.Monster.Manager.Get(iMonsterID);
			ObjectData.StatusMonster statMonster = csStatMonster;

			statMonster.fAlertRadius = csvMonster.AlertRadius;
			statMonster.fAlertTime = csvMonster.AlertTime;

			statMonster.fAttackRadius = csvMonster.AttackRadius;

			// Renderer.sprite = SpriteManager.Single.Get(SpriteManager.Container.EType.Animation, (int)SpriteDefine.Animation.Monster_001_idle01);
		}

		public override void OnPushedToPool()
		{
			csvMonster = null;
			base.OnPushedToPool();
		}

		public override void TriggeredByHuntZoneExtendPlaced(Battle_HZone hzSpawned, List<Vector2> listExtendPoint)
		{
			base.TriggeredByHuntZoneExtendPlaced(hzSpawned, listExtendPoint);
			TriggeredByHuntZoneIntersect(hzSpawned, listExtendPoint);
		}

		public override void TriggeredByHuntZoneSpawnPlaced(Battle_HZone hzSpawned)
		{
			base.TriggeredByHuntZoneSpawnPlaced(hzSpawned);
			TriggeredByHuntZoneIntersect(hzSpawned, null);
		}

		protected virtual void TriggeredByHuntZoneIntersect(Battle_HZone hzSpawned, List<Vector2> listExtendPoint)
		{
			Battle_CharacterPlayer charPlayer = SceneMain_Battle.Single.charPlayer;
			TriggeredByTakeDamage(charPlayer);

			if (false == isAlive)
				return;

			// ����� ���ظ� ���� ������ �˹�
			KnockbackByHuntZone(hzSpawned, listExtendPoint);
		}

		protected virtual void KnockbackByHuntZone(Battle_HZone hzSpawned, List<Vector2> listHuntZonePoint)
		{
			// Ȯ���� �ƴ� ��� ����� ������ ���� �˹� ó��
			if (null == listHuntZonePoint)
			{
				listHuntZonePoint = new List<Vector2>();
				List<Battle_HPoint> listhlp = hzSpawned.lineEdge.listPoint;

				int iHlpCount = listhlp.Count;
				for (int i = 0; i < iHlpCount; ++i)
				{
					listHuntZonePoint.Add(listhlp[i].transform.position);
				}
			}

			// ����� �߽� �� �˹� ������ �� ����� Ȯ��
			int iDirection8ByInterval = Direction8.GetDirectionToInterval(hzSpawned.vec2Center, transform.position);
			List<List<Battle_HPoint>> listDirectionalPoint = new List<List<Battle_HPoint>>(hzSpawned.listDirectionalPoint);

			Vector2 vec2ResultPos = Vector2.zero;
			bool isKnockback = false;
			int iMaxSearchIndex = listDirectionalPoint.Count - 1;
			int iCurrentDirectionIndex = Direction8.cdictJoinDirArray[iDirection8ByInterval][8];
			while (0 < iMaxSearchIndex)
			{
				List<Battle_HPoint> listCurrentSearchPoint = listDirectionalPoint[iCurrentDirectionIndex];

				// ������ ����� ����� ��ȿ���� �ʴٸ�, Ž�� �������� ���� �� ���ο� ������ ������ ����
				if (listCurrentSearchPoint == null || listCurrentSearchPoint.Count == 0)
				{
					listDirectionalPoint[iCurrentDirectionIndex] = listDirectionalPoint[iMaxSearchIndex];

					iCurrentDirectionIndex = Random.Range(0, iMaxSearchIndex);
					--iMaxSearchIndex;

					continue;
				}

				// �˹� ���� ����� ���� ( 3�� Ž�� ���� ��, �ٸ� ����� ���� )
				int iCurrentHlpIndex = Random.Range(0, listCurrentSearchPoint.Count);
				Battle_HPoint hlp = listCurrentSearchPoint[iCurrentHlpIndex];

				for (int i = 0; isKnockback == false && i < 3; ++i)
				{
					Vector2 vec2KnockbackPos = Vector2.Lerp(
						hlp.transform.position,
						hlp.PointPrev.transform.position,
						Random.value);

					Vector2 vec2KnockbackDistance = (vec2KnockbackPos - hzSpawned.vec2Center).normalized * GlobalDefine.GVar.StatusEffect.c_fKnockbackDistance;
					for (int j = 0; isKnockback == false && j < 3; ++j)
					{
						vec2ResultPos = vec2KnockbackPos + (vec2KnockbackDistance * (1.0f / csStatEffect.fWeight) * Random.value);
						if (false == Physics2D.OverlapPoint(vec2ResultPos, 1 << GlobalDefine.CollideLayer.HuntZoneEdge))
						{
							isKnockback = true;

							// Log.Test
							Debug.Log($"Knockback. Monster : { iOwnSequenceID } / Point : { hlp.iOwnSequenceID }");
						}
					}
				}

				if (isKnockback == false)
				{
					listCurrentSearchPoint[iCurrentHlpIndex] = listCurrentSearchPoint[listCurrentSearchPoint.Count - 1];
					listCurrentSearchPoint.RemoveAt(listCurrentSearchPoint.Count - 1);
				}
				else
				{
					break;
				}
			}

			// �˹� �ִϸ��̼� ����
			if (true == isKnockback)
			{
				SceneMain_Battle.Single.mcsMonster.AnimateKnockbackByZone(this, vec2ResultPos);
			}
			else
			{
				// ���� �޼��� ���
			}
		}

		public override void TriggeredByDeadBegin(Battle_BaseCharacter charKiller)
		{
			base.TriggeredByDeadBegin(charKiller);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GGZ
{
	using GlobalDefine;

	public abstract class Battle_BaseCharacter : Battle_BaseObject
	{
		[Header("----- Base Character -----")]
		[Header("Own Component")]
		[SerializeField]
		private AnimationModule _AniModule;
		public AnimationModule AniModule { get => _AniModule; }

		[SerializeField]
		private Collider2D _colliderOwn;
		public Collider2D colliderOwn { get => _colliderOwn; }

		[SerializeField]
		private Rigidbody2D _RigidbodyOwn;
		public Rigidbody2D RigidbodyOwn { get => _RigidbodyOwn; }

		[SerializeField]
		private Battle_BaseBehaviour _behaviorOwn;
		public Battle_BaseBehaviour behaviorOwn { get => _behaviorOwn; set => SetBehavior(value, true); }

		[Header("Info : Character")]
		public long lCharacterID = -1;

		[SerializeField]
		private ObjectData.StatusEffect _csStatEffect = new ObjectData.StatusEffect();
		public ObjectData.StatusEffect csStatEffect { get => _csStatEffect; }

		[SerializeField] 
		private ObjectData.StatusBasic _csStatBasic = new ObjectData.StatusBasic();
		public ObjectData.StatusBasic csStatBasic { get => _csStatBasic; }

		public int iLastMovedDirection { get; set; }
		public int _iDirection { get; set; }
		public virtual int iDirection 
		{
			get => _iDirection;
			set
			{
				// ���� ��ȯ Ʈ����
				if (value != _iDirection)
				{
					iLastMovedDirection = _iDirection;
					_iDirection = value;
					behaviorOwn.OnChangeDirection(_iDirection, value, iLastMovedDirection);
				}
			}
		}

		// ���� ���� ������Ƽ
		public bool isMove { get; protected set; }
		public bool isAlive { get => 0 < csStatBasic.iLife; }

		protected override void Init()
		{
			base.Init();

			// _colliderOwn : ������ / �ν����Ϳ��� ���� �ʿ�
			// _RigidbodyOwn : ������ / �ν����Ϳ��� ���� �ʿ�
			// _behaviorOwn : ������ / �ν����Ϳ��� ���� �ʿ�

			base.iObjectType |= ObjectData.ObjectType.ciCharacter;
			base.iAttribute |= ObjectData.Attribute.ciBasic_Character;

			lCharacterID = -1;

			_iDirection = Direction8.ciDir_5;
			iLastMovedDirection = Direction8.ciDir_5;

			isMove = false;
		}

		public override void OnPopedFromPool()
		{
			base.OnPopedFromPool();

			behaviorOwn?.SetCharacter(this, false);
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			_AniModule = GetComponent<AnimationModule>();
			_colliderOwn = GetComponent<Collider2D>();
			_RigidbodyOwn = GetComponent<Rigidbody2D>();
			behaviorOwn = GetComponent<Battle_BaseBehaviour>();
		}

		public virtual void ProcessUpdateMove()
		{
			if (isMove)
			{
				float fMoveDistance = csStatBasic.fMoveSpeed;
				Vector2 vec2NormalDirection = Direction8.GetNormalByDirection(iDirection);

				RigidbodyOwn.velocity = fMoveDistance * vec2NormalDirection;

				behaviorOwn.OnChangePosition(transform.position);
			}
		}

		public virtual void ProcessUpdateMoveOld()
		{
			if (isMove)
			{
				float fMoveDistance = csStatBasic.fMoveSpeed;
				Vector2 vec2NormalDirection = Direction8.GetNormalByDirection(iDirection);

				Vector2 vec2MoveFromWorld = transform.position;
				Vector2 vec2MoveFromLocal = transform.localPosition;

				RaycastHit2D[] refArrHit = SceneMain_Battle.Single.mcsMonster.arrRch2CharMoveResult;

				// �浹 ���
				int iHitCount = Physics2D.RaycastNonAlloc(
					vec2MoveFromWorld,
					vec2NormalDirection,
					refArrHit,
					fMoveDistance,
					1 << GlobalDefine.CollideLayer.HuntZoneEdge);

				Vector2 vec2MoveFrom = vec2MoveFromLocal;
				Vector2 vec2MoveTo = Vector2.zero;

				if (iHitCount == 0)
				{
					// �Ϲ� �ʵ� �̵�
					vec2MoveTo = vec2MoveFrom + (vec2NormalDirection * fMoveDistance);
				}
				else
				{
					// ����Ϳ��� ����
					Vector2 vec2NormalForwardMoveDirection = vec2NormalDirection;

					for (int i = 0; i < iHitCount; ++i)
					{
						RaycastHit2D rch2Move = refArrHit[i];

						// �ܰ��� ���� ( �浹 ó�� �� �ش� �κк��� �̵� ���� ó�� )
						Vector2 vec2ContactDistance = rch2Move.point - vec2MoveFromWorld;
						fMoveDistance -= Vector2.Distance(vec2MoveFromWorld, rch2Move.point);

						Vector2 vec2ReflectNormal = vec2NormalDirection + rch2Move.normal;
						vec2ReflectNormal.Normalize();

						Vector2 vec2SlipMove = (vec2NormalForwardMoveDirection + vec2ReflectNormal).normalized;
						vec2SlipMove = Vector2.Dot(vec2NormalForwardMoveDirection, vec2SlipMove) * vec2SlipMove;

						// �̵��ӵ� ����
						Vector3 vec3MoveToDirection = Vector2.Dot(
							fMoveDistance * vec2ReflectNormal,
							fMoveDistance * vec2NormalDirection) * vec2ReflectNormal;

						// ���� ��ġ ����
						vec2MoveTo = vec2MoveFromLocal + vec2ContactDistance + fMoveDistance * vec2SlipMove;

						// ����� ��� ��ġ, ���� ���� ����
						vec2MoveFromWorld = rch2Move.point;
						vec2MoveFromLocal += vec2ContactDistance;
						vec2NormalForwardMoveDirection = vec2SlipMove.normalized;
					}
				}

				transform.localPosition = vec2MoveTo;
				behaviorOwn.OnChangePosition(vec2MoveTo);
			}
		}

		public void ProcessEnterMove(int iDirection)
		{
			this.iDirection = iDirection;
			isMove = true;
		}

		public void ProcessExitMove()
		{
			// this.iDirection = SCDirection8.ciDir_5;
			isMove = false;
			RigidbodyOwn.velocity = Vector3.zero;
		}

		public void SetBehavior(Battle_BaseBehaviour csBehavior, bool isSetBehaviorCharacter = true)
		{
			_behaviorOwn = csBehavior;

			if (isSetBehaviorCharacter)
			{
				_behaviorOwn?.SetCharacter(this, false);
			}
		}

		public virtual void ProcessLossLife()
		{
			csStatBasic.iLife--;
			TriggeredByLifeDecrease(SceneMain_Battle.Single.charPlayer);
		}

		public virtual void TriggeredByTakeDamage(Battle_BaseCharacter charAttacker, float fAttackPower)
		{
			ObjectData.StatusBasic statOwn = csStatBasic;

			float fDamage = Mathf.Max(0f, fAttackPower - statOwn.fDefendPower);
			if (fDamage < 0)
				return;

			statOwn.fHealthNow -= fDamage;

			// �ǰ� ����
			AniModule.spriteRenderer.material.color = Color.red;
			AniModule.spriteRenderer.material.DOColor(Color.white, 1.0f);

			if (statOwn.fHealthNow <= 0)
			{
				ProcessLossLife();
			}
		}

		public virtual void TriggeredByLifeDecrease(Battle_BaseCharacter charKiller)
		{
			ObjectData.StatusBasic statOwn = csStatBasic;

			if (0 < statOwn.iLife)
			{
				// ������ ���ҿ� ���� ü�� ȸ��
				statOwn.fHealthNow = statOwn.fHealthMax;
			}
			else
			{
				// �������� ��� ����
				TriggeredByLifeZero(charKiller);
			}
		}

		public virtual void TriggeredByLifeZero(Battle_BaseCharacter charKiller)
		{
			// ��� ����
			AniModule.spriteRenderer.material.color = Color.red;
			AniModule.spriteRenderer.material.DOColor(new Color(1, 1, 1, 0), 1.0f);

			CustomRoutine.CallLate(1.0f, () =>
			{
				TriggeredByDead(charKiller);
				Push();
				AniModule.spriteRenderer.material.color = Color.white;
			});   
		}

		public virtual void TriggeredByDead(Battle_BaseCharacter charKiller)
		{
			BattleManager.Single.OnCharacterDead(this, charKiller);
		}
	}
}
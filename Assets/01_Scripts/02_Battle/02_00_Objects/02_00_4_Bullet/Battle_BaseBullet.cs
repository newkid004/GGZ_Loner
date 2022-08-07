using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_BaseBullet : Battle_BaseObject
	{
		[Header("----- Base Bullet -----")]
		public SpriteRenderer rdrSplite;
		public new Rigidbody2D rigidbody;
		public CircleCollider2D colCircle;

		[Header("----- Bullet Info -----")]
		public Vector2 vec2Direction;
		public float fSpeed;

		public CSVData.Battle.Skill.BulletInfo csvInfo { get; set; }
		public float fDeadTime { get; set; }

		public bool isAlive { get; set; }
		public bool isAlly { get; set; }
		public Battle_BaseCharacter charOwner { get; set; }
		
		public Vector2 vec2Velocity => vec2Direction * fSpeed;
		public bool IsMove
		{
			get { return isAlive; }
			set
			{
				isAlive = value;

				colCircle.enabled = isAlive;
			}
		}

		protected Dictionary<object, float> dictHittedCharacter = new Dictionary<object, float>();

		protected override void Init()
		{
			base.Init();

			iObjectType = GlobalDefine.ObjectData.ObjectType.ciBullet;
		}

		public override void OnPopedFromPool()
		{
			base.OnPopedFromPool();
			
			isAlive = true;
			dictHittedCharacter.Clear();
		}

		public override void OnPushedToPool()
		{
			isAlive = false;
			base.OnPushedToPool();
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			rdrSplite = GetComponent<SpriteRenderer>();
			rigidbody = GetComponent<Rigidbody2D>();
			colCircle = GetComponent<CircleCollider2D>();
		}

		private void Update()
		{
			if (false == isAlive)
				return;

			switch (csvInfo.MoveType)
			{
				case 0:		// 선형 이동
				{
					rigidbody.velocity = vec2Velocity;
					break;
				}
			}

			CheckDispose();
		}

		public Vector2 TargetTo(Vector2 vec2Pos) => vec2Direction = (vec2Pos - transform.position.Vec2()).normalized;

		private void CheckDispose()
		{
			if (false == isAlive)
				return;

			// 수명 확인
			if (fDeadTime < Time.time)
			{
				TriggeredByDispose_DeadTime(Time.time);
				Push();
				return;
			}

			// 화면 외부 확인
			ref Rect rRect = ref SceneMain_Battle.Single.mcsField.rtFieldSpace;
			Vector2 vec2Pos = transform.position;
			float fCircleRadius = colCircle.radius;

			if (vec2Pos.x + fCircleRadius < -rRect.width  / 2f || rRect.width  / 2f < vec2Pos.x - fCircleRadius ||
				vec2Pos.y + fCircleRadius < -rRect.height / 2f || rRect.height / 2f < vec2Pos.y - fCircleRadius)
			{
				TriggeredByDispose_OutOfField(vec2Pos);
				Push();
				return;
			}
		}

		private List<int> listTargetLayer = new List<int>()
		{
			CollideLayer.CharBody,
			CollideLayer.CharPoint,
			CollideLayer.PlayerPoint,
			CollideLayer.HuntLineDrawing,
			CollideLayer.HuntLineDrawings,
			CollideLayer.HuntLineDrawed,
			CollideLayer.Bullet,
		};
		private void OnTriggerStay2D(Collider2D collision)
		{
			if (false == listTargetLayer.Contains(collision.gameObject.layer))
				return;

			Battle_BaseObject bObj = collision.GetComponent<Battle_BaseObject>();
			if (bObj == null)
				return;

			Battle_SkillManager.stHitTypeInfo stHitTypeInfo = new Battle_SkillManager.stHitTypeInfo();
			stHitTypeInfo.csvTargetingPreset = CSVData.Battle.Skill.TargetingPreset.Manager.Get(csvInfo.TargetingID);
			stHitTypeInfo.isActiveAlly = isAlly;
			stHitTypeInfo.objTarget = bObj;
			stHitTypeInfo.objOwner = charOwner;

			SceneMain_Battle.Single.mcsSkill.CheckHitType(ref stHitTypeInfo);

			if (0 < stHitTypeInfo.dgCalcTargetingFlag)
			{
				ProcessTargetHit(ref stHitTypeInfo);
			}
		}

		private void ProcessTargetHit(ref Battle_SkillManager.stHitTypeInfo stHitTypeInfo)
		{
			if (dictHittedCharacter.TryGetValue(stHitTypeInfo.objTarget, out float fNextHitTime))
			{
				if (0 < fNextHitTime && fNextHitTime < Time.time)
				{
					dictHittedCharacter[stHitTypeInfo.objTarget] = Time.time + stHitTypeInfo.csvTargetingPreset.HitInterval;
					TriggerByHit_TargetHit(stHitTypeInfo.objTarget, stHitTypeInfo.dgCalcTargetingFlag);
				}
			}
			else
			{
				float fHitInterval = CSVData.Battle.Skill.TargetingPreset.Manager.Get(csvInfo.TargetingID).HitInterval;

				if (0 < fHitInterval)
				{
					dictHittedCharacter.Add(stHitTypeInfo.objTarget, Time.time + stHitTypeInfo.csvTargetingPreset.HitInterval);
				}
				else
				{
					dictHittedCharacter.Add(stHitTypeInfo.objTarget, 0);
				}

				TriggerByHit_TargetHit(stHitTypeInfo.objTarget, stHitTypeInfo.dgCalcTargetingFlag);
			}
		}

		// 트리거 : 해당 오브젝트가 타게팅 프리셋에 의해 대상을 타격
		public virtual void TriggerByHit_TargetHit(Battle_BaseObject target, int dgType) { }

		// 트리거 : 제거됨 - 수명에 의해
		public virtual void TriggeredByDispose_DeadTime(float fTime) { }

		// 트리거 : 제거됨 - 화면 외부로 나감
		public virtual void TriggeredByDispose_OutOfField(Vector2 vec2Pos) { }
	}
}

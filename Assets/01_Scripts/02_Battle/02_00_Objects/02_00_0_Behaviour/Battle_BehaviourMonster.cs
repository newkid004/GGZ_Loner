using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_BehaviourMonster : Battle_BaseBehaviour
	{
		public enum EMoveState
		{
			Wait,
			Move,
			Aggressive,
			Dead,

			MAX
		}

		public enum ESubState
		{
			None,
			Attack,

			MAX
		}

		public CSVData.Battle.UnitBehaviour.PatternGroup csvPatternGroup = null;
		public CSVData.Battle.UnitBehaviour.PatternData csvPatternData = null;
		public int iPatternIndex;

		private float _fStateTimeBasic = 0;
		public float fStateTimeBasic
		{
			get => _fStateTimeBasic;
			set
			{
				_fStateTimeBasic = value *
						(eMoveState == Battle_BehaviourMonster.EMoveState.Aggressive ?
							characterOwn.csvMonster.ReactIntervalAggressive :
							characterOwn.csvMonster.ReactIntervalNormal);
			}
		}
		public float fStateTime = 0;

		public float fAlertTime = 0;

		protected EMoveState _eMoveState = EMoveState.Wait;
		public EMoveState eMoveState 
		{
			get => _eMoveState;
			set 
			{
				OnChangeMoveState(value);
			} 
		}
		public ESubState eSubState = ESubState.None;

		public new Battle_BaseMonster characterOwn { get => (Battle_BaseMonster)base.characterOwn; set => base.characterOwn = value; }
		public Battle_BaseCharacter charTarget { get; protected set; }

		protected int iRoutineIndex_ActionRun = -1;
		protected int iRoutineIndex_ActionComplate = -1;

		public void InitPatternState()
		{
			if (csvPatternGroup == null)
				return;

			Battle_PatternManager.Single.ProcessBeginPattern(this, 0, true);
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			if (eMoveState == EMoveState.Dead)
				return;

			UpdateMoveState();

			fStateTime -= Time.fixedDeltaTime;
			fAlertTime -= Time.fixedDeltaTime;
		}

		public override void OnPushedToPool()
		{
			base.OnPushedToPool();

			if (0 <= iRoutineIndex_ActionRun)
			{
				CustomRoutine.Stop(iRoutineIndex_ActionRun);
			}

			if (0 <= iRoutineIndex_ActionComplate)
			{
				CustomRoutine.Stop(iRoutineIndex_ActionComplate);
			}
		}

		private void UpdateMoveState()
		{
			switch (csvPatternData.Type)
			{
				// ���
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				case 1:
				{
					if (fStateTime < 0)
					{
						if (charTarget = CheckAttackable())
						{
							Battle_PatternManager.Single.ProcessAttackPattern(this);
						}
						else if (charTarget = CheckAggresive())
						{
							Battle_PatternManager.Single.ProcessAggressivePattern(this);
						}
					}
					else
					{
						Battle_PatternManager.Single.ProcessComplatePattern(this);
					}
				}
				break;

				// �̵�
				// 0 : �ð�
				// 1 : �Ϲ� �Ϸ� �� PatternDataIndex
				// 2 : (��� ������) ���� ���� �� PatternDataIndex
				case 2:
				{
					if (0 < fStateTime)
					{
						if (charTarget = CheckAttackable())
						{
							Battle_PatternManager.Single.ProcessAttackPattern(this);
						}
						else if (charTarget = CheckAggresive())
						{
							Battle_PatternManager.Single.ProcessAggressivePattern(this);
						}
					}
					else
					{
						Battle_PatternManager.Single.ProcessComplatePattern(this);
					}
				}
				break;

				// �ൿ
				// 0 : �ൿ ���� AniDataSeqenceIndex
				// 1 : �ൿ �ð� ���� ( 0.0 ~ 1.0 )
				// 2 : �ൿ ��� �� SkillID
				// 3 : �ൿ �Ϸ� �� PatternDataIndex
				case 3:
				{
					if (eSubState == ESubState.None)
					{
						int iAniDataSeqenceIndex = (int)csvPatternData.Params[0] - 1;
						float fTimeLateRatio = csvPatternData.Params[1];
						int iSkillID = (int)csvPatternData.Params[2];

						var aniGroupAttack = AnimationManager.Single.GetGroupItem(characterOwn.EAniType, characterOwn.iCharacterID, "atk");

						if (aniGroupAttack != null)
						{
							var aniData = aniGroupAttack.listData.GetDef(iAniDataSeqenceIndex);
							if (aniData != null)
							{
								// �ִϸ��̼� ����, ���� �ӵ��� ���� Ÿ�̹� ����
								float fAttackSpeedRatio = 1.0f / characterOwn.csStatBasic.fAttackSpeed;

								characterOwn.AniModule.SetGroup(aniGroupAttack);
								characterOwn.AniModule.fTimeSpeed = fAttackSpeedRatio;
								characterOwn.AniModule.Play(iAniDataSeqenceIndex);
								characterOwn.RefreshDirection();

								// ��ų �ߵ� ���� ����
								fTimeLateRatio = aniData.TotalLength * fTimeLateRatio * fAttackSpeedRatio;
								iRoutineIndex_ActionRun = CustomRoutine.CallLate(fTimeLateRatio, () =>
								{
									var stSkillInfo = new Battle_SkillManager.stSkillProcessInfo(iSkillID);

									stSkillInfo.objOwner = characterOwn;
									stSkillInfo.objTarget = charTarget;

									Battle_SkillManager.Single.ProcessSkill(ref stSkillInfo);
								});

								// ��ų �Ϸ�
								iRoutineIndex_ActionComplate = CustomRoutine.CallLate(aniData.TotalLength, () =>
								{
									eMoveState = EMoveState.Aggressive;
								});

								eSubState = ESubState.Attack;
								fStateTime = aniData.TotalLength;
							}
						}
					}

					if (fStateTime < 0)
					{
						Battle_PatternManager.Single.ProcessComplatePattern(this);
					}
				}
				break;

				// Ž��
				// 0 : �ð�
				// 1 : Ž�� ���� �� PatternDataIndex
				// 2 : Ž�� ���� �� PatternDataIndex
				// 3 : Ž�� ���� �� PatternDataIndex
				case 4:
				{
					if (0 < fStateTime)
					{
						if (charTarget = CheckAttackable())
						{
							Battle_PatternManager.Single.ProcessAttackPattern(this);
						}
						else if (charTarget = CheckAggresive())
						{
							Battle_PatternManager.Single.ProcessAggressivePattern(this);
						}
					}
					else
					{
						Battle_PatternManager.Single.ProcessComplatePattern(this);
					}
				}
				break;
			}
		}

		protected virtual void OnChangeMoveState(EMoveState eNextState)
		{
			switch (eNextState)
			{
				case EMoveState.Wait:
				case EMoveState.Dead:
				{
					characterOwn.ProcessExitMove();

				}
				break;

				case EMoveState.Move:
				{
					if (_eMoveState != EMoveState.Aggressive)
					{
						characterOwn.ProcessEnterMove(Direction8.GetRandomDirection());
					}
					else
					{
						characterOwn.ProcessEnterMove(Direction8.GetDirectionToInterval(
							characterOwn.transform.position.Vec2(),
							SceneMain_Battle.Single.charPlayer.transform.position.Vec2()));

						characterOwn.RefreshDirection();
					}
				}
				break;

				case EMoveState.Aggressive:
				{
					characterOwn.ProcessEnterMove(Direction8.GetDirectionToInterval(
						characterOwn.transform.position.Vec2(),
						SceneMain_Battle.Single.charPlayer.transform.position.Vec2()));

					characterOwn.RefreshDirection();
				}
				break;
			}

			if (eMoveState == EMoveState.Aggressive &&
				eSubState == ESubState.None &&
				0 < fAlertTime)
			{
				return;
			}

			_eMoveState = eNextState;

			string strAniGroupName = null;

			switch (_eMoveState)
			{
				case EMoveState.Wait:
				{
					strAniGroupName = "idle";
				}
				break;

				case EMoveState.Move:
				case EMoveState.Aggressive:
				{
					strAniGroupName = "walk";
				}
				break;

				case EMoveState.Dead:
				{
					strAniGroupName = "die";
				}
				break;
			}

			var aniGroup = AnimationManager.Single.GetGroupItem(characterOwn.EAniType, characterOwn.iCharacterID, strAniGroupName);

			characterOwn.AniModule.SetGroup(aniGroup);
			characterOwn.AniModule.fTimeSpeed = 1;
			characterOwn.AniModule.Play(0);
			characterOwn.RefreshDirection();

			switch (_eMoveState)
			{
				case EMoveState.Wait:
				case EMoveState.Move:
				case EMoveState.Aggressive:
				{
					characterOwn.AniModule.isRepeatForced = true;
				}
				break;

				case EMoveState.Dead:
				{
					characterOwn.AniModule.isRepeatForced = false;
				}
				break;
			}
		}

		protected virtual Battle_BaseCharacter CheckAggresive()
		{
			Battle_BaseCharacter charOther = null;

			if (GlobalUtility.Digit.Include(characterOwn.iObjectType, GlobalDefine.ObjectData.ObjectType.ciAlly))
			{
				// �Ʊ� ĳ���� -> �� ���� ���
				float fMinDistance = float.MaxValue;
				Battle_MonsterManager.Single.oPool.LoopOnActiveTotal(c =>
				{
					if (GlobalUtility.Digit.Declude(characterOwn.iObjectType, GlobalDefine.ObjectData.ObjectType.ciAlly))
					{
						float fDistance = Vector2.Distance(characterOwn.transform.position.Vec2(), c.transform.position.Vec2());
						if (fDistance < fMinDistance)
						{
							fMinDistance = fDistance;
							charOther = c;
						}
					}
				});

				if (GlobalUtility.Digit.Declude(characterOwn.iObjectType, GlobalDefine.ObjectData.ObjectType.ciPlayer))
				{
					// ��ȯ ����
					if (characterOwn.csvMonster.AlertRadius < fMinDistance)
					{
						charOther = null;
					}
				}
			}
			else
			{
				// �� ���� -> �Ʊ� ĳ���� ���
				charOther = SceneMain_Battle.Single.charPlayer;

				float fDistance = Vector2.Distance(characterOwn.transform.position.Vec2(), charOther.transform.position.Vec2());
				if (characterOwn.csStatMonster.fAlertRadius < fDistance)
				{
					charOther = null;
				}
			}

			return charOther;
		}

		protected virtual Battle_BaseCharacter CheckAttackable()
		{
			Battle_BaseCharacter charOther = null;

			if (eMoveState != EMoveState.Aggressive)
				return charOther;

			if (GlobalUtility.Digit.Include(characterOwn.iObjectType, GlobalDefine.ObjectData.ObjectType.ciAlly))
			{
				// �Ʊ� ĳ���� -> �� ���� ���
				float fMinDistance = float.MaxValue;
				Battle_MonsterManager.Single.oPool.LoopOnActiveTotal(c =>
				{
					if (GlobalUtility.Digit.Declude(characterOwn.iObjectType, GlobalDefine.ObjectData.ObjectType.ciAlly))
					{
						float fDistance = Vector2.Distance(characterOwn.transform.position.Vec2(), c.transform.position.Vec2());
						if (fDistance < fMinDistance)
						{
							fMinDistance = fDistance;
							charOther = c;
						}
					}
				});

				if (GlobalUtility.Digit.Declude(characterOwn.iObjectType, GlobalDefine.ObjectData.ObjectType.ciPlayer))
				{
					// ��ȯ ����
					if (characterOwn.csStatMonster.fAttackRadius < fMinDistance)
					{
						charOther = null;
					}
				}
			}
			else
			{
				// �� ���� -> �Ʊ� ĳ���� ���
				charOther = SceneMain_Battle.Single.charPlayer;

				float fDistance = Vector2.Distance(characterOwn.transform.position.Vec2(), charOther.transform.position.Vec2());
				if (characterOwn.csStatMonster.fAttackRadius < fDistance)
				{
					charOther = null;
				}
			}

			return charOther;
		}
	}
}


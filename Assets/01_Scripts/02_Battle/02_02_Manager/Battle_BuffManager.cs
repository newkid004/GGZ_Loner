using GGZ.GlobalDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class Battle_BuffManager
	{
		public static Battle_BuffManager Single { get => SceneMain_Battle.Single.mcsBuff; }

		public class BuffActivation : PooledMemory
		{
			public bool isComplate;
			public Dictionary<int, LinkedList<Battle_BaseBuff>> dictBuff =	// key : BelongID
				new Dictionary<int, LinkedList<Battle_BaseBuff>>();

			private Queue<int> quUpdateBelong = new Queue<int>();
			private HashSet<int> hsUpdateContain = new HashSet<int>();

			public void OnAddBuffBelong(int iBelongID)
			{
				if (0 <= iBelongID)
				{
					if (hsUpdateContain.Add(iBelongID))
					{
						quUpdateBelong.Enqueue(iBelongID);
					}
				}
			}

			public void FixedUpdate(float fFixedDeltaTime)
			{
				isComplate = true;

				// 갱신 항목 설정
				quUpdateBelong.Clear();
				hsUpdateContain.Clear();

				foreach (var iBelongID in dictBuff.Keys)
				{
					quUpdateBelong.Enqueue(iBelongID);
					hsUpdateContain.Add(iBelongID);
				}

				// 항목이 빌때까지 갱신
				while (0 < quUpdateBelong.Count)
				{
					int iBelongID = quUpdateBelong.Peek();
					var list = dictBuff[iBelongID];

					bool bComplateLocal = true;

					for (var node = list.First; node != null;)
					{
						var bf = node.Value;

						bf.FixedUpdate(fFixedDeltaTime);
						bComplateLocal = bComplateLocal && bf.isComplate;

						if (bf.isComplate)
						{
							node.Value.Push();
							node = list.RemovePop(node);
						}
						else
						{
							node = node.Next;
						}
					}

					if (bComplateLocal)
					{
						foreach (var bf in list)
						{
							bf.Push();
						}
						dictBuff.Remove(iBelongID);
					}

					isComplate = isComplate && bComplateLocal;

					quUpdateBelong.Dequeue();
				}
			}

			public override void OnPushedToPool()
			{
				foreach (var list in dictBuff)
				{
					foreach (var bf in list.Value)
					{
						bf.Push();
					}
				}

				quUpdateBelong.Clear();
				hsUpdateContain.Clear();

				base.OnPushedToPool();
			}
		}

		[SerializeField] private MemoryPool<Battle_BaseBuff> mPoolBuff = new MemoryPool<Battle_BaseBuff>();
		[SerializeField] private MemoryPool<BuffActivation> mPoolActivation = new MemoryPool<BuffActivation>();

		private Dictionary<Battle_BaseObject, BuffActivation> dictActiveBuff = new Dictionary<Battle_BaseObject, BuffActivation>();

		public struct stBuffProcessInfo
		{
			public CSVData.Battle.Skill.BuffInfo csvBuffInfo;

			public Battle_BaseObject objOwner;
			public Battle_BaseObject objTarget;

			public float[] arrfEffection;
		}

		public void Init()
		{
			mPoolBuff.Init();
			mPoolActivation.Init();
		}

		private Battle_BaseBuff PopToID(int iID)
		{
			Battle_BaseBuff objResult;

			switch (iID)
			{
				case -(int)Game.Item.Equipment.EClass.Summon:	objResult = mPoolBuff.Pop<Battle_PlayerBuffGunner>(); break;
				case -(int)Game.Item.Equipment.EClass.Worrier:	objResult = mPoolBuff.Pop<Battle_PlayerBuffGunner>(); break;
				case -(int)Game.Item.Equipment.EClass.Caster:	objResult = mPoolBuff.Pop<Battle_PlayerBuffGunner>(); break;
				case -(int)Game.Item.Equipment.EClass.Explorer:	objResult = mPoolBuff.Pop<Battle_PlayerBuffGunner>(); break;
				case -(int)Game.Item.Equipment.EClass.Gunner:	objResult = mPoolBuff.Pop<Battle_PlayerBuffGunner>(); break;

				case 5: objResult = mPoolBuff.Pop<Battle_BuffTest5>(); break;
				case 6: objResult = mPoolBuff.Pop<Battle_BuffTest6>(); break;
				case 7: objResult = mPoolBuff.Pop<Battle_BuffTest7>(); break;
				default: objResult = mPoolBuff.Pop<Battle_BuffProcessHit>(); break;
			}

			return objResult;
		}

		public LinkedList<Battle_BaseBuff> GetObjBelongBuffList(Battle_BaseObject obj, int iBelongID) 
			=> dictActiveBuff.GetDef(obj)?.dictBuff.GetDef(iBelongID);

		public void LoopObjBelongBuffList(Battle_BaseObject obj, System.Action<int, Battle_BaseBuff> actCallback)
		{
			var ba = dictActiveBuff.GetDef(obj);

			if (ba != null)
			{
				foreach (var buffList in ba.dictBuff)
				{
					foreach(var buff in buffList.Value)
					{
						actCallback(buffList.Key, buff);
					}
				}
			}
		}

		public bool RemoveBuff(Battle_BaseObject obj)
		{
			var buff = dictActiveBuff.GetDef(obj);

			if (buff != null && dictActiveBuff.Remove(obj))
			{
				buff.Push();
				return true;
			}

			return false;
		}

		private List<Battle_BaseObject> listCollectBuffActObject = new List<Battle_BaseObject>();
		public void FixedUpdate(float fFixedDeltaTime)
		{
			if (dictActiveBuff.Count == 0)
				return;

			dictActiveBuff.LoopLinear((obj, buffActivation) =>
			{
				buffActivation.FixedUpdate(fFixedDeltaTime);

				if (buffActivation.isComplate)
				{
					listCollectBuffActObject.Add(obj);
				}
			});

			if (0 < listCollectBuffActObject.Count)
			{
				listCollectBuffActObject.ForEach(obj =>
				{
					dictActiveBuff[obj].Push();
					dictActiveBuff.Remove(obj);
				});
				listCollectBuffActObject.Clear();
			}
		}

		public Battle_BaseBuff ProcessBuff(ref stBuffProcessInfo info)
		{
			if (info.csvBuffInfo == null)
				return null;

			Battle_BaseBuff bfAddition = null;

			if (dictActiveBuff.TryGetValue(info.objTarget, out BuffActivation ba) == false)
			{
				ba = mPoolActivation.Pop();
				dictActiveBuff.Add(info.objTarget, ba);
			}

			LinkedList<Battle_BaseBuff> listBelong = ba.dictBuff.GetSafe(info.csvBuffInfo.BelongID);

			bool isTriggerd_Begin = false;

			// Pre Process
			switch (info.csvBuffInfo.UpdateType)
			{
				case 0: // 유일 - 갱신하지 않음
					isTriggerd_Begin = true;
				break;

				case 1: // 유일 - 새로운 버프로 대체
				{
					isTriggerd_Begin = 0 < listBelong.Count;
					listBelong.Clear();
				}
				break;

				case 2: // 스택 - 넘을 경우 추가하지 않음
				break;

				case 3: // 스택 - 넘을 경우 오래된 것 부터 제거
				{
					if (info.csvBuffInfo.MaximumStack <= listBelong.Count)
					{
						Battle_BaseBuff bfRemove = listBelong.First.Value;

						var OwnSkillInfo = bfRemove.CreateOwnSkillProcess(bfRemove.csvBuffInfo.ActiveSkillIDEnd);
						bfRemove.TriggeredByEndBuff(ref OwnSkillInfo);

						listBelong.RemoveFirst();
					}
				}
				break;

				case 4: // 유일 - 가산 ( Time )
				case 5: // 유일 - 가산 ( Value )
				case 6: // 유일 - 가산 ( Value ), 갱신 ( Time )
				case 7: // 유일 - 가산 ( Value ), 가산 ( Time )
				{
					if (0 == listBelong.Count)
					{
						bfAddition = PopToID(info.csvBuffInfo.ID);
						bfAddition.Init(ref info);
						listBelong.AddLast(bfAddition);
						ba.OnAddBuffBelong(info.csvBuffInfo.BelongID);

						var OwnSkillInfo = bfAddition.CreateOwnSkillProcess(bfAddition.csvBuffInfo.ActiveSkillIDBegin);
						bfAddition.TriggeredByBeginBuff(ref OwnSkillInfo);
					}
					else
					{
						bfAddition = listBelong.First.Value;
					}
				}
				break;
			}

			// Post Process
			switch (info.csvBuffInfo.UpdateType)
			{
				case 0: // 유일 - 갱신하지 않음
				case 1: // 유일 - 새로운 버프로 대체
				{
					if (0 == listBelong.Count)
					{
						bfAddition = PopToID(info.csvBuffInfo.ID);
						bfAddition.Init(ref info);
						listBelong.AddLast(bfAddition);
						ba.OnAddBuffBelong(info.csvBuffInfo.BelongID);

						if (isTriggerd_Begin)
						{
							var OwnSkillInfo = bfAddition.CreateOwnSkillProcess(bfAddition.csvBuffInfo.ActiveSkillIDBegin);
							bfAddition.TriggeredByBeginBuff(ref OwnSkillInfo);
						}
					}
				}
				break;

				case 2: // 스택 - 넘을 경우 추가하지 않음
				case 3: // 스택 - 넘을 경우 오래된 것 부터 제거
				{
					if (listBelong.Count < info.csvBuffInfo.MaximumStack)
					{
						bfAddition = PopToID(info.csvBuffInfo.ID);
						bfAddition.Init(ref info);
						listBelong.AddLast(bfAddition);
						ba.OnAddBuffBelong(info.csvBuffInfo.BelongID);

						var OwnSkillInfo = bfAddition.CreateOwnSkillProcess(bfAddition.csvBuffInfo.ActiveSkillIDBegin);
						bfAddition.TriggeredByBeginBuff(ref OwnSkillInfo);
					}
				}
				break;

				case 4: // 유일 - 가산 ( Time )
				{
					bfAddition.fTimeLeft += bfAddition.fCalcedTime;
				}
				break;

				case 5: // 유일 - 가산 ( Value )
				{
					bfAddition.fActiveValue += bfAddition.fActiveValue;
				}
				break;

				case 6: // 유일 - 가산 ( Value ), 갱신 ( Time )
				{
					bfAddition.fTimeLeft = bfAddition.fCalcedTime;
					bfAddition.fActiveValue += bfAddition.fActiveValue;
				}
				break;

				case 7: // 유일 - 가산 ( Value ), 가산 ( Time )
				{
					bfAddition.fTimeLeft += bfAddition.fCalcedTime;
					bfAddition.fActiveValue += bfAddition.fActiveValue;
				}
				break;
			}

			return bfAddition;
		}
	}
}
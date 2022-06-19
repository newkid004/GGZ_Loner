using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	[System.Serializable]
	public class Battle_BuffManager
	{
		private class BuffActivation
		{
			public bool isComplate;
			public Dictionary<int, LinkedList<Battle_BaseBuff>> dictBuff =	// key : BelongID
				new Dictionary<int, LinkedList<Battle_BaseBuff>>();

			public void FixedUpdate(float fFixedDeltaTime)
			{
				isComplate = true;

				foreach (var list in dictBuff)
				{
					foreach (var bf in list.Value)
					{
						bf.FixedUpdate(fFixedDeltaTime);

						isComplate = isComplate && bf.IsCompleted;
					}
				}
			}
		}

		[SerializeField] private MemoryPool<Battle_BaseBuff> mPool = new MemoryPool<Battle_BaseBuff>();

		private Dictionary<Battle_BaseObject, BuffActivation> dictActiveBuff = new Dictionary<Battle_BaseObject, BuffActivation>();

		public struct stBuffProcessInfo
		{
			public CSVData.Battle.Skill.BuffInfo csvBuffInfo;

			public Battle_BaseObject objOwner;
			public Battle_BaseObject objTarget;
		}

		private Battle_BaseBuff PopToID(int iID)
		{
			Battle_BaseBuff objResult;

			switch (iID)
			{
				default: objResult = mPool.Pop(); break;
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

		private List<Battle_BaseObject> listCollectBuffActObject = new List<Battle_BaseObject>();
		public void FixedUpdate(float fFixedDeltaTime)
		{
			if (dictActiveBuff.Count == 0)
				return;

			dictActiveBuff.LoopLinear((obj, b) =>
			{
				b.FixedUpdate(fFixedDeltaTime);

				if (b.isComplate)
				{
					listCollectBuffActObject.Add(obj);
				}
			});

			if (0 < listCollectBuffActObject.Count)
			{
				listCollectBuffActObject.ForEach(obj => dictActiveBuff.Remove(obj));
				listCollectBuffActObject.Clear();
			}
		}

		public Battle_BaseBuff ProcessBuff(ref stBuffProcessInfo info)
		{
			if (info.csvBuffInfo == null)
				return null;

			Battle_BaseBuff bfAddition = null;

			BuffActivation ba = dictActiveBuff.GetSafe(info.objTarget);
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

						var OwnSkillInfo = bfAddition.CreateOwnSkillProcess(bfAddition.csvBuffInfo.ActiveSkillIDEnd);
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
						listBelong.AddLast(bfAddition);
						bfAddition.Init(ref info);

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
						listBelong.AddLast(bfAddition);
						bfAddition.Init(ref info);

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
						listBelong.AddLast(bfAddition);
						bfAddition.Init(ref info);

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
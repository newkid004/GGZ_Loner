using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class AnimationModule : MonoBehaviour
	{
		public class Data
		{
			public class Pair : System.IComparable<Pair>
			{ 
				public Sprite sprite; 
				public float timeLength;

				public float accumulate = 0;

				public int CompareTo(Pair other) => accumulate.CompareTo(other.accumulate);
			}

			// data
			public int iID;
			public List<Pair> listPair { get; private set; } = new List<Pair>();

			// info
			public int Count => listPair.Count;
			public float TotalLength
			{
				get => 0 < listPair.Count ? listPair.Back().accumulate : 0;
				set 
				{
					float fNowLength = TotalLength;

					float fScale = value / fNowLength;
					ApplyTimeScale(fScale);
				}
			}
			

			// property
			public bool isRepeat;

			public void AddSprite(Sprite sprite, float fTimeLength)
			{
				Pair addition = new Pair
				{
					sprite = sprite,
					timeLength = fTimeLength,
					accumulate = fTimeLength,
				};

				if (0 < listPair.Count)
				{
					addition.accumulate = addition.timeLength + listPair.Back().accumulate;
				}

				listPair.Add(addition);
			}

			public void AddSpriteFront(Sprite sprite, float fTimeLength = 0f)
			{
				Pair addition = new Pair
				{
					sprite = sprite,
					timeLength = fTimeLength,
					accumulate = fTimeLength,
				};

				if (0 < fTimeLength)
				{
					listPair.ForEach(pair =>
					{
						pair.timeLength += fTimeLength;
						pair.accumulate += fTimeLength;
					});
				}

				listPair.Insert(0, addition);
			}

			public bool DelSprite(int iIndex, bool isDeleteTime)
			{
				int iCount = Count;

				if (iCount <= iIndex || iIndex < 0)
					return false;

				Pair removeItem = listPair[iIndex];

				if (isDeleteTime)
				{
					for (int i = iIndex; i < iCount; i++)
					{
						listPair[i].accumulate -= removeItem.timeLength;
					}
				}
				else
				{
					if (iCount == 1)
						return false;

					Pair targetItem = iIndex == iCount - 1 ?
						listPair[iIndex - 1] :
						listPair[iIndex + 1];

					targetItem.timeLength += removeItem.timeLength;
				}

				listPair.RemoveAt(iIndex);

				return true;
			}

			public void ModifySpriteSection(int iIndex, float fAccumulate)
			{
				if (iIndex < 0 || listPair.Count <= iIndex)
					return;

				if (fAccumulate <= 0)
					return;

				Pair p = listPair[iIndex];

				float fInterval = fAccumulate - p.accumulate;

				if (iIndex == listPair.Count - 1)
				{
					p.timeLength += fInterval;
					p.accumulate = fAccumulate;
				}
				else
				{
					int iDestIndex;

					if (0 < fInterval)
					{
						iDestIndex = listPair.FindIndex(iIndex, pair => fAccumulate < pair.accumulate);

						if (iIndex == iDestIndex && iIndex < listPair.Count &&
							listPair[iIndex + 1].accumulate < fAccumulate)
						{
							iDestIndex++;
						}

						if (iDestIndex == -1)
						{
							iDestIndex = listPair.Count - 1;
						}
					}
					else
					{
						iDestIndex = listPair.FindLastIndex(iIndex, pair => pair.accumulate < fAccumulate);

						if (iDestIndex == -1)
						{
							iDestIndex = 0;
						}

						if (listPair[iDestIndex].accumulate < fAccumulate)
						{
							iDestIndex++;
						}
					}

					if (iDestIndex == -1)
					{
#if _debug
						string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
						Debug.LogError(strError + $"Invalid Value ({fAccumulate})");
#endif
						return;
					}

					if (iIndex == iDestIndex)
					{
						p.timeLength += fInterval;
						p.accumulate = fAccumulate;

						listPair[iIndex + 1].timeLength -= fInterval;
					}
					else
					{
						// pick
						listPair[iIndex + 1].timeLength += p.timeLength;
						listPair.RemoveAt(iIndex);

						// Insert
						int iListInsertionIndex = iDestIndex;

						if (iIndex < iDestIndex)
						{
							while (0 < iDestIndex && fAccumulate < listPair[iDestIndex - 1].accumulate) 
							{
								--iDestIndex;
							}
							iListInsertionIndex = iDestIndex;
						}

						listPair.Insert(iListInsertionIndex, p);

						p.accumulate = fAccumulate;
						p.timeLength = iListInsertionIndex == 0 ?
							fAccumulate :
							fAccumulate - listPair[iListInsertionIndex - 1].accumulate;

						if (iListInsertionIndex < listPair.Count - 1)
						{
							listPair[iListInsertionIndex + 1].timeLength -= p.timeLength;
						}
						else
						{
							RefreshLength();
						}
					}
				}
			}

			public void ApplySpeed(float fSpeed)
			{
				if (fSpeed <= 0)
				{
#if _debug
					string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
					Debug.LogError(strError + "Devide 0 Error");
#endif
					return;
				}

				float fRatio = 1.0f / fSpeed;
				listPair.ForEach(data => data.timeLength *= fRatio);

				RefreshLength();
			}

			public void ApplyTimeScale(float fScale)
			{
				if (fScale <= 0)
				{
#if _debug
					string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
					Debug.LogError(strError + "Devide 0 Error");
#endif
					return;
				}

				listPair.ForEach(data => data.timeLength *= fScale);
				RefreshLength();
			}

			public void RefreshLength()
			{
				int iCount = Count;
				if (0 < iCount)
				{
					listPair[0].accumulate = listPair[0].timeLength;
					for (int i = 1; i < iCount; ++i)
					{
						listPair[i].accumulate = listPair[i].timeLength + listPair[i - 1].accumulate;
					}
				}
			}
		}

		public class Group
		{
			public int iID;
			public List<Data> listData { get; private set; } = new List<Data>();

			public void AddData(Data data) => listData.Add(data);
			public void DelData(int iIndex) => listData.RemoveAt(iIndex);
		}

		[SerializeField] private SpriteRenderer rdrSprite;
		public SpriteRenderer spriteRenderer => rdrSprite;

		public Group aniGroup { get; private set; } = null;
		public Data aniData { get; private set; } = null;

		private float _fTimeSpeed = 1f;
		public float fTimeSpeed
		{
			get => _fTimeSpeed;
			set
			{
				if (value <= 0)
				{
#if _debug
					string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
					Debug.LogError(strError + "Devide 0 Error");
#endif
					return;
				}

				float fTimeScale = _fTimeSpeed / value;

				_fTimeSpeed = value;
				SetTimeScale(fTimeScale);
			}
		}

		// property
		public float fCurrentTimer { get; private set; } = 0;
		public float fTotalTimeLength { get; private set; } = 0;
		public float fPlaySection
		{
			get => fCurrentTimer / fTotalTimeLength;
			set => fCurrentTimer = fTotalTimeLength * value;
		}

		// status
		public int iCurrentDataIndex { get; private set; } = -1;
		public int iCurrentSpriteIndex { get; private set; } = -1;

		public bool isPlay = false;
		public bool isComplate { get; private set; } = true;

		// event
		/// <summary>
		/// arg1 : animation data index
		/// </summary>
		public UnityEngine.Events.UnityEvent<int> OnProcess;

		/// <summary>
		/// arg1 : complate animation group
		/// arg2 : complate and process replay
		/// </summary>
		public UnityEngine.Events.UnityEvent<bool, bool> OnComplate;

		public void Play(int iIndex = -1, bool isPause = false)
		{
			if (iIndex < 0 || aniGroup.listData.Count <= iIndex)
			{
#if _debug
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				Debug.LogError(strError + $"Out of index : {iIndex}, list : {aniGroup.listData.Count}");
#endif
				return;
			}

			fCurrentTimer = 0;

			isComplate = false;

			if (isPause)
			{
				Pause();
			}
			else
			{
				Resume();
			}

			if (iIndex == -1)
				return;

			iCurrentDataIndex = iIndex;

			aniData = aniGroup.listData[iCurrentDataIndex];
			iCurrentSpriteIndex = 0;

			rdrSprite.sprite = aniData.listPair[0].sprite;
			fTotalTimeLength = aniData.TotalLength / fTimeSpeed;
		}

		public void Replay()
		{
			fCurrentTimer = 0;

			iCurrentSpriteIndex = 0;

			isComplate = false;

			Resume();

			rdrSprite.sprite = aniData.listPair[0].sprite;
		}

		public void Stop()
		{
			fCurrentTimer = 0;

			isComplate = true;

			if (aniData != null)
			{
				rdrSprite.sprite = aniData.listPair[0].sprite;
			}

			Pause();
		}

		public void Pause()
		{
			isPlay = false;
		}

		public void Resume()
		{
			isPlay = true;
		}

		public void SetTimeScale(float fTimeScale)
		{
			if (fTimeScale <= 0)
			{
#if _debug
				string strError = $"{this.GetType().Name} : {System.Reflection.MethodBase.GetCurrentMethod().Name}\n";
				Debug.LogError(strError + "Devide 0 Error");
#endif
				return;
			}

			fCurrentTimer *= fTimeScale;
			fTotalTimeLength *= fTimeScale;
		}

		public void SetGroup(Group group)
		{
			Stop();

			aniGroup = group;

			if (group.listData.Count == 0)
			{
				aniData = null;
				rdrSprite.sprite = null;
			}
			else
			{
				aniData = group.listData[0];

				if (aniData.listPair.Count == 0)
				{
					rdrSprite.sprite = null;
				}
				else
				{
					rdrSprite.sprite = aniData.listPair[0].sprite;
				}
			}
		}

		public void Update()
		{
			if (false == isPlay)
				return;

			if (aniGroup == null || aniData == null)
				return;

			int iBeforeIndex = iCurrentSpriteIndex;
			float fLerp = fCurrentTimer / fTotalTimeLength;
			while (	iCurrentSpriteIndex < aniData.listPair.Count &&
					aniData.listPair[iCurrentSpriteIndex].accumulate < aniData.TotalLength * fLerp)
			{
				++iCurrentSpriteIndex;
			}

			if (aniData.Count <= iCurrentSpriteIndex)
			{
				if (aniData.isRepeat)
				{
					Replay();
				}
				else
				{
					Pause();
					isComplate = true;
				}

				OnComplate?.Invoke(isComplate, aniData.isRepeat);
			}
			else
			{
				if (iBeforeIndex != iCurrentSpriteIndex)
				{
					OnProcess?.Invoke(iCurrentSpriteIndex);
				}

				rdrSprite.sprite = aniData.listPair[iCurrentSpriteIndex].sprite;
			}

			fCurrentTimer += Time.deltaTime;
		}

		public void RefreshSection()
		{
			if (aniGroup == null || aniData == null)
				return;

			iCurrentSpriteIndex = 0;

			float fLerp = fCurrentTimer / fTotalTimeLength;
			while (iCurrentSpriteIndex < aniData.listPair.Count &&
					aniData.listPair[iCurrentSpriteIndex].accumulate < aniData.TotalLength * fLerp)
			{
				++iCurrentSpriteIndex;
			}

			rdrSprite.sprite = aniData.listPair[iCurrentSpriteIndex].sprite;
		}
	}
}


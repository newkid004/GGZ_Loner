using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto_00_N
{
	using GlobalUtility;
	using Proto_00_N.GlobalDefine;

	[System.Serializable]
	public class Battle_HuntLineManager
	{
		[SerializeField] private ObjectPool<Battle_HuntLineContainer> oPoolHuntLineContainer = new ObjectPool<Battle_HuntLineContainer>();
		[SerializeField] private ObjectPool<Battle_HuntLinePoint> oPoolHuntLinePoint = new ObjectPool<Battle_HuntLinePoint>();

		public Battle_HuntLineContainer hlcDrawing { get; set; }	// 현재 작성중인 사냥선 묶음
		public Battle_HuntLinePoint hlpDrawing { get; set; }		// 현재 작성중인 사냥선 지점

		public void Init()
		{
			oPoolHuntLinePoint.Init();
			oPoolHuntLineContainer.Init();

			hlcDrawing = null;
		}

		public Battle_HuntLineContainer GetContainer(int iSequenceID) => oPoolHuntLineContainer.GetObject(iSequenceID);
		public Battle_HuntLinePoint GetLine(int iSequenceID) => oPoolHuntLinePoint.GetObject(iSequenceID);

		public Battle_HuntLineContainer PopContainer(Transform trParent = null)
		{
			Battle_HuntLineContainer hObj = oPoolHuntLineContainer.Pop(trParent);
			hObj.ReconnectRefSelf();

			return hObj;
		} // Push는 관리되며 구현할 필요 없음

		public Battle_HuntLinePoint PopLinePoint(Transform trParent = null)
		{
			Battle_HuntLinePoint hObj = oPoolHuntLinePoint.Pop(trParent);
			hObj.ReconnectRefSelf();

			return hObj;
		} // Push는 관리되며 구현할 필요 없음

		/// <summary> 사냥선 그리기 시작 </summary>
		public void StartDrawHuntLine(Vector2 vec2StartPos)
		{
			Battle_HuntLineContainer hlcStartDraw = PopContainer();
			hlcStartDraw.transform.position = vec2StartPos;

			Battle_HuntLinePoint hlpStartDraw = hlcStartDraw.AddLinePoint(hlcStartDraw.transform.localPosition);

			hlcDrawing = hlcStartDraw;
			hlpDrawing = hlpStartDraw;

			hlpDrawing.ApplyDrawLine(true);
#if _debug
			Debug.Log($"Draw Huntline Start\nContainer : { hlcDrawing.iOwnSequenceID } / Point : { hlpDrawing.iOwnSequenceID }");
#endif
		}
		
		/// <summary> 사냥선 작성 중 방향 변경 </summary>
		public void ChangeHuntLineDirection()
		{
			Battle_HuntLinePoint hlpCurrentDrawed = hlpDrawing;
			Battle_HuntLinePoint hlpCurrentDrawing = hlcDrawing.AddLinePoint(hlpDrawing.vec2ToLocalPoint);

			hlpCurrentDrawed.ApplyDrawLine(false);
			hlpCurrentDrawing.ApplyDrawLine(true);

			hlpCurrentDrawed.vec2ToPoint = SceneMain_Battle.Single.charPlayer.transform.position;
			hlpDrawing = hlpCurrentDrawing;

#if _debug
			Debug.Log($"Huntline Add { hlpDrawing.iOwnSequenceID }");
#endif
		}

		/// <summary> 사냥선 작성 취소 </summary>
		public void CancelDrawHuntLine()
		{
			hlpDrawing = hlcDrawing.DeleteLastLinePoint();

			// 모든 사냥선 작성 취소됨
			if (null == hlpDrawing)
			{
				hlcDrawing.Push();
			}
			else
			{
				if (hlpDrawing.iContainIndex != 0)
				{
					SceneMain_Battle.Single.charPlayer.iDirection = Direction8.GetDirectionToNormal(hlpDrawing.vec2Direction);
				}
			}
		}
	}
}
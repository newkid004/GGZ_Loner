using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalUtility;
	using GGZ.GlobalDefine;

	[System.Serializable]
	public class Battle_HLineManager
	{
		[SerializeField] private ObjectPool<Battle_HLine> oPoolLine = new ObjectPool<Battle_HLine>();
		[SerializeField] private ObjectPool<Battle_HPoint> oPoolPoint = new ObjectPool<Battle_HPoint>();

		public Battle_HLine nowDrawingLine { get; set; }		// 현재 작성중인 사냥선 묶음
		public Battle_HPoint nowDrawingPoint { get; set; }		// 현재 작성중인 사냥선 지점

		public void Init()
		{
			oPoolPoint.Init();
			oPoolLine.Init();

			nowDrawingLine = null;
		}

		public Battle_HLine GetLine(int iSequenceID) => oPoolLine.GetObject(iSequenceID);
		public Battle_HPoint GetPoint(int iSequenceID) => oPoolPoint.GetObject(iSequenceID);

		public Battle_HLine PopLine(Transform trParent = null)
		{
			Battle_HLine hObj = oPoolLine.Pop(trParent);
			hObj.ReconnectRefSelf();

			return hObj;
		} // Push는 관리되며 구현할 필요 없음

		public Battle_HPoint PopPoint(Transform trParent = null)
		{
			Battle_HPoint hObj = oPoolPoint.Pop(trParent);
			hObj.ReconnectRefSelf();

			return hObj;
		} // Push는 관리되며 구현할 필요 없음

		/// <summary> 사냥선 그리기 시작 </summary>
		public void StartDrawLine(Vector2 vec2StartPos)
		{
			nowDrawingLine = PopLine();
			nowDrawingLine.transform.position = vec2StartPos;

			nowDrawingPoint = nowDrawingLine.AddLinePoint(vec2StartPos);
			nowDrawingPoint.ApplyDrawLine(true);
#if _debug
			Debug.Log($"Draw Huntline Start\nLine : { nowDrawingLine.iOwnSequenceID } / Point : { nowDrawingPoint.iOwnSequenceID }");
#endif
		}
		
		/// <summary> 사냥선 작성 중 방향 변경 </summary>
		public void ChangeLineDirection()
		{
			Vector2 vec2PlayerPos = SceneMain_Battle.Single.charPlayer.transform.position;

			Battle_HPoint hlpCurrentDrawed = nowDrawingPoint;
			Battle_HPoint hlpCurrentDrawing = nowDrawingLine.AddLinePoint(vec2PlayerPos);

			hlpCurrentDrawed.transform.position = vec2PlayerPos;

			hlpCurrentDrawed.ApplyDrawLine(false);
			hlpCurrentDrawing.ApplyDrawLine(true);

			nowDrawingPoint = hlpCurrentDrawing;
#if _debug
			Debug.Log($"Huntline Add { nowDrawingPoint.iOwnSequenceID }");
#endif
		}

		/// <summary> 사냥선 작성 취소 </summary>
		public void CancelDrawPoint()
		{
			nowDrawingPoint = nowDrawingLine.DeleteLastLinePoint();

			if (null != nowDrawingPoint)
			{
				if (nowDrawingPoint.iContainIndex != 0)
				{
					SceneMain_Battle.Single.charPlayer.iDirection = Direction8.GetDirectionToNormal(nowDrawingPoint.vec2Direction);
				}
			}
		}

		public void CancelDrawLine()
		{
			nowDrawingLine.Push();
		}
	}
}
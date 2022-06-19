using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Main_PageMain : Main_PageBase
	{
		public override int iID => (int)EID.Main;

		[SerializeField] private GameObject goPlayerIdle;
		private EID eProcessPage;

		public override void OnForward()
		{
			base.OnForward();

			eProcessPage = EID.None;
			goPlayerIdle.transform.position = Vector3.up * 5;
		}

		public void PressButtonSetProcessPage(int iPageID)
		{
			if (eProcessPage == EID.None)
				eProcessPage = (EID)iPageID;
		}

		public void PressButtonWalkIdleToButton(RectTransform rt)
		{
			if (eProcessPage != EID.None)
				return;

			Camera cam = Camera.main;
			Transform trIdleParent = goPlayerIdle.transform.parent;

			List<(Vector3, float)> listPath = new List<(Vector3, float)>(5);

			listPath.Add((goPlayerIdle.transform.position, 0));

			// 해당 Rect 사각형을 감싸는 꼭지점 계산 ( World 좌표 변환 )
			Vector2 vec2AnchoredPos = rt.anchoredPosition;
			vec2AnchoredPos.y += ((RectTransform)rt.parent).rect.height;

			Vector2 vec2RectSizeHalf = rt.sizeDelta / 2f;

			Vector2 vec2Rect5 = trIdleParent.InverseTransformPoint(cam.ScreenToWorldPoint(vec2AnchoredPos));
			Vector2 vec2Rect7 = trIdleParent.InverseTransformPoint(cam.ScreenToWorldPoint(vec2AnchoredPos + new Vector2(-vec2RectSizeHalf.x, +vec2RectSizeHalf.y)));
			Vector2 vec2Rect9 = trIdleParent.InverseTransformPoint(cam.ScreenToWorldPoint(vec2AnchoredPos + new Vector2(+vec2RectSizeHalf.x, +vec2RectSizeHalf.y)));
			Vector2 vec2Rect3 = trIdleParent.InverseTransformPoint(cam.ScreenToWorldPoint(vec2AnchoredPos + new Vector2(+vec2RectSizeHalf.x, -vec2RectSizeHalf.y)));
			Vector2 vec2Rect1 = trIdleParent.InverseTransformPoint(cam.ScreenToWorldPoint(vec2AnchoredPos + new Vector2(-vec2RectSizeHalf.x, -vec2RectSizeHalf.y)));

			// 중심점에서 약간 멀어지도록 보정
			vec2Rect7 += (vec2Rect7 - vec2Rect5) * 0.1f;
			vec2Rect9 += (vec2Rect9 - vec2Rect5) * 0.1f;
			vec2Rect3 += (vec2Rect3 - vec2Rect5) * 0.1f;
			vec2Rect1 += (vec2Rect1 - vec2Rect5) * 0.1f;

			// 좌/우 위치에 따른 경로 저장
			listPath.Add((new Vector2(goPlayerIdle.transform.position.x, vec2Rect1.y), 0));

			if (vec2AnchoredPos.x < vec2RectSizeHalf.x * 2f)
			{
				listPath.Add((vec2Rect1, 0));
				listPath.Add((vec2Rect7, 0));
				listPath.Add((vec2Rect9, 0));
				listPath.Add((vec2Rect3, 0));
			}
			else
			{
				listPath.Add((vec2Rect3, 0));
				listPath.Add((vec2Rect9, 0));
				listPath.Add((vec2Rect7, 0));
				listPath.Add((vec2Rect1, 0));
			}

			// 이동 경로 계산
			float fDistance = 0;
			int iPathCount = listPath.Count;
			for (int i = 1; i < iPathCount; ++i)
			{
				float fDist = Vector2.Distance(listPath[i - 1].Item1, listPath[i].Item1);
				fDistance += fDist;

				listPath[i] = (listPath[i].Item1, fDistance);	// 누적 거리 계산
			}

			// 경로 이동
			int iMoveIndex = 0;
			CustomRoutine.CallInTime(2f, fLerp =>
			{
				fLerp = Easing.EaseInOutCubic(0, 1, fLerp);

				(Vector2, float) tpNextPoint = listPath[iMoveIndex + 1];

				float fProcessDistance = fDistance * fLerp;

				while (tpNextPoint.Item2 < fProcessDistance)
				{
					++iMoveIndex;
					tpNextPoint = listPath[iMoveIndex + 1];
				}

				(Vector2, float) tpNowPoint = listPath[iMoveIndex];

				goPlayerIdle.transform.position = Vector3.Lerp(tpNowPoint.Item1, tpNextPoint.Item1,
					(fProcessDistance - tpNowPoint.Item2) / (tpNextPoint.Item2 - tpNowPoint.Item2));
			}, 
			
			// 이동 완료 시 Page Open
			OnComplateWalkIdle);
		}

		private void OnComplateWalkIdle()
		{
			CustomRoutine.CallLate(0.5f, () => SceneMain_Main.Single.mcsPage.OpenPage(eProcessPage));
		}
	}
}
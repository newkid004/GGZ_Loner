using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;

	public class UI_DirectionDragPanel : Button, IDragHandler
	{
		[SerializeField]
		private Camera camUICanvas;

		public IDirectionControllable idcConnect;

		public long lMicroSecondOfPointerReEnter;

		// 보존 값
		private System.DateTime dtLastPointerUp;
		private Vector2 vec2ButtonCenterPosInScreen;

		// 상태 참조 프로퍼티
		public bool IsClicked { get; protected set; }
		public int iDirection { get; protected set; }

		protected override void Awake()
		{
			dtLastPointerUp = System.DateTime.Now;
			vec2ButtonCenterPosInScreen = camUICanvas.WorldToScreenPoint(((RectTransform)transform).position);
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);

			OnDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (System.DateTime.Now.Ticks < dtLastPointerUp.Ticks + lMicroSecondOfPointerReEnter)
				return;

			// 방향까지 거리 임계값 확인
			Vector2 vec2PointerPosition = eventData.position;
			if (0 < Vector2.Distance(vec2ButtonCenterPosInScreen, vec2PointerPosition))
			{
				IsClicked = true;
				iDirection = Direction8.GetDirectionToInterval(vec2ButtonCenterPosInScreen, vec2PointerPosition);

				idcConnect.OnEnterDirection(iDirection);
			}
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);

			IsClicked = false;
			iDirection = Direction8.ciProcess_Non; // 값 : 0
			dtLastPointerUp = System.DateTime.Now;

			idcConnect.OnExitDirection();
		}
	}
}

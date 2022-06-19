using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GGZ
{
	public class DragDistanceInvoker : MonoBehaviour, IPointerDownHandler, IDragHandler
	{
		public float fDistanceThreshold;
		public UnityEvent<Vector2> evActive;

		private bool isTouch = false;
		private Vector2 vec2TouchDownPosition;

		private Vector2 vec2TouchNowPosition => Input.touchCount == 0 ?
				Input.mousePosition.Vec2() :
				Input.GetTouch(0).position;

		public void OnPointerDown(PointerEventData eventData)
		{
			vec2TouchDownPosition = vec2TouchNowPosition;
			isTouch = true;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (isTouch)
			{
				Vector2 vec2NowPos = vec2TouchNowPosition;

				if (fDistanceThreshold < Vector2.Distance(vec2TouchDownPosition, vec2NowPos))
				{
					evActive.Invoke(vec2NowPos - vec2TouchDownPosition);
					isTouch = false;
				}
			}
		}
	}
}
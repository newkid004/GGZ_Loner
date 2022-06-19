using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGZ
{
	public class LogMouseClickedObject : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			Debug.Log("Clicked : " + eventData.pointerCurrentRaycast.gameObject.name);
		}
	}
}

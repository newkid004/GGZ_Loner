using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGZ
{
	using GlobalDefine;

	public class Battle_MoveDirectionButton : Button, IDragHandler
	{
		[Header("Ref Main Scene")]
		[SerializeField]  private Battle_MoveDirectionPlayerController csPlayerController;

		private bool IsInitMoveDirection = false;

		[Header("Own Component")]
		public RectTransform rtransform;
		public Image imgOwn;

		[Header("Attribute")]
		[Range(0.0f, 1.0f)] public float fDistanceThresholdOfDirection; // 비구현
		public long lMicroSecondOfPointerReEnter;

		// Test
		[Header("Debug")]
		[SerializeField] private Transform trTest;

		// 보존 값
		private System.DateTime dtLastPointerUp;
		private Vector2 vec2ButtonCenterPosInScreen;

		// 상태 참조 프로퍼티
		public bool IsClicked { get; protected set; }
		public int iDirection { get; protected set; }

		protected override void Awake()
		{
			base.Awake();

			Init();
		}

		private void Init()
		{
			if (true == IsInitMoveDirection)
				return;

			dtLastPointerUp = System.DateTime.Now;
			vec2ButtonCenterPosInScreen = imgOwn.canvas.worldCamera.WorldToScreenPoint(rtransform.position);

			IsInitMoveDirection = true;

			// trTest?.gameObject.SetActive(false);
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);

			OnDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			// base.OnPointerDown(eventData);

			if (System.DateTime.Now.Ticks < dtLastPointerUp.Ticks + lMicroSecondOfPointerReEnter)
				return;

			// 방향까지 거리 임계값 확인
			Vector2 vec2PointerPosition = eventData.position;
			if (fDistanceThresholdOfDirection < Vector2.Distance(vec2ButtonCenterPosInScreen, vec2PointerPosition))
			{
				IsClicked = true;
				iDirection = Direction8.GetDirectionToInterval(vec2ButtonCenterPosInScreen, vec2PointerPosition);

				csPlayerController.OnEnterDirection(iDirection);
			}

			/*/ // Test
			trTest.gameObject.SetActive(true);
			trTest.position = imgOwn.canvas.worldCamera.ScreenToWorldPoint(eventData.position); 
			//*/
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);

			IsClicked = false;
			iDirection = Direction8.ciProcess_Non; // 값 : 0
			dtLastPointerUp = System.DateTime.Now;

			csPlayerController.OnExitDirection();
		}


		/*/ // Test
		void OnDrawGizmos()
		{
			Vector3 vec3Size;
			Vector2 vec2Line;
			vec3Size = imgOwn.rectTransform.rect.size;
			vec2Line = vec3Size;

			vec3Size = imgOwn.transform.TransformVector(vec3Size);
			vec2Line = vec3Size;

			// 선택 범위
			Gizmos.color = new Color(0, 1, 1, 0.2f);
			Gizmos.DrawCube(rtransform.position, vec3Size);

			// 
			if (trTest != null)
			{
				Vector3 vec3Pos = rtransform.position;
				Vector3 vec3AngledDistance = new Vector3();
				vec3AngledDistance.x = vec3Size.x;

				float fRadianInterval = f1PIDiv2 - f1PIDiv3;

				// vec3AngledDistance = GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv3);

				Gizmos.color = new Color(0, 1, 1);
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 0 - fRadianInterval * fDistanceThresholdOfDirection));
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 0 + fRadianInterval * fDistanceThresholdOfDirection));
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 1 - fRadianInterval * fDistanceThresholdOfDirection));
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 1 + fRadianInterval * fDistanceThresholdOfDirection));
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 2 - fRadianInterval * fDistanceThresholdOfDirection));
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 2 + fRadianInterval * fDistanceThresholdOfDirection));
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 3 - fRadianInterval * fDistanceThresholdOfDirection));
				Gizmos.DrawLine(rtransform.position, vec3Pos + GlobalUtility.Trigonometric.GetAngledVector3(vec3AngledDistance, f1PIDiv2 * 3 + fRadianInterval * fDistanceThresholdOfDirection));
			}
		}
		//*/
	}
}


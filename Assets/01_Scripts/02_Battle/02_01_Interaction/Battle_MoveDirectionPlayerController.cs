using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalDefine;
	using System;

	public class Battle_MoveDirectionPlayerController : MonoBehaviour
	{
		[SerializeField] private Battle_BehaviourPlayer bhvPlayer;
		[SerializeField] private Battle_CharacterPlayer charPlayer;

		[Header("Debug")]
		[SerializeField] private bool isDebug;
		[SerializeField]
		private Tuple<KeyCode, int>[] arrKeyCode = {
			Tuple.Create( KeyCode.Keypad1, Direction8.ciDir_1 ),
			Tuple.Create( KeyCode.Keypad2, Direction8.ciDir_2 ),
			Tuple.Create( KeyCode.Keypad3, Direction8.ciDir_3 ),
			Tuple.Create( KeyCode.Keypad4, Direction8.ciDir_4 ),
			Tuple.Create( KeyCode.Keypad6, Direction8.ciDir_6 ),
			Tuple.Create( KeyCode.Keypad7, Direction8.ciDir_7 ),
			Tuple.Create( KeyCode.Keypad8, Direction8.ciDir_8 ),
			Tuple.Create( KeyCode.Keypad9, Direction8.ciDir_9 ),
		};

		private void Update()
		{
			if (isDebug)
			{
				for (int i = 0; i < arrKeyCode.Length; ++i)
				{
					var tpDirection = arrKeyCode[i];

					if (Input.GetKeyDown(tpDirection.Item1))
					{
						OnEnterDirection(tpDirection.Item2);
					}
					else if (Input.GetKeyUp(tpDirection.Item1))
					{
						OnExitDirection();
					}
				}

				if (Input.GetKeyDown(KeyCode.Space))
				{
					bhvPlayer.bhvHuntline.OnToggleHuntLine(null);
				}

				// ¿µ¿ª ÆÄ±«
				if (Input.GetMouseButtonDown(0))
				{
					if (Input.GetKey(KeyCode.D))
					{
						Vector2 vec2PosClick = SceneMain_Battle.Single.camMain.ScreenToWorldPoint(Input.mousePosition);
						float fSize = 0.25f;

						int iAngle = 8;
						float fAngleInterval = (360 / iAngle) * Mathf.Deg2Rad;
						float fAngleCurrent = fAngleInterval / 2f;

						List<Vector2> listVec2DestroyArea = new List<Vector2>();
						for (int i = 0; i < iAngle; ++i)
						{
							Vector2 vec2 = GlobalUtility.Trigonometric.GetAngledVector2(Vector2.right, fAngleCurrent);
							vec2 = vec2PosClick + vec2 * fSize;
							vec2 = SceneMain_Battle.Single.mcsField.trCurrentBattleField.InverseTransformPoint(vec2);

							listVec2DestroyArea.Add(vec2);

							fAngleCurrent += fAngleInterval;
						}

						List<List<Vector2>> listDestroy = new List<List<Vector2>> { listVec2DestroyArea };
						SceneMain_Battle.Single.mcsHZone.DestroyZone(listDestroy);
					}
				}
			}
		}

		public void OnEnterDirection(int iDirection)
		{
			charPlayer.iLastInputDirection = iDirection;
			charPlayer.ProcessEnterMove(iDirection);
		}

		public void OnExitDirection()
		{
			charPlayer.ProcessExitMove();
		}
	}
}

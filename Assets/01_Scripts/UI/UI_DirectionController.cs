using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GGZ
{
	using GlobalDefine;

	public class UI_DirectionController : MonoBehaviour
	{
		public IDirectionControllable idcConnect;

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

		public void Update()
		{
			for (int i = 0; i < arrKeyCode.Length; ++i)
			{
				var tpDirection = arrKeyCode[i];

				if (Input.GetKeyDown(tpDirection.Item1))
				{
					idcConnect.OnEnterDirection(tpDirection.Item2);
				}
				else if (Input.GetKeyUp(tpDirection.Item1))
				{
					idcConnect.OnExitDirection();
				}
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				idcConnect.PressButton(0);
			}
		}
	}
}
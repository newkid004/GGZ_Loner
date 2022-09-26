using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public interface IDirectionControllable
	{
		public Rigidbody2D rigidbody2D { get; }

		public float fSpeed { get; set; }
		public int iDirection { get; set; }
		public bool isMove { get; set; }

		public void OnEnterDirection(int iDirection);
		public void OnExitDirection();

		public void MoveOnUpdate();

		public void PressButton(int iButton);
	}
}


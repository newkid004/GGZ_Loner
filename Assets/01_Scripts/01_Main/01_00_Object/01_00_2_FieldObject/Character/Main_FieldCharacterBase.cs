using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	public class Main_FieldCharacterBase : Main_FieldObjectBase, IDirectionControllable
	{
		[Header("[Main_FieldCharacterBase]")]
		[SerializeField] private Rigidbody2D _rigidbody2D;

		[SerializeField] protected float _fSpeed;
		protected int _iDirection;
		protected bool _isMove;

		public new Rigidbody2D rigidbody2D => _rigidbody2D;

		public float fSpeed { get => _fSpeed; set => _fSpeed = value; }
		public int iDirection { get => _iDirection; set => _iDirection = value; }
		public bool isMove { get => _isMove; set => _isMove = value; }

		public virtual void OnEnterDirection(int iDirection)
		{
			this.iDirection = iDirection;
			isMove = true;
		}

		public virtual void OnExitDirection()
		{
			isMove = false;
			rigidbody2D.velocity = Vector2.zero;
		}

		public virtual void MoveOnUpdate()
		{
			Vector2 vec2Speed = GlobalDefine.Direction8.GetNormalByDirection(iDirection) * fSpeed;

			rigidbody2D.velocity = vec2Speed;
		}

		public virtual void PressButton(int iButton) { }

		public virtual void Update()
		{
			if (isMove)
			{
				MoveOnUpdate();
			}
		}

		public override void ReconnectRefSelf()
		{
			base.ReconnectRefSelf();

			_rigidbody2D = GetComponent<Rigidbody2D>();
		}
	}
}

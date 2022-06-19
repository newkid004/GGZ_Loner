using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GGZ
{
	public class Battle_BaseBehaviour : MonoBehaviour
	{
		[Header("----- Base -----")]
		[Header("Own Component")]
		[SerializeField]
		private Battle_BaseCharacter _characterOwn;
		public Battle_BaseCharacter characterOwn { get => _characterOwn; set => SetCharacter(value, true); }

		protected virtual void Init(Battle_BaseCharacter initChar)
		{
		}

		protected virtual void FixedUpdate()
		{
			characterOwn?.ProcessUpdateMove();
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.gameObject.layer == GlobalDefine.CollideLayer.Field_Total)
			{
				OnExitField();
			}
		}

		public void SetCharacter(Battle_BaseCharacter csCharacter, bool isSetCharacterBehavior = true)
		{
			_characterOwn = csCharacter;
			Init(csCharacter);

			if (isSetCharacterBehavior)
			{
				_characterOwn?.SetBehavior(this, false);
			}
		}

		public virtual void OnChangeDirection(int iBefore, int iAfter, int iBeforeDirect) { }

		public virtual void OnChangePosition(Vector2 vec2Pos) { }

		public virtual void OnExitField()
		{
			characterOwn.ProcessLossLife();
		}
	}
}
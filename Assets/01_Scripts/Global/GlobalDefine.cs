using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalUtility;

	namespace GlobalDefine
	{
		// ���� ����
		public static class GVar
		{
			public const float c_fDegreeError = -100f;

			public static class Zone
			{
				public const float c_fStartHuntZoneScale = 0.05f;   // ó�� ����͸� ���� �� ��ü �ʵ� �� ����

				public const float c_fHuntZoneOutlineNarrowInterval = 0f;					// ����� �ܰ� �浹���� ��� ����
				public const float c_fPlayerMoveInHuntZoneOutlineCorrectInterval = 1f;		// �÷��̾��� ����� ���� �浹 ���� ����
			}

			public static class StatusEffect
			{
				public const float c_fKnockbackTime = 1.0f;				// ƨ�ܳ����� �⺻ �ð�
				public const float c_fKnockbackDistance = 1.0f;			// ƨ�ܳ����� �⺻ �Ÿ�
			}

			public enum EWindingOrder
			{
				None = 0,
				Clockwise = 1,
				CounterClockwise = 2,
			}
		}

		public static class CollideLayer
		{
			// Character
			public const int CharBody			= 6;
			public const int CharPoint			= 7;
			public const int PlayerPoint		= 8;

			// Hunt Zone
			public const int HuntLineDrawing	= 10;
			public const int HuntLineDrawings	= 11;
			public const int HuntLineDrawed		= 12;
			public const int HuntZoneEdge		= 13;
			public const int HuntZoneHole		= 14;

			// Field
			public const int Field_ToHuntZone	= 20;
			public const int Field_Total		= 21;
			public const int Field_DestroyZone	= 22;

			// Bullet
			public const int Bullet				= 28;

			public const int flagZoneIntersectBasic =
				(1 << CharPoint) |
				(1 << HuntZoneEdge);
		}

		// ��ġ�� ���� 8���� ����
		public static class Direction8
		{
			public enum JoinState
			{
				Error,
				/// <summary> �ڽ� </summary>
				Own,

				/// <summary> �밢 ���� </summary>
				Diagonal,

				/// <summary> ���� ���� </summary>
				Perpendicular,

				/// <summary> �ݴ� �밢 (�а�) </summary>
				Obtuse,

				/// <summary> �ݴ� </summary>
				Inverse,

				/// <summary> Ű�е� ���� </summary>
				Number,
			}

			#region Direction Constants

			// ���� ����
			public const int ciProcess_Non = 0b00;
			public const int ciProcess_Inc = 0b01;
			public const int ciProcess_Dec = 0b11;

			// ���� ��
			public const int ciDir_X = 0b0100;		// X�� ����
			public const int ciDir_Y = 0b0001;		// Y�� ����

			// ��Ʈ����ũ
			public const int ciDir_Mask_X = 0b1100;	  // ��Ʈ����ũ : X��
			public const int ciDir_Mask_Y = 0b0011;   // ��Ʈ����ũ : Y��
			public const int ciDir_Mask_Inc = 0b01;   // ��Ʈ����ũ : ��ǥ ����
			public const int ciDir_Mask_Dec = 0b10;   // ��Ʈ����ũ : ��ǥ ����

			// Ű�е� ��� ����
			public const int ciDir_1 = (ciDir_X * ciProcess_Dec) | (ciDir_Y * ciProcess_Dec);	// X ����, Y ����, BIN : 0b1111 , DEC : 15
			public const int ciDir_2 = (ciDir_X * ciProcess_Non) | (ciDir_Y * ciProcess_Dec);	// X ����, Y ����, BIN : 0b0011 , DEC : 3
			public const int ciDir_3 = (ciDir_X * ciProcess_Inc) | (ciDir_Y * ciProcess_Dec);	// X ����, Y ����, BIN : 0b0111 , DEC : 7
			public const int ciDir_4 = (ciDir_X * ciProcess_Dec) | (ciDir_Y * ciProcess_Non);	// X ����, Y ����, BIN : 0b1100 , DEC : 12
			public const int ciDir_5 = (ciDir_X * ciProcess_Non) | (ciDir_Y * ciProcess_Non);	// X ����, Y ����, BIN : 0b0000 , DEC : 0
			public const int ciDir_6 = (ciDir_X * ciProcess_Inc) | (ciDir_Y * ciProcess_Non);	// X ����, Y ����, BIN : 0b0100 , DEC : 4
			public const int ciDir_7 = (ciDir_X * ciProcess_Dec) | (ciDir_Y * ciProcess_Inc);	// X ����, Y ����, BIN : 0b1101 , DEC : 13
			public const int ciDir_8 = (ciDir_X * ciProcess_Non) | (ciDir_Y * ciProcess_Inc);	// X ����, Y ����, BIN : 0b0001 , DEC : 1
			public const int ciDir_9 = (ciDir_X * ciProcess_Inc) | (ciDir_Y * ciProcess_Inc);	// X ����, Y ����, BIN : 0b0101 , DEC : 5

			public static readonly int[] ciArrDir = new int[] { ciDir_1, ciDir_2, ciDir_3, ciDir_4, ciDir_6, ciDir_7, ciDir_8, ciDir_9 };
			public static readonly Dictionary<int, int[]> cdictJoinDirArray = new Dictionary<int, int[]>
			{
				// 0 : �ڽ�
				// 1 2 : �밢 ���� ( �ð� ���� )
				// 3 4 : ���� ���� ( �ð� ���� )
				// 5 6 : �� �밢 ���� ( �ð� ���� )
				// 7 : �ݴ� ����	
				// 8 : Ű�е� ����	// 0		1		 2		  3		   4		5		 6		  7			8
				{ ciDir_1, new int[] { ciDir_1, ciDir_4, ciDir_2, ciDir_7, ciDir_3, ciDir_8, ciDir_6, ciDir_9,	1 } }, 
				{ ciDir_2, new int[] { ciDir_2, ciDir_1, ciDir_3, ciDir_4, ciDir_6, ciDir_7, ciDir_9, ciDir_8,	2 } }, 
				{ ciDir_3, new int[] { ciDir_3, ciDir_2, ciDir_6, ciDir_1, ciDir_9, ciDir_4, ciDir_8, ciDir_7,	3 } },
				{ ciDir_4, new int[] { ciDir_4, ciDir_7, ciDir_1, ciDir_8, ciDir_2, ciDir_9, ciDir_3, ciDir_6,	4 } }, 
				{ ciDir_6, new int[] { ciDir_6, ciDir_3, ciDir_9, ciDir_2, ciDir_8, ciDir_1, ciDir_7, ciDir_4,	6 } },
				{ ciDir_7, new int[] { ciDir_7, ciDir_8, ciDir_4, ciDir_9, ciDir_1, ciDir_6, ciDir_2, ciDir_3,	7 } }, 
				{ ciDir_8, new int[] { ciDir_8, ciDir_9, ciDir_7, ciDir_6, ciDir_4, ciDir_3, ciDir_1, ciDir_2,	8 } }, 
				{ ciDir_9, new int[] { ciDir_9, ciDir_6, ciDir_8, ciDir_3, ciDir_7, ciDir_2, ciDir_4, ciDir_1,	9 } },
			};

			#endregion Direction Constants

			/// <summary> ���⿡ ���� ���԰� ��ȯ </summary>
			public static Vector2 GetNormalByDirection(int iDirection)
			{
				Vector2Int vec2Result = new Vector2Int(
					(iDirection & ciDir_Mask_X) / ciDir_X,
					(iDirection & ciDir_Mask_Y)
				);

				vec2Result.Set(
					(vec2Result.x & ciDir_Mask_Inc) * (0 < (vec2Result.x & ciDir_Mask_Dec) ? -1 : 1),
					(vec2Result.y & ciDir_Mask_Inc) * (0 < (vec2Result.y & ciDir_Mask_Dec) ? -1 : 1)
				);

				return new Vector2(vec2Result.x, vec2Result.y).normalized;
			}


			/// <summary> �Ÿ��� ���� ���԰� ��ȯ </summary>
			public static Vector2 GetNormalToInterval(Vector2 vec2From, Vector2 vec2To)
			{
				Vector2 vec2Result = Vector2.zero;
				Vector2 vec2PosDistance = vec2To - vec2From;

				if (vec2PosDistance.x == 0)
				{
					// x��ǥ 0 ������ ���� ó��
					vec2Result.y = 0f < vec2PosDistance.y ? 1 : -1;
				}
				else
				{
					float fAtan = Mathf.Atan2(vec2PosDistance.y, vec2PosDistance.x);
					float fAbsAtan = Mathf.Abs(fAtan);

					// ���� ���� : X(���� ���밪�� PI������ ���� ����), Y(��� ��)
					vec2Result.x = fAbsAtan < Trigonometric.f1PIDiv2 ? 1 : -1;
					vec2Result.y = 0f < fAtan ? 1 : 0;

					// �밢 �̵� Ȯ�� : Y�� �̵� ����
					if (Trigonometric.f2PIDiv3 / 2f < Mathf.Abs(Trigonometric.f1PIDiv2 - fAbsAtan))
					{
						vec2Result.y = 0;
					}

					// �밢 �̵� Ȯ�� : X�� �̵� ����
					if (Mathf.Abs(Trigonometric.f1PIDiv2 - fAbsAtan) < Trigonometric.f1PIDiv3 / 2f)
					{
						vec2Result.x = 0;
					}
				}

				return vec2Result;
			}

			/// <summary> ���԰��� ���� ���� ��ȯ </summary>
			public static int GetDirectionToNormal(Vector2 vec2Normal)
			{
				return GetDirectionToInterval(Vector2.zero, vec2Normal);
			}

			/// <summary> �Ÿ��� ���� ���� ��ȯ </summary>
			public static int GetDirectionToInterval(Vector2 vec2From, Vector2 vec2To)
			{
				int iResultDirection;

				Vector2 vec2PosDistance = vec2To - vec2From;

				if (vec2PosDistance.x == 0)
				{
					// x��ǥ 0 ������ ���� ó��
					iResultDirection = ciDir_Y * (0f < vec2PosDistance.y ? ciProcess_Inc : ciProcess_Dec);
				}
				else
				{
					float fAtan = Mathf.Atan2(vec2PosDistance.y, vec2PosDistance.x);
					float fAbsAtan = Mathf.Abs(fAtan);

					// ���� ���� : X(���� ���밪�� PI������ ���� ����), Y(��� ��)
					iResultDirection =
						(ciDir_X * (fAbsAtan < Trigonometric.f1PIDiv2 ? ciProcess_Inc : ciProcess_Dec)) |
						(ciDir_Y * (0f < fAtan ? ciProcess_Inc : ciProcess_Dec));
					
					// �밢 �̵� Ȯ�� : Y�� �̵� ����
					if (Trigonometric.f2PIDiv3 / 2f < Mathf.Abs(Trigonometric.f1PIDiv2 - fAbsAtan))
					{
						// ���� �� ��Ʈ����ŷ
						iResultDirection &= ciDir_Mask_X;
					}

					// �밢 �̵� Ȯ�� : X�� �̵� ����
					if (Mathf.Abs(Trigonometric.f1PIDiv2 - fAbsAtan) < Trigonometric.f1PIDiv3 / 2f)
					{
						iResultDirection &= ciDir_Mask_Y;
					}
				}

				return iResultDirection;
			}

			public static int GetRandomDirection()
			{
				return ciArrDir[Random.Range(0, 8)];
			}

			public static int GetInverseDirection(int iDirection)
			{
				int iDirX = iDirection & ciDir_Mask_X;
				int iDirY = iDirection & ciDir_Mask_Y;

				return
					(0 < iDirX ? (iDirX ^ (ciDir_X * ciDir_Mask_Dec)) : 0) |
					(0 < iDirY ? (iDirY ^ (ciDir_Y * ciDir_Mask_Dec)) : 0);
			}

			/// <summary> �� ���⿡ ���� ���� ��ȯ </summary>
			public static float GetDegreeToDirection(int iDirFrom, int iDirTo)
			{
				Vector2 vec2NormalFrom;
				Vector2 vec2NormalTo;

				// ������ ���� �� �׻� 0��
				if (iDirFrom == ciDir_5)
				{
					iDirFrom = ciDir_6;
				}

				vec2NormalFrom = GetNormalByDirection(iDirFrom);
				vec2NormalTo = GetNormalByDirection(iDirTo);

				return Vector2.Angle(vec2NormalFrom, vec2NormalTo);
			}

			public static JoinState GetJoinState(int iDirFrom, int iDirTo)
			{
				int[] arriJoin = cdictJoinDirArray.GetDef(iDirFrom);
				if (arriJoin == null)
				{
					return JoinState.Error;
				}

				int iIndex = 0;
				int iLength = arriJoin.Length;
				for (; iIndex < iLength; ++iIndex)
				{
					if (iDirTo == arriJoin[iIndex])
						break;
				}

				return iIndex switch
				{
					0 => JoinState.Own,
					1 => JoinState.Diagonal,
					2 => JoinState.Diagonal,
					3 => JoinState.Perpendicular,
					4 => JoinState.Perpendicular,
					5 => JoinState.Obtuse,
					6 => JoinState.Obtuse,
					7 => JoinState.Inverse,
					_ => JoinState.Error
				};
			}
		}

		// ��ü ���� ������ ����
		public static class ObjectData
		{
			// ��ü ���� ����
			public static class ObjectType
			{
				public const int ciNone = 0;

				// �ش��ϴ� ������ ���� Bit index			// ���� ��Ī
				public const int ciCharacter			= 1 << 0;	// ĳ����
				public const int ciPlayer				= 1 << 1;	// �÷��̾�
				public const int ciAlly					= 1 << 2;	// �Ʊ�
				public const int ciBoss					= 1 << 3;	// ����
				public const int ciPlayerPet			= 1 << 4;	// �÷��̾��� ��
				public const int ciHuntZone				= 1 << 5;	// �����
				public const int ciHuntZoneOutline		= 1 << 6;	// ����� �ܰ�
				public const int ciHuntZoneHole			= 1 << 7;	// ����� ����
				public const int ciHuntLine				= 1 << 8;	// �ۼ����� ��ɼ�
				public const int ciBullet				= 1 << 9;	// �Ѿ�
			}

			// ĳ���� �Ӽ� ����
			public static class Attribute
			{
				public const int ciNone = 0;

				// �ൿ ���� Bit index						// ��Ī		ON											OFF
				public const int ciDeath		= 1 << 0;	// ���		��� ó�� ����								���� ����
				public const int ciResurrection = 1 << 1;	// ��Ȱ		������ ���� �� ��Ȱ							������ ���� ���� ���� ��� ó��
				public const int ciHealth		= 1 << 2;	// ü��		ü�� �Ҹ� �� �Ӱ谪 �̸��� �� ��� ó��		ü�� �Ҹ� ����
				public const int ciMove			= 1 << 3;	// �̵�		�̵� ����									�̵� �Ұ�
				public const int ciCollision	= 1 << 4;	// �浹		�ٸ� �浹 ������ ��ü�� �浹 ����			�浹 ����
				public const int ciAttack		= 1 << 5;	// ����		������ ���� ������ �ൿ ���� ����			�ൿ ����
				public const int ciSkill		= 1 << 6;	// ��ų		������ ��ų ������ �ൿ ���� ����			�ൿ ����

				public const int ciBasic_Character = ciDeath | ciHealth | ciMove | ciCollision | ciAttack;
				public const int ciBasic_Player = ciDeath | ciResurrection | ciHealth | ciMove | ciAttack | ciSkill;
				public const int ciBasic_Normal_Monster = ciDeath | ciHealth | ciMove | ciCollision | ciAttack | ciSkill;
			}

			[System.Serializable]
			public abstract class StatusBase<T>
			{
				public abstract int iValueRange { get; }

				protected T[] varValueArray;

				protected StatusBase() => 
					varValueArray = new T[iValueRange];

				public T Get(int i) => varValueArray[i];
				public T Set(int i, T value) => varValueArray[i] = value;
			}

			// ȿ�� �ο� �������ͽ�
			[System.Serializable]
			public class StatusEffect : StatusBase<float>
			{
				public enum EDefine
				{
					Weight,		// ����			: �ش� ��ġ�� �������� ����, ª�� ƨ�ܳ� ( 0�� ��� ���� )
					AirHold,	// ü��			: �ش� ��ġ�� �������� ���� ƨ�ܳ� ( 0�� ��� ���� )

					MAX
				}

				public override int iValueRange		{ get => (int)EDefine.MAX; }

				public float fWeight				{ get => Get((int)EDefine.Weight); set => Set((int)EDefine.Weight, value); }
				public float fAirHold				{ get => Get((int)EDefine.AirHold); set => Set((int)EDefine.AirHold, value); }
			}

			// �⺻ �ο� �������ͽ�
			[System.Serializable]
			public class StatusBasic : StatusBase<float>
			{
				public enum EDefine
				{
					// Health
					Life,							// ������
					HealthMax,						// �ִ� ü��
					HealthNow,						// ���� ü��

					// Battle
					AttackPower,					// ���ݷ�
					DefendPower,					// ����
					PropertyAttackPower,			// �Ӽ� ���ݷ�

					// Control
					AttackSpeed,					// ���� �ӵ�
					MoveSpeed,						// �̵� �ӵ�

					MAX
				}

				public override int iValueRange		{ get => (int)EDefine.MAX; }

				public int iLife					{ get => (int)Get((int)EDefine.Life); set => Set((int)EDefine.Life, value); }
				public float fHealthMax				{ get => Get((int)EDefine.HealthMax); set => Set((int)EDefine.HealthMax, value); }
				public float fHealthNow				{ get => Get((int)EDefine.HealthNow); set => Set((int)EDefine.HealthNow, value); }

				public float fAttackPower			{ get => Get((int)EDefine.AttackPower); set => Set((int)EDefine.AttackPower, value); }
				public float fDefendPower			{ get => Get((int)EDefine.DefendPower); set => Set((int)EDefine.DefendPower, value); }
				public float fPropertyAttackPower	{ get => Get((int)EDefine.PropertyAttackPower); set => Set((int)EDefine.PropertyAttackPower, value); }

				public float fAttackSpeed			{ get => Get((int)EDefine.AttackSpeed); set => Set((int)EDefine.AttackSpeed, value); }
				public float fMoveSpeed				{ get => Get((int)EDefine.MoveSpeed); set => Set((int)EDefine.MoveSpeed, value); }
			}

			// ���� �ο� �������ͽ�
			[System.Serializable]
			public class StatusBattle
			{
				public float fSkillCooltimeRate = 1.0f;		// ��ų ��Ÿ�� ���� (1.0 = 100%)
			}

			// �÷��̾� �ο� �������ͽ�
			[System.Serializable]
			public class StatusPlayer
			{
				public float fHuntingGroundAttackPower = 1.0f;	// ����� ���ݷ�

				public float fHuntlineDrawReturnSpeed = 1.5f;	// ��ɼ� �ۼ� �ǵ����� �ӵ�
			}

			// ���� �ο� �������ͽ�
			[System.Serializable]
			public class StatusMonster
			{
				public float fAlertRadius = 1.0f;			// ��� �ݰ� : 0�̸� �񼱰�
				public float fAlertTime = 1.0f;				// ��� �ð� : 0�̸� �ൿ ����
				public float fAggressiveMoveSpeed = 1.5f;	// �߰� �� �̵� �ӵ� ������

				public bool isAlly = false;					// �Ʊ� ����
				public bool isSummoned = false;				// ��ȯ�� ����
			}


		}

		public static class Environment
		{
			public static System.Text.Encoding DefaultEncoding = System.Text.Encoding.GetEncoding("utf-8");
		}
	}
}
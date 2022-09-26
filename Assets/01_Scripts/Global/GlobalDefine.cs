using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGZ
{
	using GlobalUtility;

	namespace GlobalDefine
	{
		// 전역 변수
		public static class GVar
		{
			public const float c_fDegreeError = -100f;

			public static class Zone
			{
				public const float c_fStartHuntZoneScale = 0.05f;   // 처음 사냥터를 만들 때 전체 필드 당 비율

				public const float c_fHuntZoneOutlineNarrowInterval = 0f;					// 사냥터 외곽 충돌간격 축소 간격
				public const float c_fPlayerMoveInHuntZoneOutlineCorrectInterval = 1f;		// 플레이어의 사냥터 내부 충돌 보정 간격
			}

			public static class StatusEffect
			{
				public const float c_fKnockbackTime = 1.0f;				// 튕겨나가는 기본 시간
				public const float c_fKnockbackDistance = 1.0f;			// 튕겨나가는 기본 거리
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

		// 위치에 따른 8방향 정의
		public static class Direction8
		{
			public enum EJoinState
			{
				Error,
				/// <summary> 자신 </summary>
				Own,

				/// <summary> 대각 관계 </summary>
				Diagonal,

				/// <summary> 수직 관계 </summary>
				Perpendicular,

				/// <summary> 반대 대각 (둔각) </summary>
				Obtuse,

				/// <summary> 반대 </summary>
				Inverse,

				/// <summary> 키패드 숫자 </summary>
				Number,
			}

			#region Direction Constants

			// 진행 방향
			public const int ciProcess_Non = 0b00;
			public const int ciProcess_Inc = 0b01;
			public const int ciProcess_Dec = 0b11;

			// 방향 축
			public const int ciDir_X = 0b0100;		// X축 방향
			public const int ciDir_Y = 0b0001;		// Y축 방향

			// 비트마스크
			public const int ciDir_Mask_X = 0b1100;	  // 비트마스크 : X축
			public const int ciDir_Mask_Y = 0b0011;   // 비트마스크 : Y축
			public const int ciDir_Mask_Inc = 0b01;   // 비트마스크 : 좌표 증가
			public const int ciDir_Mask_Dec = 0b10;   // 비트마스크 : 좌표 감소

			// 키패드 기반 방향
			public const int ciDir_1 = (ciDir_X * ciProcess_Dec) | (ciDir_Y * ciProcess_Dec);	// X 감소, Y 감소, BIN : 0b1111 , DEC : 15
			public const int ciDir_2 = (ciDir_X * ciProcess_Non) | (ciDir_Y * ciProcess_Dec);	// X 감소, Y 감소, BIN : 0b0011 , DEC : 3
			public const int ciDir_3 = (ciDir_X * ciProcess_Inc) | (ciDir_Y * ciProcess_Dec);	// X 감소, Y 감소, BIN : 0b0111 , DEC : 7
			public const int ciDir_4 = (ciDir_X * ciProcess_Dec) | (ciDir_Y * ciProcess_Non);	// X 감소, Y 감소, BIN : 0b1100 , DEC : 12
			public const int ciDir_5 = (ciDir_X * ciProcess_Non) | (ciDir_Y * ciProcess_Non);	// X 감소, Y 감소, BIN : 0b0000 , DEC : 0
			public const int ciDir_6 = (ciDir_X * ciProcess_Inc) | (ciDir_Y * ciProcess_Non);	// X 감소, Y 감소, BIN : 0b0100 , DEC : 4
			public const int ciDir_7 = (ciDir_X * ciProcess_Dec) | (ciDir_Y * ciProcess_Inc);	// X 감소, Y 감소, BIN : 0b1101 , DEC : 13
			public const int ciDir_8 = (ciDir_X * ciProcess_Non) | (ciDir_Y * ciProcess_Inc);	// X 감소, Y 감소, BIN : 0b0001 , DEC : 1
			public const int ciDir_9 = (ciDir_X * ciProcess_Inc) | (ciDir_Y * ciProcess_Inc);	// X 감소, Y 감소, BIN : 0b0101 , DEC : 5

			public static readonly int[] ciArrDir = new int[] { ciDir_1, ciDir_2, ciDir_3, ciDir_4, ciDir_6, ciDir_7, ciDir_8, ciDir_9 };
			public static readonly Dictionary<int, int[]> cdictJoinDirArray = new Dictionary<int, int[]>
			{
				// 0 : 자신
				// 1 2 : 대각 인접 ( 시계 방향 )
				// 3 4 : 수직 인접 ( 시계 방향 )
				// 5 6 : 반 대각 인접 ( 시계 방향 )
				// 7 : 반대 방향	
				// 8 : 키패드 숫자	// 0		1		 2		  3		   4		5		 6		  7			8
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

			/// <summary> 방향에 따른 정규값 반환 </summary>
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


			/// <summary> 거리에 따른 정규값 반환 </summary>
			public static Vector2 GetNormalToInterval(Vector2 vec2From, Vector2 vec2To)
			{
				Vector2 vec2Result = Vector2.zero;
				Vector2 vec2PosDistance = vec2To - vec2From;

				if (vec2PosDistance.x == 0)
				{
					// x좌표 0 나누기 예외 처리
					vec2Result.y = 0f < vec2PosDistance.y ? 1 : -1;
				}
				else
				{
					float fAtan = Mathf.Atan2(vec2PosDistance.y, vec2PosDistance.x);
					float fAbsAtan = Mathf.Abs(fAtan);

					// 방향 설정 : X(값의 절대값이 PI제곱의 절반 이하), Y(양수 값)
					vec2Result.x = fAbsAtan < Trigonometric.f1PIDiv2 ? 1 : -1;
					vec2Result.y = 0f < fAtan ? 1 : 0;

					// 대각 이동 확인 : Y축 이동 제거
					if (Trigonometric.f2PIDiv3 / 2f < Mathf.Abs(Trigonometric.f1PIDiv2 - fAbsAtan))
					{
						vec2Result.y = 0;
					}

					// 대각 이동 확인 : X축 이동 제거
					if (Mathf.Abs(Trigonometric.f1PIDiv2 - fAbsAtan) < Trigonometric.f1PIDiv3 / 2f)
					{
						vec2Result.x = 0;
					}
				}

				return vec2Result;
			}

			/// <summary> 정규값에 따른 방향 반환 </summary>
			public static int GetDirectionToNormal(Vector2 vec2Normal)
			{
				return GetDirectionToInterval(Vector2.zero, vec2Normal);
			}

			/// <summary> 거리에 따른 방향 반환 </summary>
			public static int GetDirectionToInterval(Vector2 vec2From, Vector2 vec2To)
			{
				int iResultDirection;

				Vector2 vec2PosDistance = vec2To - vec2From;

				if (vec2PosDistance.x == 0)
				{
					// x좌표 0 나누기 예외 처리
					iResultDirection = ciDir_Y * (0f < vec2PosDistance.y ? ciProcess_Inc : ciProcess_Dec);
				}
				else
				{
					float fAtan = Mathf.Atan2(vec2PosDistance.y, vec2PosDistance.x);
					float fAbsAtan = Mathf.Abs(fAtan);

					// 방향 설정 : X(값의 절대값이 PI제곱의 절반 이하), Y(양수 값)
					iResultDirection =
						(ciDir_X * (fAbsAtan < Trigonometric.f1PIDiv2 ? ciProcess_Inc : ciProcess_Dec)) |
						(ciDir_Y * (0f < fAtan ? ciProcess_Inc : ciProcess_Dec));
					
					// 대각 이동 확인 : Y축 이동 제거
					if (Trigonometric.f2PIDiv3 / 2f < Mathf.Abs(Trigonometric.f1PIDiv2 - fAbsAtan))
					{
						// 제외 값 비트마스킹
						iResultDirection &= ciDir_Mask_X;
					}

					// 대각 이동 확인 : X축 이동 제거
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

			/// <summary> 각 방향에 따른 각도 반환 </summary>
			public static float GetDegreeToDirection(int iDirFrom, int iDirTo)
			{
				Vector2 vec2NormalFrom;
				Vector2 vec2NormalTo;

				// 각도가 없을 때 항상 0도
				if (iDirFrom == ciDir_5)
				{
					iDirFrom = ciDir_6;
				}

				vec2NormalFrom = GetNormalByDirection(iDirFrom);
				vec2NormalTo = GetNormalByDirection(iDirTo);

				return Vector2.Angle(vec2NormalFrom, vec2NormalTo);
			}

			public static EJoinState GetJoinState(int iDirFrom, int iDirTo)
			{
				int[] arriJoin = cdictJoinDirArray.GetDef(iDirFrom);
				if (arriJoin == null)
				{
					return EJoinState.Error;
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
					0 => EJoinState.Own,
					1 => EJoinState.Diagonal,
					2 => EJoinState.Diagonal,
					3 => EJoinState.Perpendicular,
					4 => EJoinState.Perpendicular,
					5 => EJoinState.Obtuse,
					6 => EJoinState.Obtuse,
					7 => EJoinState.Inverse,
					_ => EJoinState.Error
				};
			}
		}

		// 객체 관련 데이터 정의
		public static class ObjectData
		{
			// 객체 종류 정의
			public static class ObjectType
			{
				public const int ciNone = 0;

				// 해당하는 종류에 대한 Bit index			// 종류 명칭
				public const int ciCharacter			= 1 << 0;	// 캐릭터
				public const int ciPlayer				= 1 << 1;	// 플레이어
				public const int ciAlly					= 1 << 2;	// 아군
				public const int ciBoss					= 1 << 3;	// 보스
				public const int ciPlayerPet			= 1 << 4;	// 플레이어의 펫
				public const int ciHuntZone				= 1 << 5;	// 사냥터
				public const int ciHuntZoneOutline		= 1 << 6;	// 사냥터 외곽
				public const int ciHuntZoneHole			= 1 << 7;	// 사냥터 구멍
				public const int ciHuntLine				= 1 << 8;	// 작성중인 사냥선
				public const int ciBullet				= 1 << 9;	// 총알
			}

			// 캐릭터 속성 정의
			public static class Attribute
			{
				public const int ciNone = 0;

				// 행동 가능 Bit index						// 명칭		ON											OFF
				public const int ciDeath		= 1 << 0;	// 사망		사망 처리 가능								죽지 않음
				public const int ciResurrection = 1 << 1;	// 부활		라이프 감소 후 부활							라이프 관계 없이 영구 사망 처리
				public const int ciHealth		= 1 << 2;	// 체력		체력 소모 후 임계값 미만일 때 사망 처리		체력 소모 없음
				public const int ciMove			= 1 << 3;	// 이동		이동 가능									이동 불가
				public const int ciCollision	= 1 << 4;	// 충돌		다른 충돌 가능한 객체와 충돌 가능			충돌 없음
				public const int ciAttack		= 1 << 5;	// 공격		장착된 공격 모듈들의 행동 실행 가능			행동 없음
				public const int ciSkill		= 1 << 6;	// 스킬		장착된 스킬 모듈들의 행동 실행 가능			행동 없음

				public const int ciBasic_Character = ciDeath | ciHealth | ciMove | ciCollision | ciAttack;
				public const int ciBasic_Player = ciDeath | ciResurrection | ciHealth | ciMove | ciAttack | ciSkill;
				public const int ciBasic_Normal_Monster = ciDeath | ciHealth | ciMove | ciCollision | ciAttack | ciSkill;
			}

			[System.Serializable]
			public abstract class StatusBase<T>
			{
				public abstract int iValueRange { get; }

				[SerializeField] protected T[] varValueArray;

				protected StatusBase() => 
					varValueArray = new T[iValueRange];

				public T Get(int i) => varValueArray[i];
				public T Set(int i, T value) => varValueArray[i] = value;

				public void Merger<TDerived>(TDerived other, System.Func<T, T, T> funcSummer) where TDerived : StatusBase<T>
				{
					int iCount = varValueArray.Length;

					for (int i = 0; i < iCount; ++i)
					{
						Set(i, funcSummer(Get(i), other.Get(i)));
					}
				}
			}

			// 기본 부여 스테이터스
			[System.Serializable]
			public class StatusBasic : StatusBase<float>
			{
				public enum EDefine
				{
					// Health
					Life,							// 라이프
					HealthMax,						// 최대 체력
					HealthNow,						// 현재 체력

					// Battle
					AttackPower,					// 공격력
					DefendPower,					// 방어력
					PropertyAttackPower,			// 속성 공격력

					// Control
					AttackSpeed,					// 공격 속도
					MoveSpeed,						// 이동 속도

					MAX
				}
				public override int iValueRange		{ get => (int)EDefine.MAX; }

				public int iLife					{ get => (int)Get((int)EDefine.Life); set => Set((int)EDefine.Life, value); }
				public float fHealthMax				{ get => Get((int)EDefine.HealthMax); set => Set((int)EDefine.HealthMax, value); }
				public float fHealthNow				{ get => Get((int)EDefine.HealthNow); set => Set((int)EDefine.HealthNow, value); }

				public float fAttackPower			{ get => Get((int)EDefine.AttackPower); set => Set((int)EDefine.AttackPower, value); }
				public float fDefendPower			{ get => Get((int)EDefine.DefendPower); set => Set((int)EDefine.DefendPower, value); }
				public float fPropertyAttackPower	{ get => Get((int)EDefine.PropertyAttackPower); set => Set((int)EDefine.PropertyAttackPower, value); }

				public float fAttackSpeed			{ get => Get((int)EDefine.AttackSpeed); set => Set((int)EDefine.AttackSpeed, value); }		// 1초에 N번 공격
				public float fMoveSpeed				{ get => Get((int)EDefine.MoveSpeed); set => Set((int)EDefine.MoveSpeed, value); }
			}

			// 효과 부여 스테이터스
			[System.Serializable]
			public class StatusEffect : StatusBase<float>
			{
				public enum EDefine
				{
					Weight,							// 무게 : 해당 수치가 높을수록 낮게, 짧게 튕겨남 ( 0일 경우 오류 )
					AirHold,						// 체공 : 해당 수치가 높을수록 오래 튕겨남 ( 0일 경우 오류 )

					MAX
				}
				public override int iValueRange		{ get => (int)EDefine.MAX; }

				public float fWeight				{ get => Get((int)EDefine.Weight); set => Set((int)EDefine.Weight, value); }
				public float fAirHold				{ get => Get((int)EDefine.AirHold); set => Set((int)EDefine.AirHold, value); }
			}

			// 전투 부여 스테이터스
			[System.Serializable]
			public class StatusBattle : StatusBase<float>
			{
				public enum EDefine
				{
					SkillCooltimeRate,				// 스킬 쿨타임 비율 (1.0 = 100%)

					MAX
				}
				public override int iValueRange { get => (int)EDefine.MAX; }

				public float fSkillCooltimeRate { get => Get((int)EDefine.SkillCooltimeRate); set => Set((int)EDefine.SkillCooltimeRate, value); }
			}

			// 플레이어 부여 스테이터스
			[System.Serializable]
			public class StatusPlayer : StatusBase<float>
			{
				public enum EDefine
				{
					HuntingGroundAttackPower,		// 사냥터 공격력
					HuntlineDrawReturnSpeed,		// 사냥선 작성 되돌리기 속도

					MAX
				}
				public override int iValueRange { get => (int)EDefine.MAX; }

				public float fHuntingGroundAttackPower { get => Get((int)EDefine.HuntingGroundAttackPower); set => Set((int)EDefine.HuntingGroundAttackPower, value); }
				public float fHuntlineDrawReturnSpeed { get => Get((int)EDefine.HuntlineDrawReturnSpeed); set => Set((int)EDefine.HuntlineDrawReturnSpeed, value); }
			}

			// 몬스터 부여 스테이터스
			[System.Serializable]
			public class StatusMonster
			{
				public float fAlertRadius = 1.0f;			// 경계 반경 : 0이면 비선공
				public float fAlertTime = 1.0f;				// 경계 시간 : 0이면 행동 없음
				public float fAggressiveMoveSpeed = 1.5f;	// 발각 시 이동 속도 증가율

				public float fAttackRadius = 1.0f;			// 공격 반경 : 경계상태일 경우 해당 범위 내라면 공격

				public bool isAlly = false;					// 아군 여부
				public bool isSummoned = false;				// 소환물 여부
			}


		}

		// 게임 내 정의
		public static class Game
		{
			public static class Item
			{
				public enum ETypeMain
				{
					Common,
					Equipment,

					MAX
				}

				// 일반 : 0
				public static class Common
				{
					public static class TypeSub
					{

					}
				}

				// 장비 : 1
				public static class Equipment
				{
					// 착용 부위
					public enum ESlot
					{
						Weapon,		// 무기

						// 갑옷
						Head,		// 머리
						Body,		// 상체
						Leg,		// 하체
						Foot,		// 신발
						Glove,		// 장갑

						// 장신구
						Necklace,	// 목걸이
						Earring,	// 귀걸이
						Bracelet,	// 팔찌
						Ring,		// 반지

						Max
					}
					public const int ciSlotCount = (int)ESlot.Max;

					// 필수 착용 장비 ( 무기, 클래스 )
					public enum EClass
					{
						All			= 0x00,

						Summon		= 0x10,
						Worrier		= 0x20,
						Caster		= 0x30,
						Explorer	= 0x40,
						Gunner		= 0x50,
					}
				}
			}
		}

		public static class Environment
		{
			public static System.Text.Encoding DefaultEncoding = System.Text.Encoding.Unicode; //.GetEncoding("utf-8");
		}
	}
}
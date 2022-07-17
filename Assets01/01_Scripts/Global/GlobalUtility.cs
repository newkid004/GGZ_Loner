﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Proto_00_N
{
	namespace GlobalUtility
	{
		// public static List<string> GetDefineSymbols()
		// {
		//		// https://dragontory.tistory.com/60 // 유니티에서 스크립트 코드로 디파인 정의 추가 하기
		// }

		// 삼각함수 관련
		public static class Trigonometric
		{
			public const float f1PIDiv2 = Mathf.PI / 2f;
			public const float f1PIDiv3 = Mathf.PI / 3f;
			public const float f2PIDiv3 = 2f * Mathf.PI / 3f;

			public static Vector2 GetAngledVector2(Vector2 vec2Line, float fRadian)
			{
				return new Vector2(
					vec2Line.x * Mathf.Cos(fRadian) - vec2Line.y * Mathf.Sin(fRadian),
					vec2Line.x * Mathf.Sin(fRadian) + vec2Line.y * Mathf.Cos(fRadian));
			}

			public static Vector3 GetAngledVector3(Vector2 vec2Line, float fRadian)
			{
				return new Vector3(
					vec2Line.x * Mathf.Cos(fRadian) - vec2Line.y * Mathf.Sin(fRadian),
					vec2Line.x * Mathf.Sin(fRadian) + vec2Line.y * Mathf.Cos(fRadian),
					0);
			}

			/// <summary> 폴리곤 크기 계산 </summary>
			public static float GetPolygonAreaSize(List<Vector2> listPolygon, bool isAbs = true)
			{
				float fAreaSize = 0;

				int iCount = listPolygon.Count;
				for (int i = 0; i < iCount; ++i)
				{
					Vector2 vecCurr = listPolygon[i];
					Vector2 vecNext = listPolygon[i.ModStep(1, iCount)];

					fAreaSize += (vecNext.x - vecCurr.x) * (vecNext.y + vecCurr.y);
				}

				return (isAbs ? Mathf.Abs(fAreaSize) : fAreaSize) / 2f;
			}

			/// <summary> 폴리곤 크기 계산 </summary>
			public static float GetPolygonAreaSize<T>(List<T> listPolygon, Func<T, Vector2> funcExtractor, bool isAbs = true)
			{
				float fAreaSize = 0;

				int iCount = listPolygon.Count;
				for (int i = 0; i < iCount; ++i)
				{
					Vector2 vecCurr = funcExtractor(listPolygon[i]);
					Vector2 vecNext = funcExtractor(listPolygon[i.ModStep(1, iCount)]);

					fAreaSize += (vecNext.x - vecCurr.x) * (vecNext.y + vecCurr.y);
				}

				return (isAbs ? Mathf.Abs(fAreaSize) : fAreaSize) / 2f;
			}

			/// <summary> 폴리곤 방향 계산 </summary>
			public static GlobalDefine.GVar.EWindingOrder GetWindingOrder(List<Vector2> listPolygon)
			{
				return 0f < GetPolygonAreaSize(listPolygon, false) ?
					GlobalDefine.GVar.EWindingOrder.Clockwise :
					GlobalDefine.GVar.EWindingOrder.CounterClockwise;
			}

			/// <summary> 폴리곤 방향 계산 </summary>
			public static GlobalDefine.GVar.EWindingOrder GetWindingOrder<T>(List<T> listPolygon, Func<T, Vector2> funcExtractor)
			{
				return 0f < GetPolygonAreaSize(listPolygon, funcExtractor, false) ?
					GlobalDefine.GVar.EWindingOrder.Clockwise :
					GlobalDefine.GVar.EWindingOrder.CounterClockwise;
			}

			/// <summary> 선과 점 사이의 최단거리, 직교점 계산 </summary>
			public static float GetClosestDistanceByPoint2Line(Vector2 point, Vector2 listStartPoint, Vector2 lineEndPoint, out Vector2 closestPoint)
			{
				// Ref : https://icodebroker.tistory.com/5771
				float dx = lineEndPoint.x - listStartPoint.x;
				float dy = lineEndPoint.y - listStartPoint.y;

				if ((dx == 0) && (dy == 0))
				{
					closestPoint = listStartPoint;

					dx = point.x - listStartPoint.x;
					dy = point.y - listStartPoint.y;

					return Mathf.Sqrt(dx * dx + dy * dy);
				}

				float t = ((point.x - listStartPoint.x) * dx + (point.y - listStartPoint.y) * dy) / (dx * dx + dy * dy);

				if (t < 0)
				{
					closestPoint = new Vector2(listStartPoint.x, listStartPoint.y);

					dx = point.x - listStartPoint.x;
					dy = point.y - listStartPoint.y;
				}
				else if (t > 1)
				{
					closestPoint = new Vector2(lineEndPoint.x, lineEndPoint.y);

					dx = point.x - lineEndPoint.x;
					dy = point.y - lineEndPoint.y;
				}
				else
				{
					closestPoint = new Vector2(listStartPoint.x + t * dx, listStartPoint.y + t * dy);

					dx = point.x - closestPoint.x;
					dy = point.y - closestPoint.y;
				}

				return Mathf.Sqrt(dx * dx + dy * dy);
			}
		}

		// 충돌 관련
		public static class PPhysics
		{
			/// <summary> 점이 선 위에 위치하는지 확인 </summary>
			public static bool InsidePointInLine(Vector2 vecPoint, Vector2 vecLineFrom, Vector2 vecLineTo)
			{
				float fAToB = Mathf.Sqrt(
					(vecLineTo.x - vecLineFrom.x) * (vecLineTo.x - vecLineFrom.x) +
					(vecLineTo.y - vecLineFrom.y) * (vecLineTo.y - vecLineFrom.y));

				float fAtoP = Mathf.Sqrt(
					(vecPoint.x - vecLineTo.x) * (vecPoint.x - vecLineTo.x) + 
					(vecPoint.y - vecLineTo.y) * (vecPoint.y - vecLineTo.y));

				float fPtoB = Mathf.Sqrt(
					(vecLineTo.x - vecPoint.x) * (vecLineTo.x - vecPoint.x) + 
					(vecLineTo.y - vecPoint.y) * (vecLineTo.y - vecPoint.y));

				return fAToB == fAtoP + fPtoB;
			}

			/// <summary> 점이 폴리곤 내부에 있는지 확인 </summary>
			public static bool InsidePointInPolygon(List<Vector2> listPolygon, Vector2 vec2Point)
			{
				// Ref : https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon

				bool result = false;
				int j = listPolygon.Count - 1;
				for (int i = 0; i < listPolygon.Count; i++)
				{
					if (listPolygon[i].y < vec2Point.y && listPolygon[j].y >= vec2Point.y || listPolygon[j].y < vec2Point.y && listPolygon[i].y >= vec2Point.y)
					{
						if (listPolygon[i].x + (vec2Point.y - listPolygon[i].y) / (listPolygon[j].y - listPolygon[i].y) * (listPolygon[j].x - listPolygon[i].x) < vec2Point.x)
						{
							result = !result;
						}
					}
					j = i;
				}
				return result;
			}

			public static bool InsidePolygonInPolygon(List<Vector2> listPolygonSource, List<Vector2> listPolygonOther)
			{
				bool result = true;

				int iCount = listPolygonOther.Count;
				for (int i = 0; i < iCount && result; ++i)
				{
					result = InsidePointInPolygon(listPolygonSource, listPolygonOther[i]);
				}
				return result;
			}

			// http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/   
			/// <summary> 선분끼리의 충돌 위치 획득 </summary>
			public static bool CollideLineLine(Vector2 vec1From, Vector2 vec1To, Vector2 vec2From, Vector2 vec2To, out Vector2 vec2Out)
			{
				double fDenominator = (vec2To.y - vec2From.y) * (vec1To.x - vec1From.x) - (vec2To.x - vec2From.x) * (vec1To.y - vec1From.y);

				double fResultA = ((vec2To.x - vec2From.x) * (vec1From.y - vec2From.y) - (vec2To.y - vec2From.y) * (vec1From.x - vec2From.x)) / fDenominator;
				double fResultB = ((vec1To.x - vec1From.x) * (vec1From.y - vec2From.y) - (vec1To.y - vec1From.y) * (vec1From.x - vec2From.x)) / fDenominator;

				vec2Out = vec1From + (float)fResultA * (vec1To - vec1From);

				return
					float.Epsilon <= fResultA && fResultA <= 1 - float.Epsilon &&
					float.Epsilon <= fResultB && fResultB <= 1 - float.Epsilon;
			}

			/// <summary> 선분, 폴리곤끼리의 충돌 위치 획득 </summary>
			public static bool CollideLinePoly(Vector2 vecSourceFrom, Vector2 vecSourceTo, List<Vector2> listCompare, ref Dictionary<int, Vector2> outdictContacts)
			{
				bool bResult = false;

				int iCount = listCompare.Count - 1;

				for (int i = 0; i < iCount; ++i)
				{
					Vector2 vec2CompareFrom = listCompare[i];
					Vector2 vec2CompareTo = listCompare[i + 1];

					bool isContact = CollideLineLine(vecSourceFrom, vecSourceTo, vec2CompareFrom, vec2CompareTo, out Vector2 vec2Out);

					if (isContact && outdictContacts != null)
					{
						bResult = true;

						if (outdictContacts != null)
						{
							outdictContacts.Add(i, vec2Out);
						}
					}
				}

				return bResult;
			}

			/// <summary> 폴리곤끼리의 충돌 위치 획득 </summary>
			public static bool CollidePolyPoly(List<Vector2> listSource, List<Vector2> listCompare, ref Dictionary<(int, int), Vector2> outdictResultContacts)
			{
				bool bResult = false;

				// 반환값에 따른 할당
				Dictionary<int, Vector2> outdictContacts = null;
				if (outdictResultContacts != null)
				{
					outdictContacts = new Dictionary<int, Vector2>();
				}

				int iCount = listSource.Count - 1;

				for (int i = 0; i < iCount; ++i)
				{
					Vector2 vec2SourceFrom = listSource[i];
					Vector2 vec2SourceTo = listSource[i + 1];

					bool isContact = CollideLinePoly(vec2SourceFrom, vec2SourceTo, listCompare, ref outdictContacts);

					if (isContact && outdictContacts != null)
					{
						bResult = true;

						if (outdictContacts != null)
						{
							foreach (var item in outdictContacts)
							{
								outdictResultContacts.Add((i, item.Key), item.Value);
							}
						}
					}
				}

				return bResult;
			}
		}

		// 비트 연산 관련
		public static class Digit
		{
			public static bool Include(int iSource, int iValue) => 0 < (iSource & iValue);
			public static bool Declude(int iSource, int iValue) => 0 == (iSource & iValue);
			public static int AND(int iSource, int iValue) => iSource & iValue;
			public static int OR(int iSource, int iValue) => iSource | iValue;
			public static int ELSE(int iSource) => ~iSource;
			public static int PICK(int iSource, int iValue) => AND(iSource, ELSE(iValue));
		}

		// UI 관련
		public static class UI
		{
			// 서식있는 텍스트 관련
			public static class RichText
			{
				public static string Bold(string str) => $"<b>{str}</b>";
				public static string Italics(string str) => $"<i>{str}</i>";
				public static string Size(string str, int iSize) => $"<size={iSize}>{str}</size>";
				public static string Color(string str, Color color) => $"<color=##{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
			}
		}

		// 성능 진단 관련
		public static class Diagnostics
		{
			private static bool isInit = false;

			private static List<System.Diagnostics.Stopwatch> listWatch;
			private static int iDepth;

			private static void Init()
			{
				if (true == isInit)
					return;

				isInit = true;

				listWatch = new List<System.Diagnostics.Stopwatch>();
				iDepth = 0;
			}

			public static long CheckTimeMS(Action act)
			{
				Init();

				System.Diagnostics.Stopwatch watch = listWatch.GetSafe(iDepth);

				++iDepth;
				watch.Start();
				act();
				watch.Stop();
				--iDepth;

				return watch.ElapsedMilliseconds;
			}
		}
	}
}

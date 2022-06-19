using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public static class CustomRoutine
{
	private static bool _bIsInit = false;

	private static CustomRoutineManager _Routiner = null;
	private static CustomRoutineManager Routiner
	{
		get
		{
			if (_Routiner == null)
			{
				GameObject goRoutiner = new GameObject("CustomRoutinManager");
				UnityEngine.Object.DontDestroyOnLoad(goRoutiner);

				_Routiner = goRoutiner.AddComponent<CustomRoutineManager>();
				_Routiner.Init();

				_bIsInit = true;

				Application.quitting += Dispose;
			}
			return _Routiner;
		}
	}

	private class CustomRoutineManager : MonoBehaviour, IDisposable
	{
		private Dictionary<int, int> _dictCoroutineIndex;
		private Dictionary<int, Dictionary<int, Coroutine>> _dictManagementCoroutine;
		private Dictionary<float, WaitForSeconds> _dictWaitForSeconds;
		private Dictionary<float, WaitForSecondsRealtime> _dictWaitForSecondsRealtime;

		private int _currentSceneIndex = -1;

		private int NextCoroutineIndex 
		{
			get
			{
				int iIndex = _dictCoroutineIndex.GetDef(_currentSceneIndex);
				return _dictCoroutineIndex.SetSafe(_currentSceneIndex, iIndex + 1);
			}
		}
		private Dictionary<int, Coroutine> CurrentCoroutineDict { get => _dictManagementCoroutine.GetSafe(_currentSceneIndex); }

		public void Init()
		{
			_dictCoroutineIndex = new Dictionary<int, int>();
			_dictManagementCoroutine = new Dictionary<int, Dictionary<int, Coroutine>>();
			_dictWaitForSeconds = new Dictionary<float, WaitForSeconds>();
			_dictWaitForSecondsRealtime = new Dictionary<float, WaitForSecondsRealtime>();

			_currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

			SceneManager.sceneLoaded += sceneLoaded;
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= sceneLoaded;
		}

		private void sceneLoaded(Scene stScene, LoadSceneMode eLoadSceneMode)
		{
			StopSceneRoutine(_currentSceneIndex);
			_currentSceneIndex = stScene.buildIndex;
		}

		private void StopSceneRoutine(int index = -1)
		{
			if (index < 0)
				index = _currentSceneIndex;

			Dictionary<int, Coroutine> dictManagementCoroutine = _dictManagementCoroutine.GetDef(index);
			if (dictManagementCoroutine != null)
			{
				foreach (var sceneRoutine in _dictManagementCoroutine[index])
					StopCoroutine(sceneRoutine.Value);
			}
		}

		public bool StopRoutine(int index)
		{
			Coroutine c = CurrentCoroutineDict.GetDef(index);

			if (null == c)
				return false;

			StopCoroutine(c);
			CurrentCoroutineDict.Remove(index);

			return true;
		}

		public int CallLate(float delay, Action actCallback, bool isRealtime = false)
		{
			int routineIndex = NextCoroutineIndex;

			Coroutine c = StartCoroutine(DoCallLate(delay, actCallback, isRealtime, routineIndex));
			CurrentCoroutineDict.Add(routineIndex, c);

			return routineIndex;
		}

		private IEnumerator DoCallLate(float delay, Action actCallback, bool isRealtime, int routineIndex)
		{
			int nowCurrentScene = _currentSceneIndex;

			if (isRealtime)
			{
				WaitForSecondsRealtime wfsObject = _dictWaitForSecondsRealtime.GetDef(delay);
				yield return wfsObject != null ?
					wfsObject : _dictWaitForSecondsRealtime.SetSafe(delay, new WaitForSecondsRealtime(delay));
			}
			else
			{
				WaitForSeconds wfsObject = _dictWaitForSeconds.GetDef(delay);
				yield return wfsObject != null ?
					wfsObject : _dictWaitForSeconds.SetSafe(delay, new WaitForSeconds(delay));
			}

			if (nowCurrentScene == _currentSceneIndex)
			{
				actCallback();
				CurrentCoroutineDict.Remove(routineIndex);
			}
		}

		public int CallLoop(Func<bool> callback, Action actOnEnd = null)
		{
			int routineIndex = NextCoroutineIndex;

			Coroutine c = StartCoroutine(DoCallLoop(callback, actOnEnd, routineIndex));
			CurrentCoroutineDict.Add(routineIndex, c);

			return routineIndex;
		}

		private IEnumerator DoCallLoop(Func<bool> callback, Action actOnEnd, int routineIndex)
		{
			int nowCurrentScene = _currentSceneIndex;

			while (true)
			{
				if (nowCurrentScene != _currentSceneIndex || !callback())
					break;

				yield return null;
			}

			if (nowCurrentScene == _currentSceneIndex)
			{
				actOnEnd?.Invoke();
				CurrentCoroutineDict.Remove(routineIndex);
			}
		}

		public int CallLoopAnyScene(Func<bool> callback, Action actOnEnd = null)
		{
			int routineIndex = NextCoroutineIndex;

			Coroutine c = StartCoroutine(DoCallLoopAnyScene(callback, actOnEnd, routineIndex));
			CurrentCoroutineDict.Add(routineIndex, c);

			return routineIndex;
		}

		private IEnumerator DoCallLoopAnyScene(Func<bool> callback, Action actOnEnd, int routineIndex)
		{
			while (true)
			{
				if (!callback())
					break;

				yield return null;
			}

			actOnEnd?.Invoke();
			CurrentCoroutineDict.Remove(routineIndex);
		}

		public int CallInTime(float time, Action<float> callback, Action actOnEnd = null)
		{
			int routineIndex = NextCoroutineIndex;

			Coroutine c = StartCoroutine(DoCallInTime(time, callback, actOnEnd, routineIndex));
			CurrentCoroutineDict.Add(routineIndex, c);

			return routineIndex;
		}

		private IEnumerator DoCallInTime(float time, Action<float> callback, Action actOnEnd, int routineIndex)
		{
			int nowCurrentScene = _currentSceneIndex;

			float fTimeSour = Time.time;
			float fTimeDest = Time.time + time;

			while (true)
			{
				float fTimeNow = Time.time;
				if (fTimeDest < fTimeNow)
					break;

				callback((fTimeNow - fTimeSour) / time);

				yield return null;
			}

			if (nowCurrentScene == _currentSceneIndex)
			{
				actOnEnd?.Invoke();
				CurrentCoroutineDict.Remove(routineIndex);
			}
		}
	}

	public static void Init()
	{
		if (false == _bIsInit)
		{
			if (_Routiner == null)
			{
				CustomRoutineManager refManager = Routiner;
			}
		}
	}

	public static void Dispose()
	{
		if (_bIsInit && null != _Routiner)
		{
			_Routiner.Dispose();
		}
	}

	public static bool Stop(int routineIndex) => Routiner.StopRoutine(routineIndex);
	public static int CallLate(float delay, Action actCallback, bool isRealtime = false) => Routiner.CallLate(delay, actCallback, isRealtime);
	public static int CallLoop(Func<bool> callback, Action OnEnd = null) => Routiner.CallLoop(callback, OnEnd);
	public static int CallLoopAnyScene(Func<bool> callback, Action OnEnd = null) => Routiner.CallLoopAnyScene(callback, OnEnd);
	public static int CallInTime(float time, Action<float> actCallback, Action actOnEnd = null) => Routiner.CallInTime(time, actCallback, actOnEnd);

	// public static int Pause(float Second, bool isRealtime = true)
	// {
	// 	float prevTime = Time.timeScale;
	// 	Time.timeScale = 0.0f;
	// 
	// 	return Routiner.CallLate(Second, () => Time.timeScale = prevTime, isRealtime);
	// }
} //
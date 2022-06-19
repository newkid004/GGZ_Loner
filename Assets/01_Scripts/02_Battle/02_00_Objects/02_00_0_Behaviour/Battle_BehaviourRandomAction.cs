using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GGZ
{
	[CustomEditor(typeof(Battle_BehaviourRandomAction))]
	public class E_Battle_BehaviourRandomAction : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Reimport Time Interval Accumulate"))
			{
				Battle_BehaviourRandomAction scReimporter = (Battle_BehaviourRandomAction)target;
				scReimporter.ReimportTimeIntervalAccumulate();
			}

			base.OnInspectorGUI();
		}
	}

	public class Battle_BehaviourRandomAction : Battle_BaseBehaviour
	{
		[System.Serializable]
		public struct stTimeInterval
		{
			public float fMoveTime;

			[Range(0.0f, 1.0f)]
			public float fRandomRange;

			public int iWeight;

			[HideInInspector]
			public float fAccumulate;
		}

		[Header("----- RandomAction -----")]

		[SerializeField]
		protected List<stTimeInterval> listActionTimer = new List<stTimeInterval>();

		public void ReimportTimeIntervalAccumulate()
		{
			float fAccumulate = 0;
			for (int i = 0; i < listActionTimer.Count; ++i)
			{
				stTimeInterval timer = listActionTimer[i];

				fAccumulate += timer.fMoveTime;
				timer.fAccumulate = fAccumulate;

				listActionTimer[i] = timer;
			}
		}
	}
}
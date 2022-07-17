using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace Proto_00_N
{
	public class Loading_PageCSVLoading : Loading_PageBase
	{
		[System.Serializable]
		public struct stInputCsvData
		{
			public string strPath;
			public List<string> listFile;
		}

		[SerializeField] private List<stInputCsvData> listInput;

		private Dictionary<string, Type> dictInputType;
		private int iNeedComplateCount;

		public override void ProcessLoad()
		{
			base.ProcessLoad();

			InitComplateCondition();
			InitInputReflection();

			LoadDataCSV();
		}

		private void InitComplateCondition()
		{
			iNeedComplateCount = 0;
			listInput.ForEach(data => iNeedComplateCount += data.listFile.Count );
		}

		private void InitInputReflection()
		{
			dictInputType = new Dictionary<string, Type>();
			InputCSVType(typeof(CSVData), typeof(CSVData).GetNestedTypes(BindingFlags.Public));
		}

		private void InputCSVType(Type t, Type[] arrInner)
		{
			if (arrInner.Length == 0)
			{
				dictInputType.Add(t.ToString(), t);
			}
			else
			{
				foreach (Type tInner in arrInner)
				{
					InputCSVType(tInner, tInner.GetNestedTypes(BindingFlags.Public));
				}
			}
		}

		private void LoadDataCSV()
		{
			string strBasicPath = "R_02_CSV";
			Type tRaw = typeof(CSVObjectBase<>);

			ResourceRequest resReq;

			listInput.ForEach(data =>
			{
				data.listFile.ForEach(strCSV =>
				{
					// Type È¹µæ
					string strKey = $"Proto_00_N.CSVData+{data.strPath.Replace("/", "+")}+{strCSV}";
					Type tMatch = dictInputType.GetDef(strKey);
#if _debug
					if (tMatch == null)
					{
						Debug.LogAssertion($"Loading_PageCSVLoading.LoadDataCSV : Type is Not Defined ({strCSV})");
					}
#endif
					resReq = CSVReader.ReadAsync($"{strBasicPath}/{data.strPath}/{strCSV}", (dictResource) =>
					{
						// ID ÇÊµå Å¸ÀÔ È¹µæ
						FieldInfo fiID = tMatch.GetField("ID", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

						// Generic ÀÔ·Â
						Type tGen = tRaw.MakeGenericType(tMatch);

						// Manager Type È¹µæ
						Type tManager = tGen.GetNestedTypes(BindingFlags.Public)[0];
						Type tGenManager = tManager.MakeGenericType(tMatch, fiID.FieldType);

						// Init ÇÔ¼ö È¹µæ ¹× È£Ãâ
						tGenManager.GetMethod("Init").Invoke(null, new object[] { dictResource });
					});

					// Request ¿Ï·á ½Ã ·Îµù¿Ï·á È®ÀÎ
					resReq.completed += (asyncOper) =>
					{
						if (Interlocked.Decrement(ref iNeedComplateCount) == 0)
						{
							ProcessLoadComplate();
						}
					};
				});
			});
		}

		public void InputLoadResourcePath(Dictionary<string, List<string>> dictPath)
		{
			listInput.Clear();

			dictPath.ForEach((key, value) =>
			{
				listInput.Add(new stInputCsvData()
				{
					strPath = key,
					listFile = value
				});
			});
		}
	}
}
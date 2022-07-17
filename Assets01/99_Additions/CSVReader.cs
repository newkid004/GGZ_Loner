
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Ref : https://pastebin.com/7XCA2UDD
public class CSVReader
{
	static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
	static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
	static char[] TRIM_CHARS = { '\"' };

	static string LINE_SPLIT_TOCKEN = "|";
	static string LINE_MATCH_STRING_RE = "[^0-9.|]";

	public static List<Dictionary<string, object>> Read(string file)
	{
		TextAsset ta = Resources.Load(file) as TextAsset;

#if _debug
		if (ta == null)
		{
			Debug.LogAssertion($"CSVReader.Read : Invalid File Path ({file})");
			return null;
		}
#endif

		return GetObjToTextAsset(ta);
	}

	public static ResourceRequest ReadAsync(string file, Action<List<Dictionary<string, object>>> actOnEnd)
	{
		var list = new List<Dictionary<string, object>>();
		var resReq = Resources.LoadAsync(file);

#if _debug
		if (resReq == null)
		{
			Debug.LogAssertion($"CSVReader.ReadAsync : Invalid File Path ({file})");
			return null;
		}
#endif

		resReq.completed += (oper) =>
		{
			if (oper.isDone)
			{
				actOnEnd(GetObjToTextAsset(resReq.asset as TextAsset));
			}
		};

		return resReq;
	}

	private static List<Dictionary<string, object>> GetObjToTextAsset(TextAsset data)
	{
		var list = new List<Dictionary<string, object>>();

		var lines = Regex.Split(data.text, LINE_SPLIT_RE);

		if (lines.Length <= 1) return list;

		var header = Regex.Split(lines[0], SPLIT_RE);
		for (var i = 1; i < lines.Length; i++)
		{
			var values = Regex.Split(lines[i], SPLIT_RE);
			if (values.Length == 0 || values[0] == "") continue;

			var entry = new Dictionary<string, object>();
			for (var j = 0; j < header.Length && j < values.Length; j++)
			{
				string value = values[j];
				value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
				object finalvalue = value;
				int n;
				float f;
				if (int.TryParse(value, out n))
				{
					finalvalue = n;
				}
				else if (float.TryParse(value, out f))
				{
					finalvalue = f;
				}
				else
				{
					if (value.Contains(LINE_SPLIT_TOCKEN))
					{
						bool isManyArray = true;
						string[] strSplit = value.Split(LINE_SPLIT_TOCKEN.ToCharArray());

						if (strSplit.Length == 2)
						{
							if (strSplit[0] == string.Empty)
							{
								if (int.TryParse(strSplit[1], out n))
								{
									finalvalue = new int[] { n };
								}
								else if (float.TryParse(strSplit[1], out f))
								{
									finalvalue = new float[] { f };
								}
								isManyArray = false;
							}
						}

						if (isManyArray)
						{
							if (Regex.IsMatch(value, LINE_MATCH_STRING_RE))
							{
								finalvalue = strSplit;
							}
							else if (value.Contains("."))
							{
								float[] floatSplit = new float[strSplit.Length];

								for (int k = 0; k < strSplit.Length; ++k)
								{
									float.TryParse(strSplit[k], out floatSplit[k]);
								}

								finalvalue = floatSplit;
							}
							else
							{
								int[] intSplit = new int[strSplit.Length];

								for (int k = 0; k < strSplit.Length; ++k)
								{
									int.TryParse(strSplit[k], out intSplit[k]);
								}

								finalvalue = intSplit;
							}
						}
					}
				}
				entry[header[j]] = finalvalue;
			}
			list.Add(entry);
		}
		return list;
	}
}


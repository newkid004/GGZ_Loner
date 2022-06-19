using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace GGZ
{
	[Serializable]
	public abstract class SaveMultiObjectBase<T> where T : SaveMultiObjectBase<T>, new()
	{
		public virtual string strPathSave => $"{Application.persistentDataPath}/save";
		public virtual string strFileExtension => "sav";

		public string GetFilePath(string strName) => string.Format($"{strPathSave}/{typeof(T).Name}/" + "{0}" + $".{strFileExtension}", strName);

		public static void Save(string strName, ref T tObject)
		{
			var bf = new BinaryFormatter();

			string strDirectoryPath = $"{tObject.strPathSave}/{typeof(T).Name}/";
			if (false == Directory.Exists(strDirectoryPath))
			{
				Directory.CreateDirectory(strDirectoryPath);
			}

			using (FileStream fs = File.Create(tObject.GetFilePath(strName)))
			{
				bf.Serialize(fs, tObject);
			}
		}

		public static bool Load(string strName, ref T tObject)
		{
			if (!File.Exists(tObject.GetFilePath(strName)))
			{
				return false;
			}

			var bf = new BinaryFormatter();
			using (FileStream fs = File.Open(tObject.GetFilePath(strName), FileMode.Open))
			{
				tObject = (T)bf.Deserialize(fs);
			}

			return true;
		}
	}
}
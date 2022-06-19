using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace GGZ
{
	[Serializable]
	public abstract class SaveSingleObjectBase<T> where T : SaveSingleObjectBase<T>, new()
	{
		#region Singleton
		protected SaveSingleObjectBase() { }

		private static T _Main;

		public static T Main
		{
			get
			{
				if (_Main == null)
				{
					_Main = new T();
				}

				return _Main;
			}
		}

		public void JustCall() { }
		#endregion

		public virtual string strPathSave => $"{Application.persistentDataPath}/save";
		public virtual string strFileExtension => "sav";

		public string strFilePath => $"{strPathSave}/{typeof(T).Name}.{strFileExtension}";

		public void Save(string strPath)
		{
			var bf = new BinaryFormatter();

			if (strPath == null)
				strPath = strFilePath;

			if (false == Directory.Exists(strPathSave))
			{
				Directory.CreateDirectory(strPathSave);
			}

			using (FileStream fs = File.Create(strPath))
			{
				bf.Serialize(fs, Main);
			}
		}

		public bool Load(string strPath)
		{
			if (strPath == null)
				strPath = strFilePath;

			if (!File.Exists(strPath))
			{
				return false;
			}

			var bf = new BinaryFormatter();

			try
			{
				using (FileStream fs = File.Open(strPath, FileMode.Open))
				{
					_Main = (T)bf.Deserialize(fs);
				}
			}
			catch
			{
				Save(strPath);
				Load(strPath);
			}

			return true;
		}
	}
}
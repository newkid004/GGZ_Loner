using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Proto_00_N
{
	[Serializable]
	public abstract class SaveObjectBase<T> where T : SaveObjectBase<T>, new()
	{
#region Singleton
		protected SaveObjectBase() { }

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

		public static string strFilePath => $"{SaveData.strPathSave}/{typeof(T).Name}.{SaveData.strFileExtension}";

		// public T Export()
		// {
		// 	T obj = new T();
		// 	obj.Import((T)this);
		// 	return obj;
		// }
		// 
		// public void Import(T obj)
		// {
		// 	FieldInfo[] arrFI = typeof(T).GetFields(
		// 		BindingFlags.Public
		// 		| BindingFlags.NonPublic
		// 		| BindingFlags.Instance
		// 		| BindingFlags.Static);
		// 
		// 	foreach (FieldInfo fi in arrFI)
		// 	{
		// 		fi.SetValue(this, fi.GetValue(obj));
		// 	}
		// }

		public void Save()
		{
			var bf = new BinaryFormatter();

			Directory.CreateDirectory(SaveData.strPathSave);
			using (FileStream fs = File.Create(strFilePath))
			{
				bf.Serialize(fs, Main);
			}
		}

		public bool Load()
		{
			if (!File.Exists(strFilePath))
			{
				return false;
			}

			var bf = new BinaryFormatter();
			using (FileStream fs = File.Open(strFilePath, FileMode.Open))
			{
				_Main = (T)bf.Deserialize(fs);
			}

			return true;
		}
	}
}
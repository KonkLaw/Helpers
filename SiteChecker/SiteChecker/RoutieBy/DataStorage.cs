using System;
using System.IO;
using System.Xml.Serialization;

namespace SiteChecker
{
	class DataStorage
	{
		public static void WriteData<T>(T creadentials, string fileName) where T : class
		{
			var serializer = new XmlSerializer(typeof(T));
			using (var file = new FileStream(fileName, FileMode.Create))
			{
				serializer.Serialize(file, creadentials);
			}
		}

		public static bool TryGetDataOrSaveDefautl<T>(out T credentials, string fileName, Func<T> defaultCreator) where T : class
		{
			var serializer = new XmlSerializer(typeof(T));
			if (!File.Exists(fileName))
			{
				if (defaultCreator == null)
					credentials = default;
				else
				{
					credentials = defaultCreator();
					WriteData<T>(credentials, fileName);
				}
				return false;
			}
			using (FileStream file = File.Open(fileName, FileMode.Open))
			{
				credentials = (T)serializer.Deserialize(file);
				return true;
			}
		}
	}
}
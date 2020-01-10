using System.Globalization;
using System.Text;

namespace RouteByApi
{
	static class StringHelper
	{
		private readonly static StringBuilder cache = new StringBuilder();

		public static string DecodeUnicide(this string input)
		{
			const string pattern = @"\u";
			const int partLength = 4;

			int currentIndex = 0;
			do
			{
				int newEncodeIndex = input.IndexOf(pattern, currentIndex);
				if (newEncodeIndex < 0)
				{
					cache.AppendLine(input.Substring(currentIndex, input.Length - currentIndex));
					break;
				}
				else
				{
					cache.Append(input, currentIndex, newEncodeIndex - currentIndex);
				}

				string sign = input.Substring(newEncodeIndex + pattern.Length, partLength);
				int charData;
				bool parsedSuccessfully = int.TryParse(sign,
						NumberStyles.HexNumber,
						CultureInfo.CurrentCulture,
						out charData);
				cache.Append((char)charData);

				currentIndex = newEncodeIndex + pattern.Length + partLength;
			} while (currentIndex < input.Length);
			string result = cache.ToString();
			cache.Clear();
			return result;
		}
	}
}

using System.Globalization;
using System.Text;

namespace AtlasbusByApi
{
	static class StringHelper
	{
		private readonly static StringBuilder Cache = new StringBuilder();

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
					Cache.AppendLine(input[currentIndex..]);
					break;
				}
				else
				{
					Cache.Append(input, currentIndex, newEncodeIndex - currentIndex);
				}

				string sign = input.Substring(newEncodeIndex + pattern.Length, partLength);
				bool parsedSuccessfully = int.TryParse(sign,
						NumberStyles.HexNumber,
						CultureInfo.CurrentCulture,
						out int charData);
				Cache.Append((char)charData);

				currentIndex = newEncodeIndex + pattern.Length + partLength;
			} while (currentIndex < input.Length);
			string result = Cache.ToString();
			Cache.Clear();
			return result;
		}

		public static string? ExtractValue(this string input, string key)
		{
			int index = input.IndexOf(key);
			if (index < 0)
				return null;
			index = input.IndexOf(':', index);
			if (index < 0)
				return null;
			bool isStringValue = input[index + 1] == '"';

			int indexEnd;
			do
			{
				indexEnd = input.IndexOf(',', index);
			} while (indexEnd >= 0 && isStringValue && input[index - 1] == '\\');

			if (indexEnd < 0)
			{
				do
				{
					indexEnd = input.IndexOf('}', index);
				}
				while (indexEnd >= 0 && isStringValue && input[index - 1] == '\\');
			}
			if (indexEnd < 0)
				return null;
			return input[(index + 1)..indexEnd];
		}
	}
}

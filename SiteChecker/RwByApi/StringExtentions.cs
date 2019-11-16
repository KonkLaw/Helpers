namespace RwByApi
{
	static class StringExtentions
	{
		public static int IndexOfEnd(this string content, string value, int indexOfStart)
		{
			int index = content.IndexOf(value, indexOfStart);
			if (index < 0)
				return index;
			return index + value.Length;
		}

		public static int IndexOfEnd(this string content, string value, int indexOfStart, int count)
		{
			int index = content.IndexOf(value, indexOfStart, count);
			if (index < 0)
				return index;
			return index + value.Length;
		}
	}
}

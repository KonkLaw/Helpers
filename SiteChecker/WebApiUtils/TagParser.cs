using System;
using System.Collections.Generic;

namespace WebApiUtils
{
    public class JsonParser
    {
        private readonly string openTang = "{";
        private readonly string closedTag = "}";
        private readonly string StartValueTag = "\"";
        private readonly string StopValueTag = "\"";
        private readonly string ValueSeparator = ",";

        public object GetStructure(string text)
        {
            DoRecursion(text, 0);
            return null;
        }

        private void DoRecursion(string text, int offsetIndex)
        {
            int startIndex = text.IndexOfEnd(openTang, offsetIndex);

            var qwe = new List<object>();
            ParseValues(qwe, text, startIndex);

        }

        private void ParseValues(List<object> list, string text, int startIndex)
        {
            int index = startIndex;
            while (true)
            {
                index = text.IndexOf(StartValueTag, index);
                if (index < 0)
                    break;
                index += StartValueTag.Length;
                int stopIndex = text.IndexOfEnd(StopValueTag, index);
                stopIndex -= StopValueTag.Length;
                string valueName = text.SubstringStop(index, stopIndex);
                index = stopIndex + StopValueTag.Length + 1; // for ':'

                int? indexOfValueSeparator = text.IndexOfWithNull(ValueSeparator, index);
                int? indexOfTagStart = text.IndexOfWithNull(openTang, index);
                int? indexOfTagClose = text.IndexOfWithNull(closedTag, index);

                if (indexOfValueSeparator < indexOfTagStart)
                {
                    string valueContent = text.SubstringStop(index, indexOfValueSeparator);
                    var value = new Leaf(valueName, valueContent);
                    list.Add(value);
                    index = indexOfValueSeparator + ValueSeparator.Length;
                }
                else
                {
                    var newList = new List<object>();
                    ParseValues(newList, text, indexOfTagStart);
                    var b = new Branch(valueName, newList);
                }
            }
        }

        private static int GetScNumber(int? indexOfValueSeparator, int? indexOfTagStart, int? indexOfTagClose)
        {
            if (
                indexOfValueSeparator.HasValue &&
                (!indexOfTagStart.HasValue || indexOfValueSeparator.Value < indexOfTagStart.Value) &&
                (!indexOfTagClose.HasValue || indexOfValueSeparator.Value < indexOfTagClose.Value))
            {
                return 0;
            }
            else if ()
        }
    }

    class Leaf
    {
        public string ValueName;
        public string ValueContent;

        public Leaf(string valueName, string valueContent)
        {
            ValueName = valueName;
            ValueContent = valueContent;
        }
    }

    class Branch
    {
        public string ValueName;
        private List<object> newList;

        public Branch(string valueName, List<object> newList)
        {
            ValueName = valueName;
            this.newList = newList;
        }
    }

    public static class StringExt
    {
        // Not correct in the end of string.
        public static int IndexOfEnd(this string value, string substring, int startIndex)
        {
            return value.IndexOf(substring, startIndex) + substring.Length;
        }

        public static string SubstringStop(this string value, int startIndex, int stopIndex)
            => value.Substring(startIndex, stopIndex - startIndex);

        public static int? IndexOfWithNull(this string value, string substring, int startIndex)
        {
            int index = value.IndexOf(substring, startIndex);
            if (index < 0)
                return null;
            else
                return index;
        }
    }
}

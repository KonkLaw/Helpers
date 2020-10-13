using System;
using System.Collections.Generic;

namespace WebApiUtils
{
    public class JsonParser
    {
        private const string OpenTang = "{";
        private const string ClosedTag = "}";
        private const string StartValueTag = "\"";
        private const string StopValueTag = "\"";
        private const string ValueSeparator = ",";

        public object GetStructure(string text)
        {
            DoRecursion(text, 0);
            return null;
        }

        private void DoRecursion(string text, int offsetIndex)
        {
            int startIndex = text.IndexOfEnd(OpenTang, offsetIndex);

            var qwe = new List<object>();
            ParseValues(qwe, text, startIndex);

        }

        private int ParseValues(List<object> list, string text, int startIndex)
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
                int? indexOfTagStart = text.IndexOfWithNull(OpenTang, index);
                int? indexOfTagClose = text.IndexOfWithNull(ClosedTag, index);


                if (indexOfValueSeparator.HasValue &&
                    (!indexOfTagStart.HasValue || indexOfValueSeparator.Value < indexOfTagStart.Value) &&
                    (!indexOfTagClose.HasValue || indexOfValueSeparator.Value < indexOfTagClose.Value))
                {
                    // next value after separator
                    string valueContent = text.SubstringStop(index, indexOfValueSeparator.Value);
                    var value = new Leaf(valueName, valueContent);
                    list.Add(value);
                    index = indexOfValueSeparator.Value + ValueSeparator.Length;

                }
                else if (indexOfTagStart.HasValue &&
                    (!indexOfValueSeparator.HasValue || indexOfTagStart.Value < indexOfValueSeparator.Value) &&
                    (!indexOfTagClose.HasValue || indexOfTagStart.Value < indexOfTagClose.Value))
                {
                    var newList = new List<object>();
                    index = ParseValues(newList, text, indexOfTagStart.Value);
                    index += ClosedTag.Length;
                    var branch = new Branch(valueName, newList);
                    list.Add(branch);
                }
                else if (indexOfTagClose.HasValue &&
                    (!indexOfValueSeparator.HasValue || indexOfTagClose.Value < indexOfValueSeparator.Value) &&
                    (!indexOfTagStart.HasValue || indexOfTagClose.Value < indexOfTagStart.Value))
                {
                    string valueContent = text.SubstringStop(index, indexOfTagClose.Value);
                    var value = new Leaf(valueName, valueContent);
                    list.Add(value);
                    index = indexOfTagClose.Value + ClosedTag.Length;
                    break; // end of inner tag.
                }
                else
                    throw new Exception();
            }
            return index;
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

using System;
using System.Collections.Generic;

namespace WebContentParser
{
	readonly struct NodeParamemters
	{
		public readonly string Text;
		public readonly int StartIndex;
		public readonly int EndIndex;
		public readonly int BodyStartIndex;
		public readonly int BodyEndIndex;

		public NodeParamemters(string text, int startIndex, int endIndex)
		{
			Text = text;
			StartIndex = startIndex;
			EndIndex = endIndex;
			BodyStartIndex = -1;
			BodyEndIndex = -1;
		}

		public NodeParamemters(string text, int startIndex, int endIndex, int bodyStartIndex, int bodyEndIndex)
		{
			Text = text;
			StartIndex = startIndex;
			EndIndex = endIndex;
			BodyStartIndex = bodyStartIndex;
			BodyEndIndex = bodyEndIndex;
		}
	}

	class Node
	{
		private readonly NodeParamemters paramemters;
		private List<Node>? listOrNot;
		public List<Node>? ListOrNot => listOrNot;

		private Node(NodeParamemters paramemters)
		{
			this.paramemters = paramemters;
		}

		public static Node Craete() { return new Node(new NodeParamemters()); }

		public Node Add(in NodeParamemters paramemters)
		{
			var node = new Node(paramemters);
			listOrNot ??= new List<Node>();
			listOrNot.Add(node);
			return node;
		}

		public void GetText(out string tagContext, out string? tagBody)
		{
			tagContext = paramemters.Text[paramemters.StartIndex..paramemters.EndIndex];
			if (paramemters.BodyEndIndex > 0)
			{
				tagBody = paramemters.Text[paramemters.BodyStartIndex..paramemters.BodyEndIndex];
			}
			else
				tagBody = default;
		}
	}

	static class TagParser
	{
		const string TagStart = "<";
		const string CloseTag = ">";
		const string Tag = "<div ";
		const string ClosingTag = "</div>";

		public static Node Parse(string text)
		{
			Node rootNode = Node.Craete();
			Rуc(rootNode, text, 0);
			return rootNode;
		}

		private static int Rуc(Node parentNode, string text, int currentIndex)
		{
			bool openTagNotClose = true;
			while (openTagNotClose)
			{
				int startTagIndex = text.IndexOf(Tag, currentIndex);
				startTagIndex += Tag.Length;
				currentIndex = startTagIndex;
				int endTagIndex = text.IndexOf(CloseTag, currentIndex);
				currentIndex = endTagIndex;
				FindNextTagIndex(text, ref currentIndex, out openTagNotClose);
				if (openTagNotClose)
				{
					Node node = parentNode.Add(
						new NodeParamemters(text, startTagIndex, endTagIndex));
					currentIndex = Rуc(node, text, currentIndex);
				}
				else
				{
					parentNode.Add(new NodeParamemters(
						text, startTagIndex, endTagIndex, endTagIndex + 1, currentIndex));
					currentIndex++;
				}
				FindNextTagIndex(text, ref currentIndex, out openTagNotClose);
				if (currentIndex < 0)
					return -1;
			}
			return currentIndex + 1;
		}

		private static void FindNextTagIndex(
			string text, ref int currentIndex, out bool openTagNotClose)
		{
			while (true)
			{
				currentIndex = text.IndexOf(TagStart, currentIndex);
				if (currentIndex < 0)
				{
					openTagNotClose = false;
					currentIndex = -1;
					return;
				}
				ReadOnlySpan<char> span = text[currentIndex..];
				if (span.StartsWith(Tag))
				{
					openTagNotClose = true;
					break;
				}
				else if (span.StartsWith(ClosingTag))
				{
					openTagNotClose = false;
					break;
				}
				else
				{
					currentIndex++;
					continue;
				}
			}
		}
	}
}

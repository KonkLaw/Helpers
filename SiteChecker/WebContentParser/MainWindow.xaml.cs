using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebContentParser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Node rootNode = TagParser.Parse(TextBox.Text);
			Tree.ItemsSource = Process(rootNode);
		}

		private IEnumerable<NodeModel> Process(Node rootNode)
		{
			var rootList = new List<NodeModel>();
			foreach (Node child in rootNode.ListOrNot!)
			{
				rootList.Add(GetNode(child));
			}
			return rootList;
		}

		private NodeModel GetNode(Node node)
		{
			string resultString;
			node.GetText(out string tagContent, out string? tagBody);
			if (tagBody == null)
			{
				resultString = tagContent;
			}
			else
			{
				resultString = tagContent + " |+| " + tagBody;
			}

			if (node.ListOrNot == null)
			{
				return new NodeModel(null, resultString);
			}
			else
			{
				List<NodeModel> nodeModels = new List<NodeModel>(node.ListOrNot.Count);
				foreach (Node child in node.ListOrNot)
				{
					nodeModels.Add(GetNode(child));
				}
				return new NodeModel(nodeModels, resultString);
			}
		}
	}

	class NodeModel : BindableBase
	{
		public List<NodeModel>? Children { get; }

		public string TagContent { get; }

		public NodeModel(List<NodeModel>? children, string tagContent)
		{
			Children = children;
			TagContent = tagContent;
		}
	}
}

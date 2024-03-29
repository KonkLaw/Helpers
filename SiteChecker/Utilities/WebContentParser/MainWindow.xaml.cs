﻿using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows;

namespace WebContentParser;

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
        if (string.IsNullOrEmpty(TextBox.Text))
            return;
        Node rootNode = TagParser.Parse(TextBox.Text);
        Tree.ItemsSource = ConverToViewModels(rootNode);
    }

    private IEnumerable<NodeModel> ConverToViewModels(Node rootNode)
    {
        var rootList = new List<NodeModel>();
        foreach (Node child in rootNode.ListOrNot!)
        {
            rootList.Add(ConvertNode(child));
        }
        return rootList;
    }

    private NodeModel ConvertNode(Node node)
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
            var nodeModels = new List<NodeModel>(node.ListOrNot.Count);
            foreach (Node child in node.ListOrNot)
            {
                nodeModels.Add(ConvertNode(child));
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
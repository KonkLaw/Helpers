using Notifier.UtilTypes;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Notifier;

class NavigationViewModel : BindableBase
{
    private object? currentPage;
    public object? CurrentPage
    {
        get => currentPage;
        private set => SetProperty(ref currentPage, value);
    }

    private bool isOnWaiting;
    public bool IsOnWaiting
    {
        get => isOnWaiting;
        set => SetProperty(ref isOnWaiting, value);
    }

    private readonly Dictionary<Type, UserControl> mapping = new Dictionary<Type, UserControl>();

    public void DeclareMapping<TViewModel>(UserControl view)
    {
        Type viewmodelType = typeof(TViewModel);
        if (mapping.ContainsKey(viewmodelType))
        {
            mapping[viewmodelType] = view;
        }
        else
        {
            mapping.Add(viewmodelType, view);
        }
    }

    public void Show(object viewmodel)
    {
        mapping.TryGetValue(viewmodel.GetType(), out UserControl? view);
        if (view == null)
        {
            throw new InvalidOperationException();
        }
        view.DataContext = viewmodel;
        CurrentPage = view;
    }
}
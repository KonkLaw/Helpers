using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Notifier
{
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
			Type key = viewmodel.GetType();
			if (!mapping.ContainsKey(key))
				throw new InvalidOperationException();
			UserControl view = mapping[key];
			view.DataContext = viewmodel;
			CurrentPage = view;
		}
	}
}
 
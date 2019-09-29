using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Notifier
{
    class NavigationViewModel : BindableBase
	{
		private object curentPage;
		public object CurentPage
		{
			get => curentPage;
			private set => SetProperty(ref curentPage, value);
		}

		public object CurrentPageViewmodel { get; private set; }

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
            if (!mapping.TryGetValue(viewmodel.GetType(), out UserControl view))
            {
                throw new InvalidOperationException();
            }
			view.DataContext = viewmodel;
			CurentPage = view;
			CurrentPageViewmodel = viewmodel;
		}
	}
}
 
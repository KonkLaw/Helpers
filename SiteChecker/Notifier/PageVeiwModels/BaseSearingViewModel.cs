using Prism.Commands;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Notifier.PageVeiwModels
{
	abstract class BaseSearingViewModel : BasePageViewModel
	{
		protected readonly NavigationViewModel NavigaionViewModel;
		protected readonly CancellationTokenSource CancelationSource = new CancellationTokenSource();

		public ICommand LinkCommand { get; }
		public ICommand CancelCommand { get; }

		private string message;
		public string Message
		{
			get => message;
			set => SetProperty(ref message, value);
		}

		protected BaseSearingViewModel(NavigationViewModel navigaionViewModel)
		{
			NavigaionViewModel = navigaionViewModel;
			CancelCommand = new DelegateCommand(CancelHandler);
			LinkCommand = new DelegateCommand(LinkHandler);
		}

		protected abstract Uri GetLink();

		protected abstract object GetCancelViewModel();

		private void LinkHandler()
			=> Process.Start(new ProcessStartInfo(GetLink().AbsoluteUri));

		private void CancelHandler()
		{
			CancelationSource.Cancel();
			NavigaionViewModel.Show(GetCancelViewModel());
		}

		protected abstract bool TryFind();

		protected void SearchProcess()
		{
			const int waitTimeoutSeconds = 10;
			while (!CancelationSource.IsCancellationRequested)
			{
				if (TryFind())
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						Message = "Was found at: " + DateTime.Now.ToLongTimeString();
					});
					while (!CancelationSource.IsCancellationRequested)
					{
						System.Media.SystemSounds.Hand.Play();
						Thread.Sleep(2000);
					}
				}
				else
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						Message = "Alive at : " + DateTime.Now.ToLongTimeString();

					});
					Thread.Sleep(waitTimeoutSeconds * 1000);
				}
			}
		}
	}
}
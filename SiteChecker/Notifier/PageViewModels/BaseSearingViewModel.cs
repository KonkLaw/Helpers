using Prism.Commands;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Notifier.PageViewModels
{
	abstract class BaseSearingViewModel : BasePageViewModel
	{
		protected readonly NavigationViewModel NavigationViewModel;
		protected readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

		
		public ICommand TestSound { get; }
		public ICommand LinkCommand { get; }
		public ICommand CancelCommand { get; }

		private string message;
		public string Message
		{
			get => message;
			private set => SetProperty(ref message, value);
		}

		protected BaseSearingViewModel(NavigationViewModel navigationViewModel)
		{
			NavigationViewModel = navigationViewModel;
			CancelCommand = new DelegateCommand(CancelHandler);
			LinkCommand = new DelegateCommand(LinkHandler);
			TestSound = new DelegateCommand(PalySound);
		}

		protected abstract Uri GetLink();

		protected abstract object GetCancelViewModel();

		private void LinkHandler()
			=> Process.Start(new ProcessStartInfo(GetLink().AbsoluteUri));

		private void CancelHandler()
		{
			CancellationSource.Cancel();
			NavigationViewModel.Show(GetCancelViewModel());
		}

		protected abstract bool TryFind(out string goodResultMessage);

		protected void SearchProcess()
		{
			const int waitTimeoutSeconds = 10;
			while (!CancellationSource.IsCancellationRequested)
			{
				if (TryFind(out string goodResultMessage))
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						Message = goodResultMessage;
					});
					while (!CancellationSource.IsCancellationRequested)
					{
						PalySound();
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

		private void PalySound()
		{
			System.Media.SystemSounds.Hand.Play();
		}
	}
}
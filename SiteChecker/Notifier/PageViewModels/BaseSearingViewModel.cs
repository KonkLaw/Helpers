﻿using Notifier.UtilTypes;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Notifier.PageViewModels
{
	abstract class BaseSearingViewModel : BasePageViewModel
	{
		protected readonly NavigationViewModel NavigationViewModel;
		private bool isCanceled = false;
		protected bool IsCanceled => isCanceled;
		
		public ICommand TestSound { get; }
		public ICommand LinkCommand { get; }
		public ICommand CancelCommand { get; }

		private string details = string.Empty;
		public string Details
		{
			get => details;
			private set => SetProperty(ref details, value);
		}

		private string message = string.Empty;
		public string Message
		{
			get => message;
			private set => SetProperty(ref message, value);
		}

		protected BaseSearingViewModel(NavigationViewModel navigationViewModel, string description)
		{
			NavigationViewModel = navigationViewModel;
			CancelCommand = new DelegateCommand(CancelHandler);
			LinkCommand = new DelegateCommand(LinkHandler);
			TestSound = new DelegateCommand(PlaySound);
			details = description;
		}

		protected abstract Uri GetLink();

		protected abstract object GetCancelViewModel();

		private void LinkHandler()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				string url = GetLink().AbsoluteUri;
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
			}
			else
				throw new InvalidOperationException("You are damned as a user of wrong a operating system. Please install correct operating system)");
		}

		private void CancelHandler()
		{
			isCanceled = true;
			NavigationViewModel.Show(GetCancelViewModel());
		}

		protected abstract bool TryFind(out string goodResultMessage);

		protected void SearchProcess()
		{
			const int waitTimeoutSeconds = 10;
			while (!isCanceled)
			{
				if (TryFind(out string goodResultMessage))
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						Message = goodResultMessage;
					});
					while (!isCanceled)
					{
						PlaySound();
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

		private static void PlaySound() => Sounds.Instance.Play();
	}
}
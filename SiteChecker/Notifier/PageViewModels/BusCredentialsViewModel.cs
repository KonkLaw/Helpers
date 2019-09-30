using System;
using System.Runtime.InteropServices;
using System.Security;
using CredentialHelper;
using CredentialHelper.Interface;
using Prism.Commands;

namespace Notifier.PageViewModels
{
	class BusCredentialsViewModel : BasePageViewModel
	{
		private readonly NavigationViewModel navigationViewModel;

		private string login;
		public string Login
		{
			get => login;
			set => SetProperty(ref login, value);
		}

		private SecureString securePassword;
		public SecureString SecurePassword
		{
			get => securePassword;
			set => SetProperty(ref securePassword, value);
		}

		public DelegateCommand<SecureString> NextCommand { get; }
		public DelegateCommand BackCommand { get; }

		public BusCredentialsViewModel(NavigationViewModel navigationViewModel)
		{
			this.navigationViewModel = navigationViewModel;
			NextCommand = new DelegateCommand<SecureString>(NextHandler, NextValidator);
			BackCommand = new DelegateCommand(() => navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
		}

		private void NextHandler(SecureString obj)
		{
			// TODO: pull down to native code
			string SecureStringToString(SecureString value)
			{
				IntPtr valuePtr = IntPtr.Zero;
				try
				{
					valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
					return Marshal.PtrToStringUni(valuePtr);
				}
				finally
				{
					Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
				}
			}


			var userInfo = new UserInfo(Login, SecureStringToString(securePassword));
			new WindowsCredentialStorage().Save(userInfo);
			navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, userInfo));
		}

		private bool NextValidator(SecureString obj)
		{
			return true;
		}
	}
}

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using CredentialHelper.Interface;
using Prism.Commands;
using RouteByApi;

namespace Notifier.PageViewModels
{
	class BusCredentialsViewModel : BasePageViewModel
	{
		private readonly NavigationViewModel navigationViewModel;

		private string login;
		public string Login
        {
            get => login;
            set
            {
                if (SetProperty(ref login, value))
                    InputChanged();
            }
        }

        private SecureString securePassword;
        public SecureString SecurePassword
        {
            get => securePassword;
            set
            {
                if (SetProperty(ref securePassword, value))
                    InputChanged();
            }
        }

        private string message = "";
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        private void InputChanged()
        {
            Message = null;
            NextCommand.RaiseCanExecuteChanged();
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
			var loginData = new LoginData(Login, SecureStringToString(securePassword));
			if (BusApi.TryGetNewSession(
				in loginData, out RouteApiSession session, out string errorMessage))
            {
                SessionData sessionData = session.SessionData;
                var credentials = new Credentials(
                    sessionData.PhoneNumber, SecureStringToString(securePassword), sessionData.PhpSesSid, sessionData.Uidh);
                StorageHelper.Save(credentials);
                navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, session));
            }
            else
            {
                Message = errorMessage;
            }
        }

        // TODO: pull down to native code or don't use at all
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

        private bool NextValidator(SecureString obj)
            => login?.Length == 12
               && login.StartsWith("37529")
               && login.All(char.IsDigit)
               && SecurePassword?.Length > 2;
    }
}

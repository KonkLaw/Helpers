using AtlasbusByApi;
using CredentialHelper;
using Notifier.UtilTypes;
using Prism.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace Notifier.PageViewModels
{
    class BusCredentialsViewModel : BasePageViewModel
    {
        private readonly NavigationViewModel navigationViewModel;
        private readonly BusParametersViewmodel backViewModel;

        private string login = string.Empty;
        public string Login
        {
            get => login;
            set
            {
                if (SetProperty(ref login, value))
                    InputChanged();
            }
        }

        private SecureString securePassword = new SecureString();
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
            Message = string.Empty;
            NextCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand<SecureString> NextCommand { get; }
        public DelegateCommand BackCommand { get; }

        public BusCredentialsViewModel(NavigationViewModel navigationViewModel, BusParametersViewmodel backViewModel)
        {
            this.navigationViewModel = navigationViewModel;
            this.backViewModel = backViewModel;
            NextCommand = new DelegateCommand<SecureString>(NextHandler, NextValidator);
            BackCommand = new DelegateCommand(BackHandler);
        }

        private void BackHandler()
        {
            backViewModel.ShouldBy = false;
            navigationViewModel.Show(backViewModel);
        }


        private void NextHandler(SecureString obj)
        {
            var loginData = new LoginData(Login, SecureStringToString(securePassword));
            BusApiSession? session = BusApi.TryLogin(in loginData, out string errorMessage);
            if (session == null)
            {
                Message = errorMessage;
            }
            else
            {
                var credentials = new Credentials(
                    Login, SecureStringToString(securePassword));
                StorageHelper.Save(credentials);
                backViewModel.SetSession(session);
                navigationViewModel.Show(backViewModel);
            }
        }

        // TODO: pull down to native code or don't use at all
        private static string SecureStringToString(SecureString value)
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
            => 
            login != null && 
            login.Length == 12 && 
            login.StartsWith("375") && 
            login.All(char.IsDigit) && 
            SecurePassword != null && 
            SecurePassword.Length > 2;
    }
}
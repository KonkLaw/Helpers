using CredentialHelper;
using Prism.Commands;

namespace Notifier.PageViewModels
{
    class BusCredentialsViewModel : BasePageViewModel
    {
        private readonly NavigationViewModel navigationViewModel;
        private readonly BusSearchParameters searchParameters;
        private readonly WindowsCredentialStorage credentialHelper;

        private string? name;
        public string? Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                    ValidateNextButton();
            }
        }

        private string? phone;
        public string? Phone
        {
            get => phone;
            set
            {
                if (SetProperty(ref phone, value))
                    ValidateNextButton();
            }
        }

        public DelegateCommand OkClick { get; }

        public BusCredentialsViewModel(
            NavigationViewModel navigationViewModel,
            in BusSearchParameters searchParameters,
            WindowsCredentialStorage credentialHelper)
        {
            OkClick = new DelegateCommand(Ok, NextAllowed);
            this.navigationViewModel = navigationViewModel;
            this.searchParameters = searchParameters;
            this.credentialHelper = credentialHelper;
        }

        private void ValidateNextButton() => OkClick.RaiseCanExecuteChanged();

        private bool NextAllowed() => name != null && name.Length > 2 && phone != null && phone.Length == 9;

        private void Ok()
        {
            var credentials = new Credentials(Name, "375" + phone);
            credentialHelper.Save(credentials);
            navigationViewModel.Show(BusSearchingViewModel.Create(navigationViewModel, in searchParameters, credentials));
        }
    }
}
using CredentialHelper;
using Notifier.Model;
using Notifier.UtilTypes;

namespace Notifier.PageViewModels;

class BusCredentialsViewModel : BasePageViewModel
{
    private readonly NavigationViewModel navigationViewModel;
    private readonly WindowsCredentialStorage credentialHelper;
    private readonly IOrderSearchService searchService;

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
        WindowsCredentialStorage credentialHelper,
        IOrderSearchService searchService)
    {
        OkClick = new DelegateCommand(Ok, NextAllowed);
        this.navigationViewModel = navigationViewModel;
        this.credentialHelper = credentialHelper;
        this.searchService = searchService;
    }

    private void ValidateNextButton() => OkClick.RaiseCanExecuteChanged();

    private bool NextAllowed() => name is { Length: > 2 } && phone is { Length: 9 };

    private void Ok()
    {
        var credentials = new Credentials(Name, "375" + phone);
        credentialHelper.Save(credentials);
        searchService.SetCredentials(credentials);
        SearchViewModel searchViewModel = new SearchViewModel(navigationViewModel, searchService);
        navigationViewModel.Show(searchViewModel);
        searchViewModel.RunSearch();
    }
}
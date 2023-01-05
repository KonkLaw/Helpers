using Notifier.Model;
using Notifier.UtilTypes;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Notifier.PageViewModels;

class SearchViewModel : BasePageViewModel
{
    private readonly NavigationViewModel navigationViewModel;
    private readonly ISearchService searchService;

    private bool isCanceled;

    public ICommand TestSound { get; }
    public ICommand LinkCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ShowErrorDetails { get; }

    public string Description { get; }

    private string message = string.Empty;

    public string Message
    {
        get => message;
        private set => SetProperty(ref message, value);
    }

    private ErrorInfo? errorInfo;
    public ErrorInfo? ErrorInfo
    {
        get => errorInfo;
        set => SetProperty(ref errorInfo, value);
    }

    public SearchViewModel(NavigationViewModel navigationViewModel, ISearchService searchService)
    {
        this.navigationViewModel = navigationViewModel;
        this.searchService = searchService;
        CancelCommand = new DelegateCommand(CancelHandler);
        LinkCommand = new DelegateCommand(LinkHandler);
        TestSound = new DelegateCommand(PlaySound);
        ShowErrorDetails = new DelegateCommand(ShowErrorDetailsHandler);
        Description = searchService.GetSearchDescription();
    }

    private void CancelHandler()
    {
        isCanceled = true;
        navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel));
    }

    private void LinkHandler()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string url = searchService.GetFastNavigationLink().AbsoluteUri;
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        else
            throw new InvalidOperationException("You are damned as a user of wrong a operating system. Please install correct operating system)");
    }

    private static void PlaySound() => Sounds.Instance.Play();

    private void ShowErrorDetailsHandler() => navigationViewModel.ShowErrorWindow(errorInfo!.Description);

    protected bool? ProcessRequest(out string details)
    {
        try
        {
            if (searchService.MakeRequest(out string goodResultMessage))
            {
                details = goodResultMessage;
                return true;
            }

            details = "Alive at : " + DateTime.Now.ToLongTimeString();
            return false;
        }
        catch (Exception e)
        {
            details = e.ToString();
            return null;
        }
    }

    protected void SearchProcess()
    {
        const int waitTimeoutMilliseconds = 10_000;
        const int soundMilliseconds = 2_000;

        while (!isCanceled)
        {
            bool? result = ProcessRequest(out string details);
            if (result.HasValue && result.Value)
            {
                Message = details;
                ErrorInfo = null;

                while (!isCanceled)
                {
                    PlaySound();
                    Thread.Sleep(soundMilliseconds);
                }
            }
            else if (result.HasValue && !result.Value)
            {
                Message = details;
                ErrorInfo = null;
                Thread.Sleep(waitTimeoutMilliseconds);
            }
            else
            {
                if (ErrorInfo == null)
                    ErrorInfo = new ErrorInfo(details, TimeOnly.FromDateTime(DateTime.Now));
                else 
                    ErrorInfo = ErrorInfo with { Description = details };

                Message = "First problems at: " + ErrorInfo.Time;

                int timeOnPause = 0;
                while (!isCanceled && timeOnPause < waitTimeoutMilliseconds)
                {
                    PlaySound();
                    Thread.Sleep(soundMilliseconds);
                    timeOnPause += soundMilliseconds;
                }
            }
        }
    }

    public void RunSearch() => Task.Run(SearchProcess);
}

record ErrorInfo(string Description, TimeOnly Time);
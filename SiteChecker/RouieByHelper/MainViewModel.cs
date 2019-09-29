using SiteChecker.RoutieBy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace RouieByHelper
{
    public class MainViewModel : BasePropertyChanged
    {
        private const string StobtcyName = "Stolbtcy";
        private const string MinskName = "Minsk";
        private const string RunText = "Run";
        private const string StopText = "Stop";
        private Thread thread;

        private string outStation;
        public string OutStation
        {
            get => outStation;
            set
            {
                if (outStation == value)
                    return;
                outStation = value;
                OnPropertyChanged();
            }
        }

        private string inStation;
        public string InStation
        {
            get => inStation;
            set
            {
                if (inStation == value)
                    return;
                inStation = value;
                OnPropertyChanged();
            }
        }
        
        private bool notOnRun;
        public bool NotOnRun
        {
            get => notOnRun;
            set
            {
                if (notOnRun == value)
                    return;
                notOnRun = value;
                OnPropertyChanged();
            }
        }
        
        private DateTime lowerDate;
        public DateTime LowerDate
        {
            get => lowerDate;
            set
            {
                if (lowerDate == value)
                    return;
                lowerDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime selectedDate;
        public DateTime SelectedDate
        {
            get => selectedDate;
            set
            {
                if (selectedDate == value)
                    return;
                selectedDate = value;
                OnPropertyChanged();
            }
        }

        private List<int> hours;
        public List<int> Hours
        {
            get => hours;
            set
            {
                if (hours == value)
                    return;
                hours = value;
                OnPropertyChanged();
            }
        }

        private List<int> minutes;
        public List<int> Minutes
        {
            get => minutes;
            set
            {
                if (minutes == value)
                    return;
                minutes = value;
                OnPropertyChanged();
            }
        }

        private int fromHours;
        public int FromHours
        {
            get => fromHours;
            set
            {
                if (fromHours == value)
                    return;
                fromHours = value;
                UpdateRunEnabled();
                OnPropertyChanged();
            }
        }

        private int fromMinutes;
        public int FromMinutes
        {
            get => fromMinutes;
            set
            {
                if (fromMinutes == value)
                    return;
                fromMinutes = value;
                UpdateRunEnabled();
                OnPropertyChanged();
            }
        }

        private int toHours;
        public int ToHours
        {
            get => toHours;
            set
            {
                if (toHours == value)
                    return;
                toHours = value;
                UpdateRunEnabled();
                OnPropertyChanged();
            }
        }
        
        private int toMinutes;
        public int ToMinutes
        {
            get => toMinutes;
            set
            {
                if (toMinutes == value)
                    return;
                toMinutes = value;
                UpdateRunEnabled();
                OnPropertyChanged();
            }
        }

        private bool canRun;
        public bool CanRun
        {
            get => canRun;
            set
            {
                if (canRun == value)
                    return;
                canRun = value;
                OnPropertyChanged();
            }
        }
        
        private string doButtomText;
		private PrivateData privateData;

		public string DoButtomText
        {
            get => doButtomText;
            set
            {
                if (doButtomText == value)
                    return;
                doButtomText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Log { get; set; }

        public ICommand SwithCommand { get; }
        public ICommand RunCommand { get; }

        public MainViewModel(Action closer)
        {
            notOnRun = true;
            outStation = MinskName;
            inStation = StobtcyName;
            selectedDate = lowerDate = DateTime.Now;
            SwithCommand = new SimpleCommand(SwitchHandler);
            RunCommand = new SimpleCommand(RunHandler);
            hours = Enumerable.Range(5, 22 - 5).ToList();
            minutes = Enumerable.Range(0, 60 / 5).Select(i => 5 * i).ToList();
            fromHours = -1;
            fromMinutes = -1;
            toHours = -1;
            toMinutes = -1;
            doButtomText = RunText;
            Log = new ObservableCollection<string>();
            UpdateRunEnabled();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			if (!PrivateDataLoader.TryGetData(out privateData))
			{
				MessageBox.Show("Refillprivatedarta");
				closer();
			}
		}

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            => MessageBox.Show(e.ExceptionObject.ToString());

        private void UpdateRunEnabled()
        {
            if (!NotOnRun)
                return;
            CanRun = false;
            bool canRun = fromHours >= 0 && fromMinutes >= 0 && toHours >= 0 && toMinutes >= 0;
            if (!canRun)
                return;
            var fromTime = new MyTime(hours[fromHours], minutes[fromMinutes]);
            var toTime = new MyTime(hours[toHours], minutes[toMinutes]);
            CanRun = fromTime <= toTime;
        }

        private void RunHandler()
        {
            if (NotOnRun == true)
            {
                thread = new Thread(new ThreadStart(ThreadWork))
                {
                    IsBackground = true
                };
                thread.Start();
            }
            else
            {
                thread.Abort();
            }
            NotOnRun = !NotOnRun;
            CanRun = NotOnRun;
            DoButtomText = notOnRun ? RunText : StopText;
        }

        private void ThreadWork()
        {
            RoutieByLogicAndParser.Buy(
                    new MyTime(hours[fromHours], minutes[fromMinutes]),
                    new MyTime(hours[toHours], minutes[toMinutes]),
                    new MyTime(hours[fromHours], minutes[fromMinutes]),
                    selectedDate, outStation == MinskName, IdleHandle, ExceptionHandler, privateData);
        }

        private void ExceptionHandler(Exception ex)
        {
            if (ex.GetType() == typeof(ThreadAbortException))
                return;
            MessageBox.Show(ex.ToString());
        }

        private void IdleHandle()
        {
            Append($"Alive " + DateTime.Now);
        }

        private void Append(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (Log.Count > 10)
                    Log.RemoveAt(0);
                Log.Add(message);
            }));
        }

        private void SwitchHandler()
        {
            string temp = InStation;
            InStation = OutStation;
            OutStation = temp;
        }
    }
}

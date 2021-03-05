using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Notifier.UtilTypes
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool SetProperty<T>(ref T storage, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, newValue))
                return false;
            storage = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class DelegateCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action handler;
        private readonly Func<bool>? canExecute;

        public DelegateCommand(Action handler)
        {
            this.handler = handler;
        }

        public DelegateCommand(Action handler, Func<bool> canExecute) : this(handler)
        {
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (canExecute == null)
                return true;
            else
                return canExecute();
        }

        public void Execute(object? parameter) => handler();

        internal void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

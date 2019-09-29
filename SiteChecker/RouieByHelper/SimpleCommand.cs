using System;
using System.Windows.Input;

namespace RouieByHelper
{
    class SimpleCommand : ICommand
    {
        private readonly Action action;

		public EventHandler canExecuteChanged;

		event EventHandler ICommand.CanExecuteChanged
		{
			add { canExecuteChanged += value; }
			remove { canExecuteChanged -= value; }
		}

        public SimpleCommand(Action action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => action();
    }
}

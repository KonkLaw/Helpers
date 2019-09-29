using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace Notifier.PaveViews
{
	/// <summary>
	/// Interaction logic for BusParametersView.xaml
	/// </summary>
	public partial class BusParametersView : UserControl
    {
        public BusParametersView()
        {
            InitializeComponent();
        }

		private void TimePicker_TextChanged(object sender, TextChangedEventArgs e)
		{
			// TODO: this is the ugly workaround for next issue: selection of time  in list from time don't raise property changed
			if (sender is TimePicker timePicker)
			{
				FakeName.Focus();
				timePicker.Focus();
			}
		}
	}
}

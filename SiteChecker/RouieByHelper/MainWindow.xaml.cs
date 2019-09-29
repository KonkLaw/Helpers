using System.Windows;

namespace RouieByHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
            DataContext = new MainViewModel(() => Close());
		}
    }
}

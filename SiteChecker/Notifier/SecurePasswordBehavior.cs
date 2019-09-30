using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Notifier.PaveViews
{
	public class SecurePasswordBehavior : Behavior<PasswordBox>
	{
		public static readonly DependencyProperty SecurePasswordProperty =
			DependencyProperty.Register(nameof(SecurePassword), typeof(SecureString), typeof(SecurePasswordBehavior), new PropertyMetadata(default(SecureString)));

		public SecureString SecurePassword
		{
			get { return (SecureString)GetValue(SecurePasswordProperty); }
			set { SetValue(SecurePasswordProperty, value); }
		}

		protected override void OnAttached()
		{
			AssociatedObject.PasswordChanged += PasswordBox_PasswordChanged;
		}

		protected override void OnDetaching()
		{
			AssociatedObject.PasswordChanged -= PasswordBox_PasswordChanged;
		}

		private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			//using (AssociatedObject.SecurePassword)
			{
				SecurePassword = AssociatedObject.SecurePassword;
			}
		}
	}
}

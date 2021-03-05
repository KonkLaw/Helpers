using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MyControls
{
	public class TimePicker : Control
	{
		private const double DefaultSmallStepMinutes = 5;
		private const double DefaultBigStepMinutes = 60;
		static readonly TimeSpan MinValue = new TimeSpan(6, 0, 0);
		static readonly TimeSpan MaxValue = new TimeSpan(22, 0, 0);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
        public static DependencyProperty ValueProperty = DependencyProperty.Register(
					nameof(Value),
					typeof(TimeSpan),
					typeof(TimePicker),
					new FrameworkPropertyMetadata(
						(MinValue + MaxValue) / 2,
						FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
						new PropertyChangedCallback(ValuePropertyChanged),
						new CoerceValueCallback(CorrectValue)));

		private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var timePicker = (TimePicker)d;
			if (timePicker.textBox == null)
				return;
			timePicker.textBox.Text = GetText((TimeSpan)e.NewValue);
		}
		private static object CorrectValue(DependencyObject d, object baseValue)
		{
			var value = (TimeSpan)baseValue;
			if (value > MaxValue)
				return MaxValue;
			if (value < MinValue)
				return MinValue;
			return baseValue;
		}

		private static string GetText(TimeSpan time) => time.ToString(@"hh\:mm");

		static TimePicker()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
		}

		public TimeSpan Value
		{
			get => (TimeSpan)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		private Button hoursButton;
		private TextBox textBox;
		private Button plusButton;
		private Button minusButton;
		private Popup popup;
		private ListView listView;

		public override void OnApplyTemplate()
		{
			hoursButton = GetTemplateChild("PART_HoursButton") as Button;
			textBox = GetTemplateChild("PART_TextBox") as TextBox;
			textBox.IsReadOnly = true;
			plusButton = GetTemplateChild("PART_PlusButton") as Button;
			minusButton = GetTemplateChild("PART_MinusButton") as Button;
			popup = GetTemplateChild("PART_HoursPopup") as Popup;
			listView = GetTemplateChild("PART_HoursList") as ListView;
			listView.ItemsSource = Enumerable.Range(6, 22 - 6).Select(t => new TimeSpan(t, 0, 0));
			listView.SelectionChanged += ListView_SelectionChanged;

			hoursButton.MouseWheel += HoursButton_MouseWheel;
			hoursButton.Click += HoursButton_Click;
			plusButton.Click += PlusButton_Click;
			minusButton.Click += MinusButton_Click;
			MouseWheel += TimePicker_MouseWheel;
			textBox.Text = GetText(Value);
			base.OnApplyTemplate();
		}

		private void HoursButton_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			e.Handled = true;
			ChangeTime(Math.Sign(e.Delta) * DefaultBigStepMinutes);
		}

		private void TimePicker_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			if (e.GetPosition(this).X > ActualWidth / 2)
				ChangeTime(Math.Sign(e.Delta) * DefaultSmallStepMinutes);
			else
				ChangeTime(Math.Sign(e.Delta) * DefaultBigStepMinutes);
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 1 && e.AddedItems[0] is TimeSpan newTime)
			{
				Value = newTime;
				popup.IsOpen = false;
			}
		}

		private void HoursButton_Click(object sender, RoutedEventArgs e) => popup.IsOpen = !popup.IsOpen;

		private void MinusButton_Click(object sender, RoutedEventArgs e) => ChangeTime(-DefaultSmallStepMinutes);

		private void PlusButton_Click(object sender, RoutedEventArgs e) => ChangeTime(DefaultSmallStepMinutes);

		private void ChangeTime(double minutesDelta) => Value = Value.Add(TimeSpan.FromMinutes(minutesDelta));
	}
}

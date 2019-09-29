namespace Notifier.Controls
{
	public class TimePickerExtended : Xceed.Wpf.Toolkit.TimePicker
	{
		protected override void OnIncrement()
		{
			SetStep();
			base.OnIncrement();
		}
		
		protected override void OnDecrement()
		{
			SetStep();
			base.OnDecrement();
		}
		
		private void SetStep()
		{
			Step = CurrentDateTimePart == Xceed.Wpf.Toolkit.DateTimePart.Minute ? 5 : 1;
		}
	}
}
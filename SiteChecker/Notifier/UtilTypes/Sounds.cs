using System.Media;

namespace Notifier.UtilTypes
{
	class Sounds
	{
		private readonly SoundPlayer soundsPlayer;
		public static Sounds Instance = new Sounds();

		private Sounds()
		{
			soundsPlayer = new SoundPlayer(Properties.Resources.Windows_Foreground);
		}

		public void Play()
		{
			soundsPlayer.Play();
		}
	}
}

using System.IO;
using System.Media;
using System.Reflection;

namespace Notifier.UtilTypes
{
		class Sounds
	{
		private readonly SoundPlayer soundsPlayer;
		public static Sounds Instance = new Sounds();

		private Sounds()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string resourceName = "Notifier.Resources.Sound.wav";

			using Stream stream = assembly.GetManifestResourceStream(resourceName);
			soundsPlayer = new SoundPlayer(stream);
			soundsPlayer.Load();
		}

		public void Play()
		{
			soundsPlayer.Play();
		}
	}
}

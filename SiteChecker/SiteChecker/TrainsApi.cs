using System;
using System.IO;
using System.Net;
using System.Threading;

namespace SiteChecker
{
	public interface IComplexInterface
	{

	}

	// NOTE: Times of tickets loading
	// 22.47
	// 22.46
	class TrainsApi
	{
		// TODO: configuration
		// 0) revert all
		// 1) proper uri
		// 2) file in Contents folder - "Copy always"
		// 3) propper content of files
		//	**	DO NOT COPY from browser (site gives different html for different clients):
		//		copy html from editor of VS
		//	**	Some problems with some html:
		//		avoid "<span class="hidden">342206</span>" (constantly changed),
		//		but "<span class="hidden">56</span>" (do not changed).
		//		!! easiest way - to copy starting from arrival time
		//	**	Copy train html to the end: "</tr><!-- // b-train -->"

		private static readonly Uri Uri = new Uri(
		"http://rasp.rw.by/ru/route/?from=%D0%A1%D1%82%D0%BE%D0%BB%D0%B1%D1%86%D1%8B&from_exp=2100123&from_esr=&to=%D0%9C%D0%B8%D0%BD%D1%81%D0%BA-%D0%9F%D0%B0%D1%81%D1%81%D0%B0%D0%B6%D0%B8%D1%80%D1%81%D0%BA%D0%B8%D0%B9&to_exp=2100001&to_esr=&date=2018-01-07");

		public static void Run()
		{
			string[] filesContents = GetFilesContents();

			while (true)
			{
				try
				{
					string responseText = WebApiUtils.WebApiHelper.GetResponseString(Uri);
					foreach (string fileContens in filesContents)
					{
						if (!responseText.Contains(fileContens))
							Alarm();
					}
					Console.WriteLine("Alive: " + DateTime.Now);
					Thread.Sleep(10 * 1000);
				}
				// TODO: m.b. add processing of bad connection related exceptions
				catch (WebException e)
				{
					Alarm(e);
				}
				catch (Exception ex)
				{
					Alarm(ex);
				}
			}

		}

		private static string[] GetFilesContents()
		{
			string[] files = Directory.GetFiles(@"Contents");
			for (int i = 0; i < files.Length; i++)
			{
				files[i] = File.ReadAllText(files[i]);
			}
			return files;
		}

		private static void Alarm(Exception ex)
		{
			Console.WriteLine();
			Console.WriteLine(ex.ToString());
			PlaySoundLoop();
		}

		private static void Alarm()
		{
			Console.WriteLine("!! ALARM !!");
			Console.WriteLine("!! ALARM !!");
			Console.WriteLine("!! ALARM !!");
			PlaySoundLoop();
		}

		private static void PlaySoundLoop()
		{
			while (true)
			{
				Console.Beep();
			}
		}
	}
}

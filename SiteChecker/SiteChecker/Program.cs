using SiteChecker.RoutieBy;
using System;
using System.Net;

namespace SiteChecker
{
	class TicketsHelper
    {
        static void Main(string[] args)
        {
			var tripDay = new DateTime(2019, 03, 30);
			//PrivateDataLoader.WriteTest();
			bool fromMinskToStol = true;
			if (!PrivateDataLoader.TryGetData(out PrivateData privateData))
				throw new Exception("Wrong private data.");
			RoutieByLogicAndParser.Buy(
				new MyTime(12, 25),
				new MyTime(15, 00),
				new MyTime(16, 20),
				tripDay, fromMinskToStol, Do, ExceptionH, privateData);
        }

        private static void ExceptionH(Exception obj)
        {
            RoutieByLogicAndParser.Signal();
        }

        private static void Do()
        {
            Console.WriteLine(DateTime.Now);
        }
    }
}

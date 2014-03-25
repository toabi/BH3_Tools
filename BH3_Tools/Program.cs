using System;
using System.IO;

using Zephyr.Logging;
using Zephyr.IO;


namespace BH3_Tools
{
	class MainClass
	{
	
		private static void printSession(Session session){
			Console.WriteLine ("===========================");
			Console.WriteLine (String.Format("= When: {0}", session.Timestamp));
			Console.WriteLine (String.Format("= Dur.: {0}h {1}m", session.Duration.Hours, session.Duration.Minutes));
			Console.WriteLine (String.Format("= Size: {0}kb", session.Length / 1024));
			Console.WriteLine ("===========================\n");
		}

		private static bool testPort(string serialDevice){
			Console.WriteLine (String.Format("Checking Serial Device {0}", serialDevice));
			return File.Exists (serialDevice);
		}

		public static int Main (string[] args)
		{
			#region ArgParsing
			string action = null;
			string argument = null;

			try {
				action = args [0];
				if (action != "download" && action != "list") {
					throw new Exception ("Wrong action!");
				}

				if (action == "download") {
					argument = args [1];
				}

			} catch {
				Console.WriteLine ("Usage: dl.exe [list|download] [session_id]");
				return 1;
			}
			#endregion

			Console.WriteLine ("Welcome to the BioHarness 3 Log Downloader\n");
			var serialDevice = "/dev/tty.usbmodemfa131";

			if(!testPort(serialDevice)){
				Console.WriteLine ("It looks like the device is not connected!");
				return 1;
			}

			SessionDirectory sessionDir = BioHarnessCommunications.SyncGetSessionDirectory(serialDevice);
			var sessionData = sessionDir.SessionData;

			if (action == "list") {

				Console.WriteLine ("Available Sessions:\n");

				for (int idx = 0; idx < sessionData.Count; idx++) {
					var session = sessionData[idx];
					Console.WriteLine (String.Format ("Session {0}", idx));
					printSession (session);
				}

			} else if (action == "download") {

				Console.WriteLine (String.Format("Downloading Session {0}", argument));
				var mySession = sessionData[Int32.Parse(argument)];
				printSession (mySession);

				var myData = BioHarnessCommunications.SyncLoadData ("/dev/tty.usbmodemfa131", mySession);
				Console.WriteLine (myData.Length);
				string outDir = "/tmp/";  // needs trailing slash
				CsvConverter.CreateStandardCSVFiles (mySession, myData, outDir);
				CsvConverter.CreateAccelCSVFiles (mySession, myData, outDir);

			}

			return 0;
		}
	}
}

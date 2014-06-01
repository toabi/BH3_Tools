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
			string device = "usb";

			var serialDevice = "/dev/tty.usbmodemfa131"; //default to USB

			try {
				action = args [0];

				if (action == "download") {
					argument = args [1];
					try {
						device = args [2];
					} catch {}
				} else if (action == "list"){
					try {
						device = args [1];
					} catch {}
				} else {
					throw new Exception ("Wrong action!");
				}

				try {
					if (device == "usb") {
						// the default is already USB
					} else if (device == "bt") {
						serialDevice = "/dev/tty.BHBHT007929-iSerialPort1";
					} else {
						serialDevice = device; // otherwise use the given string
					}
				} catch {
					// nothing!
				}

			} catch {
				Console.WriteLine ("Usage: BH3_Tools.exe [list|download session_id] (usb|bt|$custom)");
				return 1;
			}
			#endregion

			Console.WriteLine ("Welcome to the BioHarness 3 Log Downloader\n");

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

				Console.WriteLine (String.Format ("Transmitting data..."));
				var myData = BioHarnessCommunications.SyncLoadData ("/dev/tty.usbmodemfa131", mySession);
				Console.WriteLine (String.Format("Received {0} bytes.", myData.Length));

				string outDir = Directory.GetCurrentDirectory() + "/"; // needs trailing slash
				Console.WriteLine (String.Format ("Saving files in {0}", outDir));
				CsvConverter.CreateStandardCSVFiles (mySession, myData, outDir);
				CsvConverter.CreateAccelCSVFiles (mySession, myData, outDir);
				Console.WriteLine ("Done.");
			}

			return 0;
		}
	}
}

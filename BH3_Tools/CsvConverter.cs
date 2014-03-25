using System.IO;
using System;
using System.Globalization;

using Zephyr.Logging;


public class CsvConverter
{
	// Used for float > string conversion
	private static IFormatProvider culture = new CultureInfo ("en-NZ", false);
	/// <summary>
	/// Constant used in interpretation of accelerometer data
	/// </summary>
	private const double ZeroGoffset = 512.0;
	/// <summary>
	/// Constant used in interpretation of accelerometer data
	/// </summary>
	private const double Conversion = 19.456;

	/// <summary>
	/// Standard log format export data to CSV files
	/// </summary>
	/// <param name="session">The log session the data relates to</param>
	/// <param name="data">The binary data for the log.</param>
	public static void CreateStandardCSVFiles (Session session, byte[] data, string outDir)
	{
		int heartRate;
		float breathingRate;
		float temperature;
		int posture;
		float activity;
		float acceleration;
		float battery;
		float breathingAmplitude;
		float ecgAmplitude;
		float ecgNoise;
		float axisXMin;
		float axisXPeak;
		float axisYMin;
		float axisYPeak;
		float axisZMin;
		float axisZPeak;
		int rawBreathingWaveform;
		float intervalRR;
		string filePathAndPrefix = outDir + session.Timestamp.ToString ("yyyy_MM_dd__HH_mm_ss", culture);

		var timestamp = session.Timestamp;
		using (StreamWriter generalFile = new StreamWriter (filePathAndPrefix + "_General.csv")) {
			using (StreamWriter breathingAndRRFile = new StreamWriter (filePathAndPrefix + "_BR_RR.csv")) {
				generalFile.WriteLine ("Timestamp,HR,BR,Temp,Posture,Activity,Acceleration,Battery,BRAmplitude,ECGAmplitude,ECGNoise,XMin,XPeak,YMin,YPeak,ZMin,ZPeak");
				breathingAndRRFile.WriteLine ("Timestamp,BR,RtoR");

				int offset = 0;
				while (offset < data.Length) {
					// Generate the General CSV string
					heartRate = BitConverter.ToInt16 (data, offset);
					breathingRate = BitConverter.ToInt16 (data, offset + 2) / 10f;
					temperature = BitConverter.ToInt16 (data, offset + 4) / 10f;
					posture = BitConverter.ToInt16 (data, offset + 6);
					activity = BitConverter.ToInt16 (data, offset + 8) / 100f;
					acceleration = BitConverter.ToInt16 (data, offset + 10) / 100f;
					battery = BitConverter.ToInt16 (data, offset + 12) / 1000f;
					breathingAmplitude = BitConverter.ToInt16 (data, offset + 14) / 1000f;
					ecgAmplitude = BitConverter.ToInt16 (data, offset + 18) / 1000000f;
					ecgNoise = BitConverter.ToInt16 (data, offset + 20) / 1000000f;
					axisXMin = BitConverter.ToInt16 (data, offset + 24) / 100f;
					axisXPeak = BitConverter.ToInt16 (data, offset + 26) / 100f;
					axisYMin = BitConverter.ToInt16 (data, offset + 28) / 100f;
					axisYPeak = BitConverter.ToInt16 (data, offset + 30) / 100f;
					axisZMin = BitConverter.ToInt16 (data, offset + 32) / 100f;
					axisZPeak = BitConverter.ToInt16 (data, offset + 34) / 100f;
					generalFile.WriteLine (
						string.Format (
							culture,
							"{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
							timestamp.ToString ("dd/MM/yyyy HH:mm:ss.fff", culture),
							heartRate.ToString (culture),
							breathingRate.ToString ("F1", culture),
							temperature.ToString ("F1", culture),
							posture.ToString (culture),
							activity.ToString ("F2", culture),
							acceleration.ToString ("F2", culture),
							battery.ToString ("F3", culture),
							breathingAmplitude.ToString ("F3", culture),
							ecgAmplitude.ToString ("F6", culture),
							ecgNoise.ToString ("F6", culture),
							axisXMin.ToString ("F2", culture),
							axisXPeak.ToString ("F2", culture),
							axisYMin.ToString ("F2", culture),
							axisYPeak.ToString ("F2", culture),
							axisZMin.ToString ("F2", culture),
							axisZPeak.ToString ("F2", culture)));

					/* Generate BR/RR CSV string */
					var brrrTimestamp = timestamp;
					for (int sample = 0; sample < 36; sample += 2) {
						rawBreathingWaveform = BitConverter.ToInt16 (data, offset + 36 + sample);
						intervalRR = BitConverter.ToInt16 (data, offset + 72 + sample) / 1000f;
						/* Check if sample is marked as invalid */
						if (rawBreathingWaveform != 32767 && intervalRR != 32.767) {
							breathingAndRRFile.WriteLine (
								string.Format (
									culture,
									"{0},{1},{2}",
									brrrTimestamp.ToString ("dd/MM/yyyy HH:mm:ss.fff", culture),
									rawBreathingWaveform.ToString (culture),
									intervalRR.ToString (culture)));
							brrrTimestamp += TimeSpan.FromMilliseconds (56);
						}
					}

					offset += session.Channels * 2;
					timestamp += TimeSpan.FromMilliseconds (session.Period);
				}
			}
		}
	}
	// Creates CSV files with acceleration data
	public static void CreateAccelCSVFiles (Session session, byte[] data, string outDir)
	{

		int[] accel = new int[3];

		string filePathAndPrefix = outDir + session.Timestamp.ToString ("yyyy_MM_dd__HH_mm_ss", culture);

		var timestamp = session.Timestamp;

		using (StreamWriter accelerometerFile = new StreamWriter (filePathAndPrefix + "_ACCEL.csv")) {

			accelerometerFile.WriteLine ("Timestamp,Accel_x,Accel_y,Accel_z");

			int offset = 0;
			while (offset < data.Length) {
				// Generate Accel CSV string
				var accelTimestamp = timestamp;
				for (int sample = 0; sample < 126; sample++) {
					if (session.Period < (sample + 1) * 8) {
						break;
					}

					/* Work out which byte to packing starts in */
					int sampleOffset = (30 * sample) / 8;

					/* for all three accelerometer axes in each sample */
					for (int accelChannel = 0; accelChannel < 3; accelChannel++) {
						/* Figure out how many bits have already been used in the current byte */
						int usedBits = ((30 * sample) + (accelChannel * 10)) % 8;

						/* Value read from the queue, add to the packet (bit packed) */
						switch (usedBits) {
						case 0:
						default:
							accel [accelChannel] = data [offset + 36 + sampleOffset] + ((data [offset + 36 + sampleOffset + 1] & 0x03) << 8);
							sampleOffset++;
							break;
						case 2:
							accel [accelChannel] = ((data [offset + 36 + sampleOffset] & 0xFC) >> 2) + ((data [offset + 36 + sampleOffset + 1] & 0x0F) << 6);
							sampleOffset++;
							break;
						case 4:
							accel [accelChannel] = ((data [offset + 36 + sampleOffset] & 0xF0) >> 4) + ((data [offset + 36 + sampleOffset + 1] & 0x3F) << 4);
							sampleOffset++;
							break;
						case 6:
							accel [accelChannel] = ((data [offset + 36 + sampleOffset] & 0xC0) >> 6) + (data [offset + 36 + sampleOffset + 1] << 2);
							sampleOffset++;
							sampleOffset++;
							break;
						}
					}

					accelerometerFile.WriteLine (
						string.Format (
							culture,
							"{0},{1},{2},{3}",
							accelTimestamp.ToString ("dd/MM/yyyy HH:mm:ss.fff", culture),
							((accel [0] - ZeroGoffset) / Conversion).ToString (culture),
							((accel [1] - ZeroGoffset) / Conversion).ToString (culture),
							((accel [2] - ZeroGoffset) / Conversion).ToString (culture)));
					accelTimestamp += TimeSpan.FromMilliseconds (8);
				}

				offset += session.Channels * 2;
				timestamp += TimeSpan.FromMilliseconds (session.Period);
			}
		}
	}
}

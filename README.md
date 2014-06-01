# Bio Harness 3 Tools

This little C# program can download data form the BH3 in CSV format.
It is compatible with [Mono](www.mono-project.com) so it should run everywhere but it excepcts unix-style devices.

It is based on the example code of the [BioHarness Bluetooth Developer Kit](http://bioharness.com/zephyr-labs/)
but all windows specific and mono incompatible parts where removed.

## Installation

It's easiest to use [Xamarin Studio](http://xamarin.com/download) to build this solution.

You may have to change the device name. The BH3 usually appears as something 
like `/dev/tty.usbmodemfa131` if connected via USB and `/dev/tty.BHBHT007929-iSerialPort1` via Bluetooth.
You can specify the device location as the last argument on the commandline.
Downloading CSV files via Bluetooth is possible, but listing the sessions is.

Deleting sessions is not possible (yet).

## Example Usage

    $ mono BH3_Tools.exe list
    Welcome to the BioHarness 3 Log Downloader

    Checking Serial Device /dev/tty.usbmodemfa131
    Available Sessions:

    Session 0
    ===========================
    = When: 3/14/2014 8:28:29 AM
    = Dur.: 10h 17m
    = Size: 37041kb
    ===========================
    
    Session 1
    ===========================
    = When: 3/16/2014 5:30:07 PM
    = Dur.: 0h 37m
    = Size: 2243kb
    ===========================
 
	$ mono bin/Debug/BH3_Tools.exe download 8
    Welcome to the BioHarness 3 Log Downloader

    Checking Serial Device /dev/tty.usbmodemfa131
    Downloading Session 8
    Session 1
    ===========================
    = When: 3/16/2014 5:30:07 PM
    = Dur.: 0h 37m
    = Size: 2243kb
    ===========================

    Transmitting data...
    Received 2296832 bytes.
    Saving files in /Users/tobi/BH3_Tools/
    Done.
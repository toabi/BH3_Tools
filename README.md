# BioHarness 3 Tools

This little C# program can download data form the BH3 in CSV format.
It is compatible with [Mono](www.mono-project.com), so it should run everywhere. It just excepcts unix-style devices to do serial communication with. It was never tested on Windows.

It is based on the example code of the [BioHarness Bluetooth Developer Kit](http://bioharness.com/zephyr-labs/)
but all the Windows-specific and Mono-incompatible parts where removed.


## Installation

It's easiest to use [Xamarin Studio](http://xamarin.com/download) to build this solution.

You may have to change the device name. The BH3 usually appears as something 
like `/dev/tty.usbmodemfa131` if connected via USB and `/dev/tty.BHBHT007929-iSerialPort1` via Bluetooth.
You can specify the device location as the last argument on the commandline or change the strings in the source.
Downloading CSV files via Bluetooth is not possible, but listing the sessions is.

Deleting sessions and doing fancy things is not possible (yet).


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
     
    $ mono BH3_Tools.exe download 1
    Welcome to the BioHarness 3 Log Downloader

    Checking Serial Device /dev/tty.usbmodemfa131
    Downloading Session 1
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

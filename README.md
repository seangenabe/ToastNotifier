# ToastNotifier

Painlessly send Windows toast notifications from the desktop.

## Installation

Note: The program will automatically install itself during usage.

In case you want to install the program without sending a notification, you can do:

```
ToastNotifier --install
```

The program will be installed to `%LOCALAPPDATA%\ToastNotifier\ToastNotifier.exe`.

## Usage

```
ToastNotifier [options] message
```

### `-desc`

[4] The message to send.

### `-t`, `--title`, `-title`

[4] Specify the title for the toast notification."

### `-p`, `--image`

Include an image. Should be a URI. [1]

### `-a`, `--audio`

Specify audio to play. Should be a URI. [2][3] Set to `silent` to mute.

### `-x`, `--xml`

Custom XML to send to the notifier. This overrides everything above.

### `-m`, `--multiline` `[0|1]`

Multiline support. Enabled by default.

### `-i`, `--install`

Installs the program. Use if you want to install the program manually without actually doing anything else.

### `-h`, `--help`

Displays help. [5]

## Exit codes

* 1: Generic error. Consult the standard output for more information.

## Using with Scripty and Growl for Windows

This program can receive Growl notifications from Growl for Windows via the Scripty display. Here's how:
* Install [Growl for Windows](http://www.growlforwindows.com/gfw/).
* Install the display [Scripty](http://www.growlforwindows.com/gfw/displays/scripty).
* Install ToastNotifier. (See the Installation section.)
* Open GFW.
* Select Scripty as the default display.
* On Scripty's settings, browse to ToastNotifier's location. (See the Installation section.)

## Notes

* 1: [Notification schema: image element](http://msdn.microsoft.com/en-us/library/windows/apps/br230844.aspx)
* 2: [The toast audio options catalog](http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh761492.aspx)
* 3: [Notification schema: audio element](http://msdn.microsoft.com/en-us/library/windows/apps/br230842.aspx)
* 4: Additional parameters exist so you can use this program directly with the Scripty display for Growl for Windows.
* 5: This is now useless since the program is set as a Windows Application in order to hide the console window, and will exit early. If you know how to fix this, send a pull request.

## License

Copyright (c) 2014 [Sean Genabe](https://github.com/s4g6)

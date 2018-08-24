# Telegram Storage Parser
[![Build Status](https://travis-ci.org/MihaZupan/TelegramStorageParser.svg?branch=master)](https://travis-ci.org/MihaZupan/TelegramStorageParser)

Program to decrypt and parse Telegram Desktop's local storage

This project relies on OpenSSL for crypto primitives. See their [license here](https://www.openssl.org/source/license.txt).

Build on Net Standard 1.3

## Usage example

```csharp
using MihaZupan.TelegramStorageParser;
using MihaZupan.TelegramStorageParser.TelegramDesktop;

string tDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Telegram Desktop/tdata";
ParsingState parsingState = LocalStorage.TryParse(tDataPath, out LocalStorage localStorage);

if (parsingState == ParsingState.Success)
{
    Console.WriteLine("Phone number: " + localStorage.LoggedPhoneNumber ?? "not present");
}
```

### Example project

[View project source](https://github.com/MihaZupan/TelegramStorageParser/blob/master/examples/TelegramDesktopExample/Program.cs)

It can:
* Parse local storage
* Export all cached images
* Export all cached voice recordings
* Export MTProto keys
* Export a bunch of miscellaneous settings

Parsing of other data types (stickers, messages ...) will follow

## Brute forcing the passcode

A gpu based cracker is now available in
[John the Ripper](https://github.com/magnumripper/JohnTheRipper)
thanks to [@kholia](https://github.com/kholia)

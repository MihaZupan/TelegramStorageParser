# Telegram Desktop Local Storage
Program to decrypt and parse Telegram Desktop's local storage

This project relies on OpenSSL for MD5, SHA1, AES-IGE and PBKDF2 implementations. See their [license here](https://www.openssl.org/source/license.txt).

To use, copy the 'tdata' directory (found in "AppData\Roaming\Telegram Desktop" by default) into the working directory or change the location in the [Constants.cs](https://github.com/MihaZupan/TelegramDesktopLocalStorage/blob/master/src/TelegramLocalStorage/Constants.cs) file.

## Useful references:

[Telegram Desktop source code](https://github.com/telegramdesktop/tdesktop)

Most relevant files (happy digging):
* [localstorage.cpp](https://github.com/telegramdesktop/tdesktop/blob/dev/Telegram/SourceFiles/storage/localstorage.cpp)
* [auth_key.cpp](https://github.com/telegramdesktop/tdesktop/blob/dev/Telegram/SourceFiles/mtproto/auth_key.cpp)
* [auth_key.h](https://github.com/telegramdesktop/tdesktop/blob/dev/Telegram/SourceFiles/mtproto/auth_key.h)

## Brute forcing the passcode

[MapPasscodeBruteForce.cs](https://github.com/MihaZupan/TelegramDesktopLocalStorage/blob/master/src/TelegramLocalStorage/MapPasscodeBruteForce.cs) coitains a sample implementation for brute forcing a passcode by eliminating as much excess code as possible

Some estimates:
* Ryzen 1700	@ 3,7 GHz - 255 tries/s
* Intel 5820k	@ 3,4 GHz - 180 tries/s


A gpu based cracker is [in the works](https://github.com/magnumripper/JohnTheRipper/issues/3280) thanks to [@kholia](https://github.com/kholia)

Initial post on Hashcat forums: [here](https://hashcat.net/forum/thread-7598.html)

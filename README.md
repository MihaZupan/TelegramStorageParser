# Telegram Desktop Local Storage
Program to decrypt and parse Telegram Desktop's local storage

This project relies on OpenSSL for crypto primitives. See their [license here](https://www.openssl.org/source/license.txt).

## Usage example

Parse localstorage (brute force the passcode if necessary), export all saved images and audio files: [Example project](https://github.com/MihaZupan/TelegramDesktopLocalStorage/blob/master/src/TelegramLocalStorageExample/Program.cs)

Parsing of other data types (stickers, MTP auth keys ...) will follow

## Brute forcing the passcode

A gpu based cracker is now available in [John the Ripper](https://github.com/magnumripper/JohnTheRipper) thanks to [@kholia](https://github.com/kholia)

The hash string can be generated using the 'GenerateJohnTheRipperHashString' method in PasscodeBruteForce.cs

For testing you can also generate hash strings using 'GenerateTestJohnTheRipperHashString'

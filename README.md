# XCI Explorer

[Original MaxConsole Release](https://www.maxconsole.com/threads/exclusive-xci-explorer-released-for-switch-game-cartridge-backups.47046/)

View contents of XCI files and more!

## Features
* View metadata
* Explore partitions
* Check NCA hashes
* Extract NCA
* Modify cert

## Requirements
* Visual Studio 2017
* [Hactool](https://github.com/SciresM/hactool/releases)
* [Dumped keys](https://gbatemp.net/threads/how-to-get-switch-keys-for-hactool-xci-decrypting.506978/) ([or get them here](https://github.com/StudentBlake/XCI-Explorer/releases/download/v1.0.0.0/Get-keys.txt.bat))

Main Tab Metadata/Cert/Trimming | Partitions Tab NCA Extract/Hash Check
:-------------------------:|:-------------------------:
![main](https://imgur.com/tO5oPq4.jpg) | ![partitions](https://imgur.com/U7FpWr2.jpg)

## Build Instructions
* Open **XCI Explorer.sln**
* Change *Debug* to *Release* in the dropdown menu
* Go to *Build*, then *Build Solution*
* Extract **hactool.zip** to `XCI-Explorer/bin/Release/` folder
* Run **XCI-Explorer.exe**

## Special Thanks
klks - CARD2, Hash Validation, bug fixes

## Disclaimer
This is not my original work. I just made minor changes with the help of others.
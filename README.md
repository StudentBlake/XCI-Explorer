# XCI Explorer

[Original MaxConsole Release](https://www.maxconsole.com/threads/exclusive-xci-explorer-released-for-switch-game-cartridge-backups.47046/)

View contents of XCI files and more!

## Features
* View metadata for XCI and NSP files
* Explore partitions
* Check NCA hashes
* Extract NCA
* Modify cert

Main | Partitions
:-------------------------:|:-------------------------:
![main](https://cdn.discordapp.com/attachments/377518386826969088/567051362827763717/1.JPG) | ![partitions](https://cdn.discordapp.com/attachments/377518386826969088/567051371375755264/2.JPG)

## Build Requirements
* [Visual Studio Community 2019](https://visualstudio.microsoft.com/downloads/)
* [hactool](https://github.com/SciresM/hactool/releases)
* [Lockpick](https://gbatemp.net/threads/switch-7-0-key-derivation-lockpick_rcm-payload.532916/)

## Build Instructions
* Open **XCI Explorer.sln**
* Change *Debug* to *Release* in the dropdown menu
* Go to *Build*, then *Build Solution*
* Extract **hactool.zip** to `XCI-Explorer/bin/Release/tools/` folder
* Run **XCI-Explorer.exe**

## Special Thanks
* klks - CARD2, hash validation and bug fixes
* garoxas - Game revision, QoL changes and bug fixes

## Disclaimer
This is not my original work. I just made minor changes with the help of others.
# XCI Explorer

[Original Release](https://www.maxconsole.com/threads/exclusive-xci-explorer-released-for-switch-game-cartridge-backups.47046/)

View contents of XCI files and more

## Features
* View metadata
* View partitions
* Compare hashes
* Extract NCA
* Modify cert

## Requirements
* Visual Studio 2017
* [Hactool](https://github.com/SciresM/hactool/releases) (optional)
* [Dumped keys](https://gbatemp.net/threads/how-to-get-switch-keys-for-hactool-xci-decrypting.506978/) (optional)

![program](https://i.imgur.com/xt6VpN7.jpg)


## Build Instructions
* Open XCI Explorer.sln
* Build -> Build Solution
* Add hactool + dependencies + keys.txt to `XCI-Explorer/bin/Debug/` folder

## Special Thanks
Addition of CARD2 and hash support provided by klks

## Disclaimer
This is not my original work. I just decompiled the rogue executable floating around and made minor changes. 

I am NOT a developer and this has NOT been extensively tested!
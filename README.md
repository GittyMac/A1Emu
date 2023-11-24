> [!IMPORTANT]
> Most contributions to U.B. Funkeys server development should be directed to Lautha1's [UBFunkeysServer repository](https://github.com/Lautha1/UBFunkeysServer) instead of A1Emu.

> [!CAUTION]
> This is an incomplete project! Please do not expect a complete or secure server experience with this.

# A1Emu
An open source recreation of the ArkONE servers used for the 2007 game, U.B. Funkeys. 

## Plugins
ArkONE is designed with multiple plugins for certain aspects/games. Here is a list of the status of A1Emu's plugin reimplementations.

### Fully Implemented
* **Plugin 0** - Core

* **Plugin 1** - Users

* **Plugin 3** - Boxing

* **Plugin 5** - Soccer

* **Plugin 7** - Galaxy

* **Plugin 10** - Trunk

### Partially Implemented
* **Plugin 2** - Chats

* **Plugin 4** - Mahjong

* **Plugin 6** - Pool

## Building
### Dependencies
A1Emu requires .NET 6 and MariaDB to run properly.

To get these dependencies on Ubuntu/Debian:

```bash
apt install dotnet-sdk-6.0 dotnet-runtime-6.0 mariadb-server
```

To get these dependencies on RHEL/CentOS/Fedora:

```bash
dnf install dotnet-sdk-6.0 dotnet-runtime-6.0 mariadb-server
```

### Get Source
Fetch the A1Emu repository via Git, and then change into the cloned directory.
```bash
git clone https://github.com/GittyMac/A1Emu
cd A1Emu
```

### Initiating Build
Use the .NET SDK to restore the NuGet packages used by A1Emu and to initiate the build.
```bash
dotnet restore
dotnet build
```
The output will be located in `A1Emu/A1Emu/bin/Debug/net6.0`.

## Running
A1Emu does not generate a SQL database, so you will have to manually create one with MariaDB. A [template SQL file](https://github.com/GittyMac/A1Emu/blob/master/A1DB.sql) is provided to create the basic structure.

```bash
mysql -u [USERNAME] -p [CREATED_DATABASE] < A1DB.sql
```

In the folder containing the A1Emu DLL, there should be a file called `config` created with these contents:
```
port=2000
directory=[DIRECTORY FOR PROFILES]
sq=server=localhost;userid=[USERNAME];password=[PASSWORD];database=FKData
ip=127.0.0.1
```

All you need to do after this is to start the MariaDB server and run A1Emu.dll with .NET.
```bash
dotnet A1Emu.dll
```
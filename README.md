# Waveshare Display Controller in CSharp

This project provides c# code to control waveshare display.

## Getting started
1. Get Visual Studio 2022
2. Clone the repository
```
git clone https://github.com/laphir/waveshare.git
```
3. Open `Waveshare.sln` and compile it.


How to install .net on Linux:
* To install .NET 8.0 on an ARM device, the CPU should support at least ARMv7; Raspberry Pi 1 and Pi Zero 1, which are ARMv6, are not supported.
* Make sure following command returns 8.0 or higher.
```
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --verbose --dry-run
```
* Then install .net on your device. Default install location is `~/.dotnet`
```
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --verbose
```
* If you want to install .net on other location:
```
sudo su
export DOTNET_INSTALL_DIR=/opt/dotnet
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --verbose
```

## Supported Waveshare devices
* 2.13bc - Black, White, Red
* 2.7 HAT v2 - 4 gray
* 7.5 v2 - 800x480

## WaveshareTool
### `clear`: command clears screen
To clear screen of 2.7 HAT v2:
```
WaveshareTool clear --color white --device 2.7v2
```

### `draw`: draw an image on screen
Currently image size should match the screen size.
```
WaveshareTool draw something.png
```

## License
This project is licensed under the MIT License - see the LICENSE file for details.

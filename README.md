# Unity Package Utils

[![openupm](https://img.shields.io/npm/v/net.battlehub.packageutils?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.battlehub.packageutils/)
[![GitHub issues](https://img.shields.io/github/issues/Battlehub0x/PackageUtils)](https://github.com/Battlehub0x/PackageUtils/issues)
[![GitHub license](https://img.shields.io/github/license/Battlehub0x/PackageUtils?label=license)](https://github.com/Battlehub0x/PackageUtils/blob/main/LICENSE)

This package makes it easy to switch between the production version of packages and the local development version, find and edit package.json.

![Package Utils Window](https://github.com/Battlehub0x/PackageUtils/blob/main/Docs/PackageUtilsWindow.jpg?raw=true)

## Installation

The easiest way to install is to download and open the [Installer Package](https://package-installer.glitch.me/v1/installer/OpenUPM/net.battlehub.packageutils?registry=https%3A%2F%2Fpackage.openupm.com&scope=net.battlehub)

It runs a script that installs Package Utils via a scoped registry.

Afterwards Package Utils is listed in the Package Manager (under My Registries) and can be installed and updated from there.

Alternatively, merge the snippet to [Packages/manifest.json](https://docs.unity3d.com/Manual/upm-manifestPrj.html)

```
{
    "scopedRegistries": [
        {
            "name": "package.openupm.com",
            "url": "https://package.openupm.com",
            "scopes": [
                "net.battlehub.packageutils",
		    "net.battlehub.simplejson"
            ]
        }
    ],
    "dependencies": {
        "net.battlehub.packageutils": "major.minor.patch"
    }
}
```

## Usage 

- To get started, click **Tools->Battlehub->Package Utils** on the main menu.

- Select a package from the list. 

- Enter the local path to the file with the development package version.

- Click the button to switch the development mode.


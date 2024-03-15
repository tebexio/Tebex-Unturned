# Tebex Unturned Plugin (LDM / RocketMod)
![Static Badge](https://img.shields.io/badge/LDM-4.9.3.15-brightgreen)

## Description
[Tebex](https://tebex.io/) provides a monetization and donation platform for game servers, allowing server owners to manage in-game purchases, subscriptions, and donations with ease.

This plugin allows you to fulfill purchases on your Unturned server, enabling you to offer a wide range of virtual items, packages, and services to your players.

## Commands
The following commands are available through the Tebex Unturned Plugin:

### Admin Commands
- `/tebex:secret <secret>`: Set your server's secret key
- `/tebex:sendlink <player> <packageId>`: Send a purchase link for a package to a player.
- `/tebex:forcecheck`: Force run any due online and offline commands.
- `/tebex:refresh`: Refresh your store's listings.
- `/tebex:report`: Prepare a report that can be submitted to our support team.
- `/tebex:ban`: Ban a player from using the store. **Players can only be unbanned from the webstore UI.**
- `/tebex:lookup`: Display information about a customer.

### User Commands
- `/tebex:help`: Display a list of available commands and their descriptions.
- `/tebex:info`: Display public store information.
- `/tebex:categories`: View all store categories.
- `/tebex:packages`: View all store packages.
- `/tebex:checkout <packageId>`: Create a checkout link for a package.
- `/tebex:stats`: View your own player statistics in the store.

## Installation
To install the Tebex Unturned Plugin, follow these steps:

1. Install RocketMod / LDM to your Unturned server: https://github.com/SmartlyDressedGames/Legally-Distinct-Missile/releases
2. Download the latest release of this plugin from [Tebex.io](https://docs.tebex.io/plugin/official-plugins), or choose the latest [Release](https://github.com/tebexio/Tebex-Unturned/releases) from this repository.
3. Upload the plugin's `TebexUnturned.dll` file into your Plugins directory of your game server. By default this would be `Servers/Default/Rocket/Plugins`
4. Start the game server. Use `/tebex:secret your-secret-key-here` via the console to set up your store.
5. Your secret key can also be set in the plugin's configuration file located in the `TebexUnturned` folder generated after startup in your `Plugins` folder.

## Dev Environment Setup
If you wish to contribute to the development of the plugin, you can set up your development environment as follows:

**Requirements:**
- [Legally Distinct Missile](https://github.com/SmartlyDressedGames/Legally-Distinct-Missile/releases)

**Setup Instructions:**
1. Clone the repository to an empty folder.
2. Ensure the assemblies `Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`, `com.rlabrecque.steamworks.net.dll`, `Rocket.API.dll`, `Rocket.Core.dll`, `Rocket.Unturned.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`

For updates or additional needed assemblies, use the same assemblies as LGM.

## Contributions
We welcome contributions from the community. Please refer to the `CONTRIBUTING.md` file for more details. By submitting code to us, you agree to the terms set out in the CONTRIBUTING.md file

## Support
This repository is only used for bug reports via GitHub Issues. If you have found a bug, please [open an issue](https://github.com/tebexio/Tebex-Unturned/issues).

If you are a user requiring support for Tebex, please contact us at https://tebex.io/contact
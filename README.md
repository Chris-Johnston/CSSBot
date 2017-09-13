# CSSBot

A bot made for the CSS Discord server.

## Installation and Usage

This project is cross-platform, however Visual Studio 2017 is recommended, so you may want to stick to Windows for this.

- Install the [.NET Core SDK](https://www.microsoft.com/net/download/core).
- [Add the Discord.Net Package source to the NuGet package manager.](https://discord.foxbot.me/docs/guides/getting_started/installing.html)
  - This bot makes heavy use of prerelease versions of the Discord.NET library.
  - [Link to the Discord.NET documentation.](https://discord.foxbot.me/docs/)
  - [Link to the Discord.NET library GitHub page.](https://github.com/RogueException/Discord.Net/tree/dev)
- Clone this repository. You can do this in a command line shell, or using the built-in GUI that's part of VS.
- Update your NuGet packages. The project *should* build now.
- **Optional, only for testing.** Create a bot. This will be your testing bot.
  - Navigate to the Discord API Docs and log in: https://discordapp.com/developers/applications/me
  - Click on the "New App" button.
  - Name your app. Click "Create App".
  - Click "Create a Bot User". This is the type of bot that CSSBot is.
  - Create a file `debugBotConfig.xml` under any directory you want.
  - Copy the contents of `Test Config.xml` into your new file. Insert your bot's user token into the `<ConnectionToken>` field.
  - Set the startup parameters of your project to point to your configuration file. The command line arguments should read: `-config=/Path/To/Config.xml`.

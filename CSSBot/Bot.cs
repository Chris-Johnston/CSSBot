using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot
{
    public class Bot
    {
        private DiscordSocketClient client;
        private CommandHandler handler;

        public async Task Start()
        {
            // starts our client
            // we use LogSeverity.Debug because more info the better
            client = new DiscordSocketClient(new DiscordSocketConfig() { LogLevel = Discord.LogSeverity.Debug });

            // log in as a bot using our connection token
            await client.LoginAsync(TokenType.Bot, Program.GlobalConfiguration.Data.ConnectionToken);
            await client.StartAsync();

            // set up our commands
            handler = new CommandHandler();
            await handler.Install(client);

            // set up our logging function
            client.Log += Log;

            // show an invite link when we are ready to go
            client.Ready += Client_Ready;

            // set some help text
            await client.SetGameAsync(string.Format("Type {0}Help", GlobalConfiguration.CommandPrefix));

            // wait indefinitely 
            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            // display a helpful invite url in the log when the bot is ready
            var application = await client.GetApplicationInfoAsync();
            await Log(new LogMessage(LogSeverity.Info, "Program",
                $"Invite URL: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>"));
        }

        public async static Task Log(Discord.LogMessage arg)
        { 
            // log stuff to console
            // could also log to a file if needed later on
            Console.WriteLine(arg.ToString());
        }
    }
}

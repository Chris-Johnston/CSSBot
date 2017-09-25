using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot
{
    public class Bot
    {
        private DiscordSocketClient m_client;
        private CommandHandler m_handler;

        private IServiceCollection m_serviceCollection;

        public async Task Start()
        {
            // starts our client
            // we use LogSeverity.Debug because more info the better
            m_client = new DiscordSocketClient(new DiscordSocketConfig() { LogLevel = Discord.LogSeverity.Debug });

            // log in as a bot using our connection token
            await m_client.LoginAsync(TokenType.Bot, Program.GlobalConfiguration.Data.ConnectionToken);
            await m_client.StartAsync();

            // dependency injection
            m_serviceCollection = new ServiceCollection();

            // add client as a singleton to service collection
            m_serviceCollection.AddSingleton(m_client);

            // set up our commands
            m_handler = new CommandHandler();
            await m_handler.Install(m_client, m_serviceCollection);

            // set up our logging function
            m_client.Log += Log;

            // show an invite link when we are ready to go
            m_client.Ready += Client_Ready;

            // set some help text
            await m_client.SetGameAsync(string.Format("Type {0}Help", GlobalConfiguration.CommandPrefix));

            // wait indefinitely 
            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            // display a helpful invite url in the log when the bot is ready
            var application = await m_client.GetApplicationInfoAsync();
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

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSSBot.Reminders;
using Discord.Commands;
using System.Reflection;
using CSSBot.Counters;
using CSSBot.Models;
using CSSBot.Reminders;
using LiteDB;
using CSSBot.Tags;
using CSSBot.Services.TheSpookening;

namespace CSSBot
{
    public class Bot
    {
        private DiscordSocketClient m_client;
        private CommandService _commands;
        private IServiceProvider _services;
        private LiteDatabase _database;

        public async Task Start()
        {
            // open or create our database (if it doesn't exist)
            _database = new LiteDatabase(Program.GlobalConfiguration.Data.LiteDatabasePath);

            // add our startup date
            var startup = _database.GetCollection<StartupEvent>("startup");
            startup.Insert(new StartupEvent() { Time = DateTime.Now });
            
            // starts our client
            // we use LogSeverity.Debug because more info the better
            m_client = new DiscordSocketClient(new DiscordSocketConfig() { LogLevel = Discord.LogSeverity.Debug });

            _commands = new CommandService();

            // log in as a bot using our connection token
            await m_client.LoginAsync(TokenType.Bot, Program.GlobalConfiguration.Data.ConnectionToken);
            await m_client.StartAsync();

            // dependency injection
            _services = new ServiceCollection()
                .AddSingleton(m_client)
                .AddSingleton(_commands)
                .AddSingleton(_database)
                .AddSingleton(new CounterService(_database, m_client))
                .AddSingleton(new ReminderService(m_client, _database))
                .AddSingleton(new TagService(_database))
                .AddSingleton(new SpookeningService(m_client, _database))
                .BuildServiceProvider();
            
            await InstallCommandsAsync();

            // set up our logging function
            m_client.Log += Log;

            // show an invite link when we are ready to go
            m_client.Ready += Client_Ready;

            // set some help text
            await m_client.SetGameAsync(string.Format("Type {0}Help", GlobalConfiguration.CommandPrefix));

            // wait indefinitely 
            await Task.Delay(-1);
        }

        private async Task InstallCommandsAsync()
        {
            m_client.MessageReceived += M_client_MessageReceived;
            await _services.GetRequiredService<CommandService>().AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task M_client_MessageReceived(SocketMessage arg)
        {
            // Don't handle the command if it is a system message
            var message = arg as SocketUserMessage;
            if (message == null) return;

            // Mark where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message has a valid prefix, adjust argPos 

            //todo update command handler stuff
            if (!(message.HasMentionPrefix(m_client.CurrentUser, ref argPos) || message.HasCharPrefix(GlobalConfiguration.CommandPrefix, ref argPos))) return;

            // Create a Command Context
            var context = new CommandContext(m_client, message);
            // Execute the Command, store the result
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            // If the command failed
            if (!result.IsSuccess)
            {
                // log the error
                Discord.LogMessage errorMessage = new Discord.LogMessage(Discord.LogSeverity.Warning, "CommandHandler", result.ErrorReason);
                await Log(errorMessage);
                // don't actually reply back with the error

                // should probably redesign this
                // if a command doesn't match, should try and find closest matches
            }
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

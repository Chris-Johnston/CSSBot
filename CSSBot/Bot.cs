using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSSBot.Reminders;
using Discord.Commands;
using System.Reflection;
using CSSBot.Counters;
using CSSBot.Models;
using LiteDB;
using CSSBot.Tags;
using CSSBot.Services.TheSpookening;
using CSSBot.Services;
using CSSBot.Services.Courses;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System.Runtime.CompilerServices;

namespace CSSBot
{
    public static class LogSeverityExtensions
    {
        public static LogLevel ToLogLevel(this LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return LogLevel.Critical;
                case LogSeverity.Error:
                    return LogLevel.Error;
                case LogSeverity.Warning:
                    return LogLevel.Warning;
                case LogSeverity.Info:
                    return LogLevel.Information;
                case LogSeverity.Debug:
                    return LogLevel.Debug;
                case LogSeverity.Verbose:
                default:
                    return LogLevel.Trace;
            }
        }
    }

    public class Bot
    {
        

        private static TelemetryClient telemetryClient = null; // app insights logging

        private DiscordSocketClient m_client;
        private CommandService _commands;
        private IServiceProvider _services;
        private LiteDatabase _database;
        private MessageRetryService messageRetry;
        private static ILogger logger;

        private void SetupLogging()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter(x => true) // enable it all
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddConsole()
                    .AddDebug();

                if (!string.IsNullOrWhiteSpace(Program.GlobalConfiguration.Data.AppInsightsInstrumentationKey))
                {
                    builder.AddApplicationInsights(Program.GlobalConfiguration.Data.AppInsightsInstrumentationKey,
                        x =>
                        {
                            x.IncludeScopes = true;
                            x.TrackExceptionsAsExceptionTelemetry = true;
                            x.TrackExceptionsAsExceptionTelemetry = true;
                        });
                }
            });
            logger = loggerFactory.CreateLogger<Bot>();
            logger.LogDebug("Logger initialized.");

            // TODO: migrate away from DIY config and use IHostBuilder supported config
            // TODO: consider moving entry point from calling and waiting forever to using stuff that Host can do
        }

        public async Task Start()
        {
            SetupLogging();
            // only set the telemetryClient if the key is set
            if (Program.GlobalConfiguration.Data.AppInsightsInstrumentationKey != null)
            {
                var config = new TelemetryConfiguration(Program.GlobalConfiguration.Data.AppInsightsInstrumentationKey);
                telemetryClient = new TelemetryClient(config);
                telemetryClient.TrackTrace("App logging initialized.", SeverityLevel.Information);
            }

            // open or create our database (if it doesn't exist)
            _database = new LiteDatabase(Program.GlobalConfiguration.Data.LiteDatabasePath);

            // add our startup date
            var startup = _database.GetCollection<StartupEvent>("startup");
            startup.Insert(new StartupEvent() { Time = DateTime.Now });

            // starts our client
            // we use LogSeverity.Debug because more info the better
            m_client = new DiscordSocketClient(new DiscordSocketConfig() { LogLevel = Discord.LogSeverity.Verbose, AlwaysDownloadUsers = true });

            _commands = new CommandService();
            messageRetry = new MessageRetryService(m_client);

            // log in as a bot using our connection token
            await m_client.LoginAsync(TokenType.Bot, Program.GlobalConfiguration.Data.ConnectionToken);
            await m_client.StartAsync();

            // dependency injection
            _services = new ServiceCollection()
                .AddSingleton(m_client)
                .AddSingleton(_commands)
                .AddSingleton(_database)
                .AddSingleton(logger)
                .AddSingleton(new CounterService(_database, m_client))
                .AddSingleton(new ReminderService(m_client, _database))
                .AddSingleton(new TagService(_database))
                .AddSingleton(new SpookeningService(m_client, _database, Program.GlobalConfiguration.Data.SpookyConfigJson, logger))
                .AddSingleton(messageRetry)
                .AddSingleton(new MinesweeperSolutionService())
                .AddSingleton(new CourseService(m_client, logger))
                .BuildServiceProvider();

            await InstallCommandsAsync();
            messageRetry.OnCommandRetryHandler += commandMessageUpdated;

            // set up our logging function
            m_client.Log += Log;
            m_client.Log += async (logMessage) => {
                logger.Log(logMessage.Severity.ToLogLevel(), exception: logMessage.Exception, message: logMessage.Message, logMessage.Source, logMessage.Exception);
            };

            // show an invite link when we are ready to go
            m_client.Ready += Client_Ready;

            // set some help text
            await m_client.SetGameAsync($"Type {GlobalConfiguration.CommandPrefix}Help");

            // wait indefinitely 
            await Task.Delay(-1);
        }

        private async Task commandMessageUpdated(SocketMessage arg)
        {
            // todo clean up copied and pasted code
            // Don't handle the command if it is a system message
            var message = arg as SocketUserMessage;
            if (message == null) return;

            // Mark where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message has a valid prefix, adjust argPos

            if (!(message.HasMentionPrefix(m_client.CurrentUser, ref argPos) || message.HasCharPrefix(GlobalConfiguration.CommandPrefix, ref argPos))) return;

            // Create a Command Context
            var context = new CommandContext(m_client, message);
            // Execute the Command, store the result
            try
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                // If the command failed
                if (!result.IsSuccess)
                {
                    // log the error
                    Discord.LogMessage errorMessage = new Discord.LogMessage(Discord.LogSeverity.Warning, "CommandHandler", result.ErrorReason);
                    await Log(errorMessage);
                    // don't actually reply back with the error

                    // todo reply back with an error message that corresponds to the closest matching command name
                }
            }
            catch (Exception e)
            {
                var error = new LogMessage(LogSeverity.Error, "CommandHandler", "Caught exception", e);
                await Log(error);
            }
        }

        private async Task InstallCommandsAsync()
        {
            m_client.MessageReceived += commandMessageReceived;
            await _services.GetRequiredService<CommandService>().AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task commandMessageReceived(SocketMessage arg)
        {
            // Don't handle the command if it is a system message
            var message = arg as SocketUserMessage;
            if (message == null) return;

            // Mark where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message has a valid prefix, adjust argPos

            if (!(message.HasMentionPrefix(m_client.CurrentUser, ref argPos) || message.HasCharPrefix(GlobalConfiguration.CommandPrefix, ref argPos)))
            {
                // messageRetry.RegisterFailedCommand(arg.Id);
                return;
            }

            // Create a Command Context
            var context = new CommandContext(m_client, message);
            // Execute the Command, store the result
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            try
            {
                // If the command failed
                if (!result.IsSuccess)
                {
                    if (result is ExecuteResult exeResult)
                    {
                        logger.LogError(exeResult.Exception, result.ErrorReason);
                    }

                    // log the error
                    Discord.LogMessage errorMessage = new Discord.LogMessage(Discord.LogSeverity.Warning, "CommandHandler", result.ErrorReason);
                    await Log(errorMessage);
                    // don't actually reply back with the error

                    // todo reply back with an error message that corresponds to the closest matching command name
                }
            }
            catch (Exception e)
            {
                var error = new LogMessage(LogSeverity.Error, "CommandHandler", "Caught exception", e);
                await Log(error);
            }
        }

        private async Task Client_Ready()
        {
            // display a helpful invite url in the log when the bot is ready
            var application = await m_client.GetApplicationInfoAsync();
            await Log(new LogMessage(LogSeverity.Info, "Program",
                $"Invite URL: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>"));
        }

        // todo: remove this and use DI instead of statics
        public async static Task Log(Discord.LogMessage arg)
        {
            logger?.Log(arg.Severity.ToLogLevel(), exception: arg.Exception, message: arg.Message, arg.Source, arg.Exception);
            var appInsightsSeverity = ConvertLogSeverity(arg.Severity);
            if (appInsightsSeverity != null && telemetryClient != null)
            {
                // this should be initialized once, clean it up later
                var properties = new Dictionary<string, string>()
                {
                    {nameof(arg.Exception), arg.Exception?.ToString() },
                    {nameof(arg.Message), arg.Message?.ToString() },
                    {nameof(arg.Severity), arg.Severity.ToString() },
                    {nameof(arg.Source), arg.Source?.ToString() },
                };

                if (arg.Exception != null)
                {
                    telemetryClient.TrackException(arg.Exception, properties: properties);
                }

                telemetryClient.TrackTrace(arg.Message, appInsightsSeverity.Value, properties: properties);
            }

            if (arg.Severity <= Discord.LogSeverity.Debug)
            {
                // log stuff to console
                // could also log to a file if needed later on
                Console.WriteLine(arg.ToString());
            }
        }

        private static SeverityLevel? ConvertLogSeverity(Discord.LogSeverity discordSeverity)
        {
            var dict = new Dictionary<Discord.LogSeverity, SeverityLevel>()
            {
                { Discord.LogSeverity.Critical, SeverityLevel.Critical },
                // do not log Debug
                // { Discord.LogSeverity.Debug, SeverityLevel.Verbose },
                { Discord.LogSeverity.Error, SeverityLevel.Error },
                { Discord.LogSeverity.Info, SeverityLevel.Information },
                { Discord.LogSeverity.Verbose, SeverityLevel.Verbose },
                { Discord.LogSeverity.Warning, SeverityLevel.Warning },
            };

            if (dict.ContainsKey(discordSeverity))
            {
                return dict[discordSeverity];
            }
            return null;
        }
    }
}

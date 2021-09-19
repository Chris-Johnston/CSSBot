using CSSBot.Services.TheSpookening.Models;
using Discord;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSSBot.Services.TheSpookening
{
    public class SpookeningService
    {
        private readonly SpookConfiguration Configuration;

        public ulong TargetGuildId
            => Configuration.TargetGuildId;

        // where spook announcements are posted
        public ulong MessageChannelId
            => Configuration.MessageChannelId;

        // role to add to spooked users
        public ulong SpookyRoleId
            => Configuration.SpookyRoleId;

        public int SpookUserLimit
            => Configuration.SpookUserLimit;

        // let these users break the rules
        private List<ulong> OverrideUsers
            => Configuration.OverrideUserIds;

        private List<string> SpookyEmojis
            => Configuration.SpookyEmojis;

        private List<string> Jokes
            => Configuration.SpookyJokes;

        public string GetRandomSpookyJoke
            => Jokes[random.Next(0, Jokes.Count - 1)];

        // format strings for nicknames, or just spooky nicknames
        private List<string> NicknameFormatters
            => Configuration.NicknameFormatters;

        private string GetRandomNicknameFormatter
            => NicknameFormatters[random.Next(0, NicknameFormatters.Count - 1)];

        private Random random;

        // poll a timer every minute to check if midnight
        // this is not the ideal way to do this, but probably is fine
        private const int PollRate = 60000;
        private Timer MidnightTimer;

        private LiteDatabase database;
        private const string SpookedUserCollection = "SpookedUser";
        private const string SpookQueueCollection = "SpookedQueueCollection";
        private const string UsageLog = nameof(UsageLog) + "Collection";
        private readonly DiscordSocketClient client;

        private ILiteCollection<SpookedUser> GetSpookedUserCollection
            => database.GetCollection<SpookedUser>(SpookedUserCollection);

        private ILiteCollection<SpookQueue> SpookUserQueue
            => database.GetCollection<SpookQueue>(SpookQueueCollection);

        private ILiteCollection<UsageLog> UsageCollection
            => database.GetCollection<UsageLog>(UsageLog);

        private readonly ILogger logger;

        public SpookeningService(DiscordSocketClient client, LiteDatabase database, string configFilePath, ILogger logger)
        {
            this.client = client;
            this.database = database;
            this.logger = logger;

            // hack: too lazy to set up a json when testing
            if (configFilePath == null) return;

            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new ArgumentNullException(nameof(configFilePath), "Config file path must be specified.");
            if (!File.Exists(configFilePath))
                throw new ArgumentException("Config file path was not found.");

            // let json deserialize throw any errors it encounters
            var fileContent = File.ReadAllText(configFilePath);
            Configuration = JsonConvert.DeserializeObject<SpookConfiguration>(fileContent);

            random = new Random();

            this.client.MessageReceived += MimicSpookyEmojiWithReactions;

            MidnightTimer = new Timer(_ =>
            {
                if (DateTime.Now.Month == 10)
                {
                    if (IsTimeMidnight(DateTime.Now.TimeOfDay) && DateTime.Now.Day == 31)
                    {
                        logger?.LogInformation("Halloween Midnight timer triggered");
                        OnHalloweenMidnight();
                    }
                    else if (IsTimeMidnight(DateTime.Now.TimeOfDay))
                    {
                        logger?.LogInformation("Midnight timer triggered");
                        OnMidnight();
                    }
                    else
                    {
                        // not midnight
                    }
                }
                else
                {
                    // not October

                    // reset all of the names a few days later
                    if (DateTime.Now.Month == 11 && DateTime.Now.Day == 3 && DateTime.Now.Minute == 1 && DateTime.Now.Hour == 0)
                    {
                        // 2021 - users change their own names, so let them deal with it
                        // logger?.LogWarning("Starting reset all names task.");
                        // Task.Factory.StartNew(() => ResetAllNames());
                    }
                    else if (DateTime.Now.Month == 12 && DateTime.Now.Day == 3 && DateTime.Now.Minute == 1 && DateTime.Now.Hour == 0)
                    {
                        logger?.LogWarning("DROPPING Spookening DB because it is Dec.");
                        // drop db after a month
                        DropSpookDatabase();
                    }
                }
            }, null, 0, PollRate);

            logger?.LogDebug("Initialized spookening service.");
        }

        public void DropSpookDatabase()
        {
            // drop collections of both tables
            this.database.DropCollection(SpookedUserCollection);
            this.database.DropCollection(SpookQueueCollection);
            this.database.DropCollection(UsageLog);
        }

        // checks to see if the userId specified is within the rate limit
        public int GetRateLimitCount(string actionType, ulong? userId, TimeSpan timeRange)
        {
            return UsageCollection
                .Find(x =>
                    (x.UserId == userId || userId == null) && x.ActionType == actionType && x.Timestamp > DateTimeOffset.UtcNow.Add(-timeRange))
                .Count();
        }

        public void RegisterAction(string actionType, ulong userId)
        {
            UsageCollection
                .Insert(new UsageLog()
                {
                    ActionType = actionType,
                    UserId = userId,
                    Timestamp = DateTimeOffset.UtcNow,
                });
        }

        public bool CheckUserRerollName(ulong userId)
        {
            var count = GetRateLimitCount("reroll", userId, TimeSpan.FromHours(18));
            var globalCount = GetRateLimitCount("reroll", null, TimeSpan.FromHours(2));
            var result = count < 3 && globalCount < 10;
            if (result)
            {
                RegisterAction("reroll", userId);
            }
            return result;
        }

        public bool CheckUserJoke(ulong userId)
        {
            var count = GetRateLimitCount("joke", userId, TimeSpan.FromHours(18));
            var globalCount = GetRateLimitCount("joke", null, TimeSpan.FromHours(2));
            var result = count < 5 && globalCount < 3;
            if (result)
            {
                RegisterAction("joke", userId);
            }
            return result;
        }

        public bool CheckUserDoot(ulong userId) // also use for spoop
        {
            // yes I am copy pasting and no I do not care
            // could clean this up later
            var count = GetRateLimitCount("doot", userId, TimeSpan.FromHours(2));
            var globalCount = GetRateLimitCount("doot", null, TimeSpan.FromHours(2));
            var result = count < 3 && globalCount < 5;
            if (result)
            {
                RegisterAction("doot", userId);
            }
            return result;
        }

        // reacts to emojis with the same emojis if user is spooky
        private async Task MimicSpookyEmojiWithReactions(SocketMessage arg)
        {
            // ignore bots
            if (arg.Author.IsBot)
                return;

            if (DateTime.Now.Month == 10)
            {
                List<Emoji> toReact = new List<Emoji>();

                if (CanUserUseSpookyCommands(arg.Author.Id))
                {
                    foreach (var emoji in SpookyEmojis)
                    {
                        if (arg.Content.Contains(emoji))
                        {
                            toReact.Add(new Emoji(emoji));
                        }
                    }
                }
                var message = arg as SocketUserMessage;

                await Task.Factory.StartNew(async () =>
                {
                    foreach (var e in toReact)
                    {
                        await message.AddReactionAsync(e);
                        // don't block other rate limits so delay this a bit too
                        await Task.Delay(500);
                    }
                });
            }
        }

        public void ResetAllNames()
        {
            //foreach (var item in GetSpookedUserCollection.FindAll())
            //{
            //    Thread.Sleep(5000);
            //    ResetNickname(item.SpookedUserId);
            //}
        }

        /// <summary>
        /// Checks the given time to see if it's midnight,
        /// by hour and minute.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool IsTimeMidnight(TimeSpan time)
            => time.Hours == 22 && time.Minutes == 0;

        public void FixQueue()
        {
            foreach (var spook in SpookUserQueue.FindAll())
            {
                // expire everything in the queue
                spook.Expired = true;
                SpookUserQueue.Update(spook);
            }
        }

        public async Task ProcessSpooking()
        {
            // process spookenings that have been issued earlier that day
            foreach (var spook in SpookUserQueue.Find(x => !x.Expired))
            {
                // double check that the user isn't already spooked
                if (IsUserSpooked(spook.UserToSpookId))
                    continue;

                var guild = client.GetGuild(TargetGuildId);
                await guild.DownloadUsersAsync();

                var user = guild.GetUser(spook.UserToSpookId);
                var by = guild.GetUser(spook.SpookedById);


                if (user == null)
                {
                    logger.LogWarning($"User `user` [{spook.UserToSpookId}] was null.");

                    // could potentially result in this repeating a lot?
                    continue;
                }

                if (by == null)
                {
                    logger.LogWarning($"User `by` [{spook.SpookedById}] was null.");
                    continue;
                }

                SpookUser(user, by);

                // mark this item as expired
                spook.Expired = true;
                // update the database
                SpookUserQueue.Update(spook);
            }
        }

        /// <summary>
        /// respooks a user
        /// </summary>
        /// <param name="userId"></param>
        public async void RespookUser(ulong userId)
        {
            // ensure user is already spooked
            if (IsUserSpooked(userId))
            {
                // get their original name
                var user = GetSpookedUserCollection.FindOne(x => x.SpookedUserId == userId);
                // get the discord user to modify
                var discordUser = client.GetGuild(TargetGuildId).GetUser(userId);

                // the original nickname may be null, if the user didn't already have a nickname
                var originalName = string.IsNullOrWhiteSpace(user.OriginalNickName) ? discordUser.Username : user.OriginalNickName;
                var newName = string.Format(GetRandomNicknameFormatter, originalName, ReverseString(originalName));

                var safeOriginalName = SanitizeNickname(originalName);
                var safeNewName = SanitizeNickname(newName);

                var message =
                    $"Uh-oh! **{safeOriginalName}** has been re-spooked and is now **{safeNewName}**! SpooOoOoooKy!";

                var _ = Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        // just in case their nickname is too long
                        await discordUser.ModifyAsync(x => x.Nickname = Truncate(newName, 32));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                
                // send a message
                var _2 = Task.Factory.StartNew(async () => await SendSpookMessage(message));
            }
        }

        /// <summary>
        /// Fires on midnight each day in October
        /// </summary>
        private void OnMidnight()
        {
            for (int i = 0; i < 2; i++)
            {
                // get a random user
                var user = GetRandomUser();

                // if no users left, then just do nothing
                if (user == null) return;

                SpookUser(user);
            }
            

            // process the queue of spooked people
            ProcessSpooking()
                .GetAwaiter()
                .GetResult();

            Task.Factory.StartNew(async () =>
            {
                Thread.Sleep(1000);
                await SendSpookMessage(
@"Spooked Users have access to the following commads:
```
?Spook <@User> - Spooks a user the following night.
?Doot - Doot.
?SpookyJoke - Tells a spooky joke.
?ThankMrSkeletal - Chooses a new (spooky) nickname.
```
"
);
            });
        }

        /// <summary>
        /// Queues a user to be spooked
        /// </summary>
        /// <param name="user"></param>
        /// <param name="by"></param>
        public void QueueSpooking(ulong user, ulong by)
        {
            if (!SpookUserQueue.Exists(x => x.UserToSpookId == user))
            {
                SpookUserQueue.Insert(new SpookQueue()
                {
                    SpookedById = by,
                    UserToSpookId = user,
                    Expired = false
                });
            }
        }

        private string Truncate(string value, int maxLength)
            => value.Length <= maxLength? value : value.Substring(0, maxLength);

        private const char ZeroWidthSpace = '\x200b';

        /// <summary>
        /// Sanitizes a nickname so that it cannot ping people
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string SanitizeNickname(string name)
            => name.Replace("@", $"@{ZeroWidthSpace}");

        private string GetNameFromUser(IGuildUser user)
            => string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;

        private static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private void SpookUser(SocketGuildUser user)
        {
            //var message =
            //    $"Uh-oh! **{safeOriginalName}** has been spooked and is now **{safeNewName}**! AHH! So scary!\n" +
            //    $"\n{user.Mention} can now spook up to **{SpookUserLimit}** other people with `?spook @User`.";
            // 2021 do not update nicknames
            var message =
                $"Uh-oh! {user.Mention} has been spooked! AHH! {user.Mention} can not spook up to **{SpookUserLimit}** other people with `?spook @User`.";

            // don't care how bad this code is
            var _ = Task.Factory.StartNew(async () =>
            {
                await AddUserRoleAsync(user, $"Spooked by bot");
            });

            RegisterSpookedUser(user.Id, user.Nickname);

            var _2 = Task.Factory.StartNew(async () => await SendSpookMessage(message));
        }

        public bool IsUserSpooked(ulong userId)
            => GetSpookedUserCollection.Exists(x => x.SpookedUserId == userId);

        public bool IsUserAlreadyQueued(ulong userId)
            => SpookUserQueue.Exists(x => x.UserToSpookId == userId && !x.Expired);

        public bool CanUserUseSpookyCommands(ulong userId)
            => IsUserSpooked(userId) || OverrideUsers.Contains(userId) || (DateTime.Now.Month == 10 && DateTime.Now.Day == 31);

        public void RegisterSpookedUser(ulong user, string name, ulong? by = null)
        {
            // check that the user hasn't already been spooked
            // if so, do nothing
            if (IsUserSpooked(user))
                return;

            GetSpookedUserCollection.Insert(new SpookedUser()
            {
                SpookedByUserId = by,
                SpookedTime = DateTime.Now,
                SpookedUserId = user,
                OriginalNickName = name,
            });
        }

        private void SpookUser(SocketGuildUser user, SocketGuildUser by)
        {
            // 2021, do not automatically rename users
            //var message =
            //    $"Uh-oh! **{safeOriginalName}** has been spooked by **{by.Mention}** and is now **{safeNewName}**! Yikes!\n" +
            //    $"\n{user.Mention} can now spook up to **{SpookUserLimit}** other people with `?spook @User`.";
            var message =
                $"Uh-oh! {user.Mention} has ben spooked by {by.Mention}! Yikes!";

            // don't care
            var _ = Task.Factory.StartNew(async () =>
            {
                await AddUserRoleAsync(user, $"Spooked by {by.Mention}");
            });

            RegisterSpookedUser(user.Id, user.Nickname, by.Id);

            var _2 = Task.Factory.StartNew(async () => await SendSpookMessage(message));
        }

        private async Task SendSpookMessage(string message)
        {
            var channel = client.GetGuild(TargetGuildId).GetTextChannel(MessageChannelId);
            await channel.SendMessageAsync(message);
        }

        private async Task AddUserRoleAsync(SocketGuildUser user, string auditLogReason = null)
        {
            var role = client.GetGuild(TargetGuildId).GetRole(SpookyRoleId);

            string logReason = auditLogReason ?? "BOO!";

            try
            {
                await user.AddRoleAsync(role, options: new RequestOptions() { AuditLogReason = logReason });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool DoesUserHaveSpooksRemaining(ulong userId)
            => OverrideUsers.Contains(userId) || SpookUserQueue.Count(x => x.SpookedById == userId) < SpookUserLimit;
        /// <summary>
        /// Fires on the Midnight on 10/31.
        /// This will fire on Tuesday night so that it's effects
        /// can be seen on Wednesday (day of Halloween)
        /// </summary>
        private void OnHalloweenMidnight()
        {
            var message = "🎃 **It's Halloween** 🎃\nNow everyone has access to the spooky commands, just for today. Nicknames will reset in a few days.\nhttps://www.youtube.com/watch?v=viMWnEOYN_U";

            Task.Factory.StartNew(async () => await SendSpookMessage(message));
        }

        public void ResetNickname(ulong userId)
        {
            var item = GetSpookedUserCollection.FindOne(x => x.SpookedUserId == userId);
            var originalName = item.OriginalNickName;

            var guild = client.GetGuild(TargetGuildId);

            Task.Factory.StartNew(async () =>
            {
                var user = guild.GetUser(userId);
                await user.ModifyAsync(x =>
                {
                    x.Nickname = originalName;
                });
            });
        }

        /// <summary>
        /// Gets a random user that is not a bot, override user, or a user
        /// that has been spooked already.
        /// </summary>
        /// <returns></returns>
        private SocketGuildUser GetRandomUser()
        {
            var guild = client.GetGuild(TargetGuildId);
            var users = new List<SocketGuildUser>(guild.Users);
            // remove the bots
            users.RemoveAll(x => x.IsBot);
            // remove all override users, if they are server
            // owners then they cannot be spooked anyways
            users.RemoveAll(x => OverrideUsers.Contains(x.Id));
            // remove all users who have been spooked already
            var collection = GetSpookedUserCollection;
            users.RemoveAll(x =>
            {
                return collection.Exists(y => y.SpookedUserId == x.Id);
            });

            // if none left, then skip operation
            if (users.Count == 0) return null;

            // get a random user
            return users[random.Next(0, users.Count)];
        }

        public void ForceSpookOverride(ulong userId, string originalNick)
        {
            // remove this user from being in the queue
            SpookUserQueue.DeleteMany(x => x.UserToSpookId == userId);
            // and from the table if they are already for some reason
            GetSpookedUserCollection.DeleteMany(x => x.SpookedUserId == userId);

            GetSpookedUserCollection.Insert(new SpookedUser()
            {
                OriginalNickName = originalNick, SpookedUserId = userId,
                SpookedTime = DateTime.Now, 
                SpookedByUserId = null
            });
        }
    }
}

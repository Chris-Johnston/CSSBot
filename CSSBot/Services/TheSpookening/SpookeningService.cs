using CSSBot.Services.TheSpookening.Models;
using Discord;
using Discord.WebSocket;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSSBot.Services.TheSpookening
{
    public class SpookeningService
    {
        // hard coded variables because this is only to be used in a single
        // server for a month

        // spookening service will only work in this server
        //private const ulong TargetGuildId = 297485054836342786;
        public const ulong TargetGuildId = 428476681519497218;
        // only send messages to this text channel
        //private const ulong MessageChannelId = 297485054836342786;
        private const ulong MessageChannelId = 429115833554567188;

        private const int SpookUserLimit = 2;

        // let these users break the rules
        private readonly List<ulong> OverrideUsers = new List<ulong>()
        {
            163184946742034432
        };

        // format strings for nicknames, or just spooky nicknames
        private readonly List<string> NicknameFormatters = new List<string>()
        {
            "Spooky {0}",
            "{0}, but spooky",
            "{0}!!!",
            "a ghost",
            "{0}'s skeleton",
            "deploying to prod without testing",
            "🕸️{0}🕸️",
            "🕷️{0}🕷️",
            "🦇{0}🦇",
            "zombie {0}",
            "{0}, but a vampire",
            "franken-{0}",
            "🌚",
            "☠️{0}☠️",
            "💀{0}💀",
            "this person is actually dead now",
            "👻spooky👻",
            "👻{0}👻",
            "🤡",
            "👽{0}👽",
            "🧛{0}🧛",
            "🧟{0}🧟",
            "🎃🎃🎃🎃🎃🎃🎃",
            "🎃{0}🎃",
            "{0}'s ghost",
            "<something spooky goes here>",
            "a skeleton",
            "a zombie",
            "forgetting to lock your car",
            "leaving the iron on",
            "sql injection",
            "cross-site scripting",
            "unpaid student loans",
            "git push --force",
            "headless {0}",
            "a 5 page research paper",
            "no documentation",
            "writing documentation",
            "null reference exceptions",
            "memory leaks",
            "compiler errors",
            "typo in your resume",
            "{0}'s clone",
            "still just {0}",
            "ahh! {0}",
            "🔥{0}🔥",
            "merge conflicts",
            "{0} is not alivent",
            "🦋 🔜 💡",
            "v a p o r {0} w a v e",
            "🕸️",
            "rolled critical fail",
            // backwards
            "{1}",
            "💡 moth 💡",
            "slaps roof of {0}"
        };

        private string GetRandomNicknameFormatter
            => NicknameFormatters[random.Next(0, NicknameFormatters.Count)];

        private Random random;

        // poll a timer every minute to check if midnight
        // this is not the ideal way to do this, but probably is fine
        private const int PollRate = 60000;
        private Timer MidnightTimer;

        private LiteDatabase database;
        private const string SpookedUserColllection = "SpookedUser";
        private const string SpookQueueCollection = "SpookedQueueCollection";
        private readonly DiscordSocketClient client;

        private LiteCollection<SpookedUser> GetSpookedUserCollection
            => database.GetCollection<SpookedUser>(SpookedUserColllection);

        private LiteCollection<SpookQueue> SpookUserQueue
            => database.GetCollection<SpookQueue>(SpookQueueCollection);

        public SpookeningService(DiscordSocketClient client, LiteDatabase database)
        {
            this.client = client;
            this.database = database;
            random = new Random();

            MidnightTimer = new Timer(_ =>
            {
                if (DateTime.Now.Month == 10)
                {
                    if (IsTimeMidnight(DateTime.Now.TimeOfDay) && DateTime.Now.Day == 31)
                    {
                        OnHalloweenMidnight();
                    }
                    else if (IsTimeMidnight(DateTime.Now.TimeOfDay))
                    {
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
                        Task.Factory.StartNew(() => ResetAllNames());
                    }
                }
            }, null, 0, PollRate);
            
        }

        public void ResetAllNames()
        {
            foreach (var item in GetSpookedUserCollection.FindAll())
            {
                Thread.Sleep(5000);
                ResetNickname(item.SpookedUserId);
            }
        }

        /// <summary>
        /// Checks the given time to see if it's midnight,
        /// by hour and minute.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool IsTimeMidnight(TimeSpan time)
            => time.Hours == 0 && time.Minutes == 0;

        /// <summary>
        /// Fires on midnight each day in October
        /// </summary>
        private void OnMidnight()
        {
            // get a random user
            var user = GetRandomUser();
            
            // if no users left, then just do nothing
            if (user == null) return;

            SpookUser(user);

            // process spookenings that have been issued earlier that day
            foreach (var spook in SpookUserQueue.Find(x => !x.Expired))
            {
                var by = client.GetGuild(TargetGuildId).GetUser(spook.SpookedById);
                SpookUser(user, by);

                // remove the item from the collection
                spook.Expired = true;
            }

            Task.Factory.StartNew(async () =>
            {
                await SendSpookMessage(
@"Spooked Users have access to the following commads:
```
?Spook <@User>
?Doot
?SpookyJoke
```

More commands may be addded.
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
                    UserToSpookId = user
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

        private void SpookUser(SocketGuildUser user)
        {
            var originalName = user.Nickname ?? user.Username;

            string reverse(string input)
            {
                char[] charArray = input.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }

            var newName = string.Format(GetRandomNicknameFormatter, originalName, reverse(originalName));

            var safeOriginalName = SanitizeNickname(originalName);
            var safeNewName = SanitizeNickname(newName);

            var message =
                $"Uh-oh! **{safeOriginalName}** has been spooked and is now **{safeNewName}**! AHH! So scary!\n" +
                $"\n{user.Mention} can now spook up to **{SpookUserLimit}** other people with `?spook @User`.";

            var _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await user.ModifyAsync(x => x.Nickname = Truncate(newName, 32));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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
            var originalName = user.Nickname ?? user.Username;
            var newName = string.Format(GetRandomNicknameFormatter, originalName);

            var safeOriginalName = SanitizeNickname(originalName);
            var safeNewName = SanitizeNickname(newName);

            var message =
                $"Uh-oh! **{safeOriginalName}** has been spooked by **{by.Mention}** and is now **{safeNewName}**! Yikes!\n" +
                $"\n{user.Mention} can now spook up to **{SpookUserLimit}** other people with `?spook @User`.";

            var _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    // just in case their nickname is too long
                    await user.ModifyAsync(x => x.Nickname = Truncate(newName, 32));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            RegisterSpookedUser(user.Id, user.Nickname, by.Id);

            var _2 = Task.Factory.StartNew(async () => await SendSpookMessage(message));
        }

        private async Task SendSpookMessage(string message)
        {
            var channel = client.GetGuild(TargetGuildId).GetTextChannel(MessageChannelId);
            await channel.SendMessageAsync(message);
        }

        public bool DoesUserHaveSpooksRemaining(ulong userId)
            => SpookUserQueue.Count(x => x.SpookedById == userId) < SpookUserLimit;
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
    }
}

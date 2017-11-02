using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    public class SpookyCommands : ModuleBase
    {
        private string[] _HalloweenEmoji = new string[]
        {
            // ghost
            "👻",
            // skull
            "💀",
            // spider web
            "🕸️",
            "🕷",
            // spider
            "🕷️",
            // bat
            "🦇",
            // jack o lantern
            "🎃",
            // lightning bolt
            "⚡",
            // doot
            "🎺"
        };

        // wait 30s before spooking another person
        private TimeSpan TimeUntilNext = TimeSpan.FromSeconds(30);
        private DateTime LastTime = DateTime.MinValue;

        private string GetRandomEmoji()
        {
            Random r = new Random();
            return _HalloweenEmoji[r.Next(_HalloweenEmoji.Length)];
        }

        private bool CheckIfOctober()
        {
            return DateTime.Now.Month == 10;
        }

        [Command("Doot")]
        [Alias("Skull Trumpet")]
        public async Task Doot()
        {
            await Context.Message.AddReactionAsync(new Emoji("💀"));
            await Context.Message.AddReactionAsync(new Emoji("🎺"));

            await ReplyAsync(@"https://www.youtube.com/watch?v=eVrYbKBrI7o");
        }

        [Command("UnSpook")]
        [Alias("UnScare")]
        [Summary("Un-Spooks a role.")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner()]
        public async Task UnSpookRole([Name("Role")] IRole role)
        {
            foreach(IGuildUser user in await Context.Guild.GetUsersAsync())
            {
                // if the user has a nickname set
                if(user.Nickname != null)
                {
                    await Bot.Log(new LogMessage(LogSeverity.Info, "SpookyCommands", "Un-Spooking user " + user.Username));

                    string newNick = user.Nickname;
                    // replace each instance of the emoji with a blank
                    foreach(string s in _HalloweenEmoji)
                    {
                        newNick = newNick.Replace(s, "");
                    }

                    // set their unspooked nickname
                    // :(
                    try
                    {
                        await user.ModifyAsync(x =>
                        {
                            x.Nickname = newNick;
                        });
                    }
                    catch(Exception e)
                    {
                        // might have thrown permissions exception
                    }
                }
            }

            await ReplyAsync("Uh-oh! Looks like " + role.Mention + " has been un-spooked!");
        }

        [Command("Spook")]
        [Alias("Scare")]
        [Summary("Spooks a role.")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner()] // never forget the spam
        public async Task SpookRole([Name("Role")] IRole role)
        {
            foreach(IGuildUser user in await Context.Guild.GetUsersAsync())
            {
                // determine if each user in the guild has a match to the role
                bool match = false;
                foreach(ulong id in user.RoleIds)
                {
                    if (id == role.Id)
                        match = true;
                }

                // if they do
                if(match)
                {
                    // then update their user name
                    // prevent emoji spam
                    bool alreadySpooked = false;
                    foreach (string s in _HalloweenEmoji)
                    {
                        if (user.Nickname != null && user.Nickname.Contains(s))
                        {
                            alreadySpooked = true;
                        }
                    }

                    // update it if they don't already have spooky stuff
                    if (!alreadySpooked)
                    {
                        try
                        {
                            await user.ModifyAsync(x =>
                            {
                                x.Nickname += (user.Nickname ?? user.Username) + GetRandomEmoji();
                            });
                        }
                        // catch permissions exceptions
                        catch(Exception e)
                        { }
                    }
                }
            }

            // let everyone know
            string returnMessage = string.Format("💀💀💀 Uh oh! 💀💀💀\n\n{0} has been spooked!", role.Name);
            await ReplyAsync(returnMessage);
        }

        [Command("Spook")]
        [Alias("Scare")]
        [Summary("Spooks a user.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task Spook([Name("User")]IGuildUser user)
        {
            if (CheckIfOctober())
            {
                if (DateTime.Now.Subtract(TimeUntilNext).CompareTo(LastTime) > 0)
                {
                    LastTime = DateTime.Now;
                    // prevent emoji spam
                    bool alreadySpooked = false;
                    foreach(string s in _HalloweenEmoji)
                    {
                        if(user.Nickname != null && user.Nickname.Contains(s))
                        {
                            alreadySpooked = true;
                        }
                    }

                    if (!alreadySpooked)
                    {
                        string replyMessage = string.Format(
                            "💀💀💀 Uh oh! 💀💀💀\n\n{0} has been spooked!",
                            user.Mention
                            );
                        await user.ModifyAsync(x =>
                        {
                            x.Nickname += (user.Nickname ?? user.Username) + GetRandomEmoji();
                        });

                        await ReplyAsync(replyMessage);

                        LastTime = DateTime.Now;
                    }
                }
            }
            else
            {
                await ReplyAsync("Try again in October!");
            }
        }
    }
}

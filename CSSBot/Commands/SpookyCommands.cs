using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    //todo don't load this automatically but only load it if in october
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
            // spider
            "🕷️",
            // bat
            "🦇",
            // jack o lantern
            "🎃",
            // lightning bolt
            "⚡",
            // doot
            "💀🎺"
        };

        // wait 30s before spooking another person
        private readonly TimeSpan TimeUntilNext = TimeSpan.FromSeconds(30);
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
                if (DateTime.Now - TimeUntilNext > LastTime)
                {
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

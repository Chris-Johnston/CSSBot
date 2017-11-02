using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    public class FestiveCommands : ModuleBase
    {
        // various festive winter themed emoji
        // including but not limited to christmas
        private string[] _FestiveEmoji = new string[]
        {
            // why is there an emoji for snowman with snow
            // and snowman without snow
            "🌟", "❄️", "☃️", "⛄", "⛄", "🎄", "🕎", "✡️",
            "🦃", "🍁"
        };

        private bool CheckDate()
        {
            // allow for month 11 - 1, however january is really pushing it
            return DateTime.Now.Month > 10 || DateTime.Now.Month == 1;
        }

        public bool DoesStringContainEmoji(string str)
        {
            // just allow it for now
            return true;

            foreach(string s in _FestiveEmoji)
            {
                if (str.Contains(s))
                    return true;
            }
            return false;
        }

        private Random r = new Random();

        private string GetRandom()
        {
            return _FestiveEmoji[r.Next(_FestiveEmoji.Length)];
        }

        [Command("UnFestive")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ChangeNickname)]
        public async Task UnFestive(IRole role)
        {
            foreach(IGuildUser user in await Context.Guild.GetUsersAsync())
            {
                bool inRole = false;
                foreach(ulong id in user.RoleIds)
                {
                    if (id == role.Id)
                        inRole = true;
                }

                if(user.Nickname != null && inRole)
                {
                    // blank out the festive emoji from the names
                    string newNick = user.Nickname;
                    foreach(string s in _FestiveEmoji)
                    {
                        newNick = newNick.Replace(s, "");
                    }

                    try
                    {
                        await user.ModifyAsync(x =>
                       {
                           x.Nickname = newNick;
                       });
                    }
                    catch(Exception e)
                    {
                        // permission exceptions
                    }
                }
            }

            string replystr = string.Format(
                "Uh-oh! {0} was made un-festive!", role.Mention);
            await ReplyAsync(replystr);
        }

        [Command("UnFestive")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ChangeNickname)]
        public async Task UnFestiveSelf()
        {
            var user = Context.User as IGuildUser;

            if (user.Nickname != null)
            {
                string newNick = user.Nickname;

                foreach (string s in _FestiveEmoji)
                {
                    newNick = newNick.Replace(s, "");
                }

                // this sometimes doesn't work with IGuildUsers
                // unsure why specifically
                try
                {
                    await user.ModifyAsync(x =>
                    {
                        x.Nickname = newNick;
                    });
                }
                catch (Exception e)
                {
                    return;
                }
                string replyStr = string.Format(
                    "Uh-oh! {0} has been made un-festive!",
                    user.Mention);
                await ReplyAsync(replyStr);
            }
        }

        [Command("UnFestive")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task UnFestive(IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;

            if(user.Nickname != null)
            {
                string newNick = user.Nickname;
                
                foreach(string s in _FestiveEmoji)
                {
                    newNick = newNick.Replace(s, "");
                }

                // this sometimes doesn't work with IGuildUsers
                // unsure why specifically
                try
                {
                    await user.ModifyAsync(x =>
                   {
                       x.Nickname = newNick;
                   });
                }
                catch(Exception e)
                {
                    return;
                }
                string replyStr = string.Format(
                    "Uh-oh! {0} has been made un-festive!",
                    user.Mention);
                await ReplyAsync(replyStr);
            }
        }

        [Command("Festive")]
        [Alias("Santa")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ChangeNickname)]
        public async Task FestiveSelf()
        {
            var user = Context.User as IGuildUser;
            if (CheckDate())
            {
                string name = user.Nickname ?? user.Username;
                if (!DoesStringContainEmoji(name))
                {
                    name += GetRandom();

                    try
                    {
                        await user.ModifyAsync(x =>
                        {
                            x.Nickname = name;
                        });
                    }
                    catch (Exception e)
                    {
                        return;
                    }

                    string replyStr = string.Format(
                    "⛄⛄⛄ Uh oh! ⛄⛄⛄\n\n{0} has been ~~spooked~~ made festive!"
                    , user.Mention
                    );
                    await ReplyAsync(replyStr);
                }
            }
            else
            {
                await ReplyAsync("Try again between November and January!");
            }
        }

        [Command("Festive")]
        [Alias("Santa")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task FestiveUser(IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;

            if (CheckDate())
            {
                string name = user.Nickname ?? user.Username;
                if (!DoesStringContainEmoji(name))
                {
                    name += GetRandom();

                    try
                    {
                        await user.ModifyAsync(x =>
                        {
                            x.Nickname = name;
                        });
                    }
                    catch (Exception e)
                    {
                        return;
                    }

                    string replyStr = string.Format(
                    "⛄⛄⛄ Uh oh! ⛄⛄⛄\n\n{0} has been ~~spooked~~ made festive by {1}!"
                    , user.Mention, Context.User.Mention
                    );
                    await ReplyAsync(replyStr);
                }
            }
            else
            {
                await ReplyAsync("Try again between November and January!");
            }
        }

        // make an entire role festive
        [Command("Festive")]
        [Alias("Santa")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner()]
        public async Task FestiveRole(IRole role)
        {
            if (CheckDate())
            {
                foreach (IGuildUser user in await Context.Guild.GetUsersAsync())
                {
                    bool hasRole = false;
                    foreach(var r in user.RoleIds)
                    {
                        if (role.Id == r)
                            hasRole = true;
                    }

                    if (hasRole)
                    {

                        string name = user.Nickname ?? user.Username;

                        if (!DoesStringContainEmoji(name))
                        {
                            name += GetRandom();

                            // try to update their name
                            try
                            {
                                await user.ModifyAsync(x =>
                               {
                                   x.Nickname = name;
                               });
                            }
                            catch (Exception e)
                            {
                                // permissions exceptions
                            }
                        }
                    }
                }

                string replyStr = string.Format(
                        "⛄⛄⛄ Uh oh! ⛄⛄⛄\n\n{0} has been ~~spooked~~ made festive!",
                        role.Mention
                        );

                await ReplyAsync(replyStr);
            }
            else
            {
                await ReplyAsync("Try again between November and January!");
            }
        }
    }
}

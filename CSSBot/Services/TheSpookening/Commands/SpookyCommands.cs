using CSSBot.Services.TheSpookening;
using Discord;
using Discord.Commands;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    public class SpookyCommands : ModuleBase
    {
        private readonly Random random = new Random();

        //private readonly LiteDatabase database;
        private readonly SpookeningService spookening;

        public SpookyCommands(LiteDatabase db, SpookeningService spookyService)
        {
            // database = db;
            spookening = spookyService;
        }
        // this previously contained many commands for user nickname manipulation,
        // but that got really messy quick and turned out to be a bad idea

        [Command("ClearSpookedUserCollection")]
        [RequireOwner]
        public async Task ResetSpookedUsers()
        {
            spookening.DropSpookDatabase();
            await ReplyAsync("k");
        }

        /// <summary>
        /// Adds the skull and trumpet emoji as a reaction to the user's
        /// command message and replies back with the youtube link for
        /// the skull trumpet video
        /// </summary>
        /// <returns></returns>
        [Command("Doot")]
        [Alias("SkullTrumpet")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ViewChannel | GuildPermission.SendMessages)]
        public async Task Doot()
        {
            // actually this one is fine
            //if (Context.Guild.Id != SpookeningService.TargetGuildId)
            //{
            //    await ReplyAsync("sry wrong server");
            //    return;
            //}

            // if october
            if (DateTime.Now.Month == 10)
            {
                if (spookening.CanUserUseSpookyCommands(Context.User.Id))
                {
                    if (spookening.CheckUserDoot(Context.User.Id))
                    {
                        await Context.Message.AddReactionAsync(new Emoji("💀"));
                        await Context.Message.AddReactionAsync(new Emoji("🎺"));

                        await ReplyAsync("doot doot\nhttps://www.youtube.com/watch?v=eVrYbKBrI7o");
                    }
                    else
                    {
                        await ReplyAsync("2doot5me, please try again later");
                    }
                }
                else
                {
                    await ReplyAsync("You aren't spooky enough to use this command.");
                }
            }
            else
            {
                await ReplyAsync("nah");
            }
        }

        /// <summary>
        /// Adds the Jack-o-Lantern, man dancing, and woman dancing emoji as a reaction to the user's
        /// command message and replies back with the youtube link for
        /// the dancing pumpkin man
        /// </summary>
        /// <returns></returns>
        [Command("Spoop")]
        [Alias("SpoopyDance")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ViewChannel | GuildPermission.SendMessages)]
        public async Task Spoop()
        {
            // if october
            if (DateTime.Now.Month == 10)
            {
                if (spookening.CanUserUseSpookyCommands(Context.User.Id))
                {
                    if (spookening.CheckUserDoot(Context.User.Id))
                    {
                        await Context.Message.AddReactionAsync(new Emoji("🎃"));
                        await Context.Message.AddReactionAsync(new Emoji("🕺"));
                        await Context.Message.AddReactionAsync(new Emoji("💃"));

                        await ReplyAsync("so spoopy\nhttps://www.youtube.com/watch?v=n_qbGJuxCYY");
                    }
                    else
                    {
                        await ReplyAsync("sorry but i just need a minute, that was too much for me");
                    }
                }
                else
                {
                    await ReplyAsync("You aren't spooky enough to use this command.");
                }
            }
            else
            {
                await ReplyAsync("nah");
            }
        }

        [Command("AdminResetSpookName")]
        [RequireOwner()]
        public async Task ResetName(IGuildUser user)
        {
            spookening.ResetNickname(user.Id);
            await ReplyAsync("k");
        }

        [Command("AdminResetAll")]
        [RequireOwner()]
        public async Task ResetAllNames()
        {
            var _ = Task.Factory.StartNew(() => spookening.ResetAllNames());
            await ReplyAsync("k, brace for rate limits");
        }

        [Command("AdminFixQueue")]
        [RequireOwner]
        public async Task ExpireAllQueue()
        {
            // fix the queue
            spookening.FixQueue();
            await ReplyAsync("k. existing spooks might not work sry");
        }

        [Command("HeyAdminSpookThesePeopleRightNow")]
        [RequireOwner]
        public async Task HeyAdminSpookThesePeopleRightNow(params IGuildUser[] users)
        {
            foreach (var user in users)
            {
                spookening.ForceSpookOverride(user.Id, user.Nickname);
            }
            await ReplyAsync("ugh. fine. try now.");
        }

        [Command("ThankMrSkeletal")]
        public async Task RespookMePlease()
        {
            if (Context.Guild.Id != spookening.TargetGuildId)
            {
                await ReplyAsync("sry wrong server");
                return;
            }
            if (DateTime.Now.Month == 10)
            {
                if (spookening.CanUserUseSpookyCommands(Context.User.Id))
                {
                    // if the user is spooked, then allow them to respook themselves
                    if (spookening.IsUserSpooked(Context.User.Id))
                    {
                        if (spookening.CheckUserRerollName(Context.User.Id))
                        {
                            try
                            {
                                spookening.RespookUser(Context.User.Id);
                            }
                            catch (Exception e)
                            {
                                // silently catch all errors
                                Console.WriteLine($"Encountered exception when respooking {e}");
                            }
                        }
                        else
                        {
                            await ReplyAsync("Uhhhhhhhhhhhhhhhh I'm out of names. I'll think of more tomorrow.");
                        }
                    }
                }
                else
                {
                    await ReplyAsync("You aren't spooky enough to use this command.");
                }
            }
            else
            {
                await ReplyAsync("Nah.");
            }
        }

        [Command("AdminProcessSpooks")]
        [RequireOwner]
        public async Task ManuallySpookUsers()
        {
            spookening.ProcessSpooking();
            await ReplyAsync("Wow, that was spooky");
        }

        [Command("Spook")]
        [RequireContext(ContextType.Guild)]
        public async Task Spook(IGuildUser user)
        {
            if (Context.Guild.Id != spookening.TargetGuildId)
            {
                await ReplyAsync("sry wrong server");
                return;
            }

            if (DateTime.Now.Month == 10)
            {
                if (spookening.CanUserUseSpookyCommands(Context.User.Id))
                {
                    // check if the user they are spooking can be spooked
                    if (!user.IsBot && !spookening.IsUserSpooked(user.Id))
                    {
                        if (spookening.IsUserAlreadyQueued(user.Id))
                        {
                            await ReplyAsync("👻 This person is already going to be spooked.");
                        }
                        else if (spookening.DoesUserHaveSpooksRemaining(Context.User.Id))
                        {
                            // spook this user
                            spookening.QueueSpooking(user.Id, Context.User.Id);
                            await ReplyAsync("Beware! A spookening may happen tonight!");
                        }
                        else
                        {
                            await ReplyAsync("You can't spook any more people.");
                        }
                    }
                    else
                    {
                        await ReplyAsync("Uh oh! This user cannot be spooked.");
                    }
                }
                else
                {
                    await ReplyAsync("You aren't spooky enough to use this command.");
                }
            }
            else
            {
                await ReplyAsync("Nah.");
            }
        }

        [Command("SpookyJoke")]
        [RequireContext(ContextType.Guild)]
        public async Task SpookyJoke()
        {
            // actually this one is fine
            //if (Context.Guild.Id != SpookeningService.TargetGuildId)
            //{
            //    await ReplyAsync("sry wrong server");
            //    return;
            //}
            if (DateTime.Now.Month == 10)
            {
                if (spookening.CanUserUseSpookyCommands(Context.User.Id))
                {
                    if (spookening.CheckUserJoke(Context.User.Id))
                    {
                        await ReplyAsync(spookening.GetRandomSpookyJoke);
                    }
                    else
                    {
                        await ReplyAsync("What's the deal with airline food?\n\nnah but actually just try again in a bit.");
                    }
                }
                else
                {
                    await ReplyAsync("You aren't spooky enough to use this command.");
                }
            }
            else
            {
                await ReplyAsync("uhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh nahhhhhhhhhhhhhhhhhhhhhh");
            }
        }
    }
}

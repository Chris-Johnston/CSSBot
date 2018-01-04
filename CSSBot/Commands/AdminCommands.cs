using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace CSSBot.Commands
{
    // commands for administration by the bot owner
    [Group("Admin")]
    [RequireOwner()]
    [Remarks("These commands are for bot administration and are only allowed to be used by the bot owner.")]
    public class AdminCommands : ModuleBase
    {
        /// <summary>
        /// Removes all of the nicknames from each of the users in a guild
        /// These all assume that the bot has the permissions to do this
        /// preconditions don't matter when it's affecting another guild
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns></returns>
        [Command("RemoveNicknamesFromGuild")]
        [RequireOwner()]
        public async Task RemoveNicknamesFromGuild(ulong guildID)
        {
            var guild = await Context.Client.GetGuildAsync(guildID);
            if (guild != null)
            {
                foreach (var user in await guild.GetUsersAsync())
                {
                    RemoveUserNick(user);
                }
            }
        }

        [Command("RemoveUserNickname")]
        [RequireOwner()]
        public async Task RemoveUserNickname(ulong guildId, ulong userId)
        {
            var guild = await Context.Client.GetGuildAsync(guildId);
            if (guild != null)
            {
                var user = await guild.GetUserAsync(userId);
                RemoveUserNick(user);
            }
        }

        private static void RemoveUserNick(IGuildUser user)
        {
            try
            {
            user?.ModifyAsync(
                x => x.Nickname = null
            );
            }
            catch
            {
            // do nothing
            }
        }
        
        private async Task backupMessage(IMessage message, string path)
        {
            Console.WriteLine("backing up message " + message.Id);
            // store the content of the message in a text file
            string fileContent =
                $"ID: {message.Id} AuthorID: {message.Author.Id} {message.Timestamp}" +
                "\n\n"
                + $"{message.Content}";

            File.WriteAllText(Path.Combine(path, $"pin{message.Id}.txt"), fileContent);

            await saveMessageAttachments(message, path);
        }

        private async Task saveMessageAttachments(IMessage message, string path)
        {
            if (message.Attachments.Count > 0)
            {
                // make a directory for the attachments
                string attachPath = Path.Combine(path, $"attachments");
                var attachInfo = Directory.CreateDirectory(attachPath);
                
                foreach (var attachment in message.Attachments)
                {
                    using (var c = new HttpClient())
                    {
                        // download the attachments
                        // and save them to our directory
                        var stream = await c.GetStreamAsync(attachment.Url);
                        using (var file = new FileStream(Path.Combine(attachPath, $"{attachment.Id}{attachment.Filename}"), FileMode.Create))
                        {
                            // populate the file
                            stream.CopyTo(file);
                        }
                    }
                }
            }
        }

        private async Task backupPinMessage(IMessage message, string pinPath)
        {
            Console.WriteLine("backing up pin message " + message.Id);
            // store the content of the message in a text file
            string fileContent =
                $"ID: {message.Id} AuthorID: {message.Author.Id} {message.Timestamp}" +
                "\n\n"
                + $"{message.Content}";

            File.WriteAllText(Path.Combine(pinPath, $"pin{message.Id}.txt"), fileContent);

            await saveMessageAttachments(message, pinPath);
        }
        
        // goes and backs up all of the pinned messages and all of the images
        // this is kinda api spammy, so don't do this very often
        [Command("BackupGuild")]
        [RequireOwner()]
        public async Task BackupQuotes(ulong guildId)
        {
            Console.WriteLine("Starting backup of guild " + guildId);

            string current_dir = Directory.GetCurrentDirectory();
            var guild = await Context.Client.GetGuildAsync(guildId);

            if (guild == null) return;

            string backup_dir = Path.Combine(current_dir, $"backup {guildId} ({guild.Name})");
            var backupInfo = Directory.CreateDirectory(backup_dir);

            foreach(var channel in await guild.GetTextChannelsAsync())
            {
                // make a directory for this channel
                // with the name of the channel Id
                string channelPath = Path.Combine(backup_dir, $"{channel.Id} ({channel.Name})");

                // make new dir
                var directoryInfo = Directory.CreateDirectory(channelPath);


                var pins = await channel.GetPinnedMessagesAsync();
                if (pins != null && pins.Count > 0)
                {
                    // make a folder for pins, if doesn't already exist
                    string pinPath = Path.Combine(channelPath, "pins");
                    var pinDirInfo = Directory.CreateDirectory(pinPath);
                    // save all of the pins
                    foreach(var message in await channel.GetPinnedMessagesAsync())
                    {
                        Console.WriteLine("getting pins");
                    
                        await backupPinMessage(message, pinPath);                
                    }
                }

                // download the last 2000 messages
                ulong? lastMessageId = null;
                for(int i = 0; i < 20; i++)
                {
                    Console.WriteLine("getting messages " + i);
                    // get 100 messages at a time
                    IEnumerable<IMessage> col;
                    if(lastMessageId.HasValue)
                    {
                        col = await channel.GetMessagesAsync(lastMessageId.Value, Direction.After, 100, CacheMode.AllowDownload).Flatten();
                    }
                    else
                    {
                        col = await channel.GetMessagesAsync(100, CacheMode.AllowDownload).Flatten();
                    }

                    foreach(var message in col)
                    {
                        // only backup messages that contain images
                        if(message.Attachments.Count > 0)
                        {
                            await backupMessage(message, channelPath);
                        }
                        lastMessageId = message.Id;
                    }
                }
            }
        }

        /// <summary>
        /// Used for sending an embed-style "announcement" a text channel
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="textChannelID"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [Command("AnnounceGuildChannel")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task AnnounceToGuildChannel(ulong guildId, ulong textChannelID, [Remainder] string text)
        {
            var guild = await Context.Client.GetGuildAsync(guildId) as SocketGuild;
            var channel = guild.GetTextChannel(textChannelID) as SocketTextChannel;

            var embed = new EmbedBuilder();
            embed.WithColor(new Color(255, 204, 77));
            embed.WithTitle("Announcement");
            embed.WithCurrentTimestamp();
            embed.WithAuthor(Context.Client.CurrentUser);
            embed.WithDescription(text);

            var result = await channel.SendMessageAsync("", false, embed.Build());
            await ReplyAsync("OK, sent.");
            await ReplyAsync(result.Id.ToString(), false, embed.Build());
        }

        /// <summary>
        /// Used for sending a message to a text channel of a guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="textChannelId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [Command("SendMessageGuildChannel")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task SendMessageToGuildChannel(ulong guildId, ulong textChannelId, [Remainder] string text)
        {
            var guild = await Context.Client.GetGuildAsync(guildId) as SocketGuild;
            if (guild != null)
            {
                var channel = guild.GetTextChannel(textChannelId) as SocketTextChannel;

                var result = await channel.SendMessageAsync(text);
                await ReplyAsync("OK, " + result.Id + ".\n" + text);
            }
        }

        /// <summary>
        /// Used for sending a text message to a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [Command("SendMessageUser")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task SendMessageToUser(ulong userId, [Remainder] string text)
        {
            var user = await Context.Client.GetUserAsync(userId) as SocketUser;
            var channel = await user.GetOrCreateDMChannelAsync();

            var result = await channel.SendMessageAsync(text);
            await ReplyAsync("OK, " + result.Id + ".\n" + text);
        }

        /// <summary>
        /// used for banning a user from a guild
        /// this can be done before the user has ever joined the guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Command("GuildBanUserByID")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task GuildBanUserByID(ulong guildId, ulong userId)
        {
            var guild = await Context.Client.GetGuildAsync(guildId);
            await guild.AddBanAsync(userId);

            await ReplyAsync("Ok, banned user " + userId + " from guild " + guildId);
        }
    }
}

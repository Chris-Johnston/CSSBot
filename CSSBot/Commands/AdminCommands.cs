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
    public class AdminCommands : ModuleBase
    {
        private async Task backupMessage(IMessage message, string path)
        {
            Console.WriteLine("backing up message " + message.Id);
            // store the content of the message in a text file
            string fileContent =
                $"ID: {message.Id} AuthorID: {message.Author.Id} {message.Timestamp}" +
                "\n\n"
                + $"{message.Content}";

            File.WriteAllText(Path.Combine(path, $"pin{message.Id}.txt"), fileContent);

            foreach (var attachment in message.Attachments)
            {
                // make a directory for the attachments
                string attachPath = Path.Combine(path, $"attachments{message.Id}");
                var attachInfo = Directory.CreateDirectory(attachPath);

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

        private async Task backupPinMessage(IMessage message, string pinPath)
        {
            Console.WriteLine("backing up pin message " + message.Id);
            // store the content of the message in a text file
            string fileContent =
                $"ID: {message.Id} AuthorID: {message.Author.Id} {message.Timestamp}" +
                "\n\n"
                + $"{message.Content}";

            File.WriteAllText(Path.Combine(pinPath, $"pin{message.Id}.txt"), fileContent);

            foreach (var attachment in message.Attachments)
            {
                // make a directory for the attachments
                string attachPath = Path.Combine(pinPath, $"attachments{message.Id}");
                var attachInfo = Directory.CreateDirectory(attachPath);

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

            string backup_dir = Path.Combine(current_dir, $"backup{guildId}");
            var backupInfo = Directory.CreateDirectory(backup_dir);

            foreach(var channel in await guild.GetTextChannelsAsync())
            {
                // make a directory for this channel
                // with the name of the channel Id
                string channelPath = Path.Combine(backup_dir, channel.Id.ToString());

                // make new dir
                var directoryInfo = Directory.CreateDirectory(channelPath);

                // make a folder for pins
                string pinPath = Path.Combine(channelPath, "pins");
                var pinDirInfo = Directory.CreateDirectory(pinPath);

                Console.WriteLine("getting pins");
                // save all of the pins
                foreach(var message in await channel.GetPinnedMessagesAsync())
                {
                    await backupPinMessage(message, pinPath);
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
            embed.WithAuthor(Context.User);
            embed.WithDescription(text);

            var result = await channel.SendMessageAsync("", false, embed.Build());
            await ReplyAsync("OK, sent.");
            await ReplyAsync(result.Id.ToString(), false, embed.Build());
        }

        [Command("SendMessageGuildChannel")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task SendMessageToGuildChannel(ulong guildId, ulong textChannelId, [Remainder] string text)
        {
            var guild = await Context.Client.GetGuildAsync(guildId) as SocketGuild;
            var channel = guild.GetTextChannel(textChannelId) as SocketTextChannel;

            var result = await channel.SendMessageAsync(text);
            await ReplyAsync("OK, " + result.Id + ".\n" + text);
        }

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

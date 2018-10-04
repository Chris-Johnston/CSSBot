using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    [Name("")]
    public class MsdnCommands : ModuleBase
    {
        [Command("MSDN")]
        [Summary("Searches the MSDN API for the given string.")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task SearchMSDN([Remainder] string s)
        {
            var results = await GetMsdnResultsAsync(s);

            if (results != null)
            {
                var eb = new EmbedBuilder();
                eb.WithCurrentTimestamp();
                eb.WithTitle($"MSDN Search Results for {s}");
                eb.WithColor(new Color(255, 204, 77));

                var sb = new StringBuilder();

                foreach (var x in results.Results.Take(7))
                {
                    sb.AppendLine($"{x.ItemKind} [{x.DisplayName}]({x.Url})");
                }

                sb.AppendLine($"\n[Wrong results? Search MSDN here.]({GetMsdnFrontEndSearch(s)})");
                eb.WithDescription(sb.ToString());

                await ReplyAsync("Got results from MSDN:", embed: eb.Build());
            }
            else
            {
                await ReplyAsync("Oops. Encountered an error and couldn't get the results from MSDN.");
            }
        }

        private string GetMsdnAPISearchUrl(string s)
            => $"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={WebUtility.UrlEncode(s)}";

        private string GetMsdnFrontEndSearch(string s)
            => $"https://docs.microsoft.com/en-us/dotnet/api/?term={WebUtility.UrlEncode(s)}";

        private async Task<MsdnApiSearchResults> GetMsdnResultsAsync(string s)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(GetMsdnAPISearchUrl(s));

                if (result.IsSuccessStatusCode)
                {
                    // read the contents into a json reader
                    return JsonConvert.DeserializeObject<MsdnApiSearchResults>(await result.Content.ReadAsStringAsync());
                }
            }
            return null;
        }

    }
}

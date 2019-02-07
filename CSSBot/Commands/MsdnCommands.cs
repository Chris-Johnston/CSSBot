using CSSBot.Services;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    [Name("")]
    public class MsdnCommands : RetryModuleBase
    {
        /// <summary>
        ///     Cache resuls so that duplicates are not repeated.
        ///     Keys are uppercase.
        /// </summary>
        private Dictionary<string, MsdnApiSearchResults> SearchCache = new Dictionary<string, MsdnApiSearchResults>();

        /// <summary>
        ///     Regex for valid search queries.
        /// </summary>
        private static Regex SearchTermRegex = new Regex(@"^[A-Za-z][A-Za-z0-9\\.<>,]+$");

        public MsdnCommands(MessageRetryService retry) : base(retry)
        { }

        [Command("MSDN")]
        [Summary("Searches the MSDN API for the given string.")]
        public async Task SearchMSDN([Remainder] string s)
        {
            var results = await GetMsdnResultsAsync(s);

            if (results != null)
            {
                var eb = new EmbedBuilder();
                eb.WithCurrentTimestamp();
                eb.WithTitle($"MSDN Search Results for \"{s}\"");
                eb.WithColor(new Color(255, 204, 77));

                var sb = new StringBuilder();

                foreach (var x in results.Results.Take(3))
                {
                    sb.AppendLine($"{x.ItemKind} [{x.DisplayName}]({x.Url})\n{WebUtility.UrlDecode(x.Description)}\n");
                }

                sb.AppendLine($"[Wrong results? Search MSDN here.]({GetMsdnFrontEndSearch(s)})");
                eb.WithDescription(sb.ToString());

                await ReplyOrUpdateAsync("Got results from MSDN:", embed: eb.Build());
            }
            else
            {
                await ReplyOrUpdateAsync("Oops. Encountered an error and couldn't get the results from MSDN. Searches must match the regular expression: `^[A-Za-z][A-Za-z0-9\\.<>,]+$`");
            }
        }

        private string GetMsdnAPISearchUrl(string s)
            => $"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={WebUtility.UrlEncode(s)}";

        private string GetMsdnFrontEndSearch(string s)
            => $"https://docs.microsoft.com/en-us/dotnet/api/?term={WebUtility.UrlEncode(s)}";

        private async Task<MsdnApiSearchResults> GetMsdnResultsAsync(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentNullException(nameof(s), "The supplied search term may not be null/whitespace.");
            s = s.Trim();
            if (!SearchTermRegex.IsMatch(s))
            {
                throw new ArgumentException(paramName: nameof(s), message:
                    "The search term contained invalid characters.");
            }
            var key = s.ToUpper();

            // check if exists in cache
            if (SearchCache.ContainsKey(key))
            {
                return SearchCache[key];
            }

            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(GetMsdnAPISearchUrl(s));

                if (result.IsSuccessStatusCode)
                {
                    // read the contents into a json reader
                    var r = JsonConvert.DeserializeObject<MsdnApiSearchResults>(await result.Content.ReadAsStringAsync());
                    // store in cache
                    SearchCache.Add(key, r);
                    return r;
                }
            }
            return null;
        }

    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Services.TheSpookening.Models
{
    public class SpookConfiguration
    {
        [JsonProperty]
        public ulong TargetGuildId { get; set; }

        [JsonProperty]
        public ulong MessageChannelId { get; set; }

        [JsonProperty]
        public int SpookUserLimit { get; set; }

        [JsonProperty]
        public List<ulong> OverrideUserIds { get; set; }

        [JsonProperty]
        public List<string> SpookyEmojis { get; set; } = new List<string>()
        {
            "🕸️", "🕷️", "🦇", "🌚", "☠️", "💀", "👻", "🧛", "🧟", "🎃", "💡", "🔥"
        };

        [JsonProperty]
        public List<string> NicknameFormatters { get; set; } = new List<string>()
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
            "{0} is now alivent",
            "🦋 🔜 💡",
            "v a p o r {0} w a v e",
            "🕸️",
            "rolled critical fail",
            // backwards
            "{1}",
            "💡 moth 💡",
            "slaps roof of {0}"
        };

        public List<string> SpookyJokes { get; set; } = new List<string>()
        {
            "Q: What is in a ghost's nose?\nA: Boo-gers",
            "Q: What do you get when you cross a vampire and a snowman?\nA: Frostbite",
            "Q: Why do skeletons have low self-esteem?\nA: They have no body to love",
            "Q: Know why skeletons are so calm?\nA: Because nothing gets under their skin.",
            "Q: How do vampires get around on Halloween?\nA: On blood vessels",
            "Q: What’s a ghoul’s favorite bean?\nA: A human bean.",
            "Q: Why did the ghost go into the bar?\nA: For the Boos.",
            "Q: Why did the headless horseman go into business?\nA: He wanted to get ahead in life.",
            "Q: Why don’t mummies take time off?\nA: They’re afraid to unwind.",
            "Q: Why did the vampire need mouthwash?\nA: Because he had bat breath."
        };
    }
}

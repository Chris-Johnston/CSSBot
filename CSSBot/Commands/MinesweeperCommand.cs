using CSSBot.Commands;
using CSSBot.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot
{
    public class MinesweeperCommand : RetryModuleBase
    {
        /// <summary>
        ///     Stores the message id for the last puzzle that was made in a channel.
        /// </summary>
        public Dictionary<ulong, ulong> LastPuzzleInChannel
            = new Dictionary<ulong, ulong>();



        public enum Difficulty
        {
            Default = 0,
            Easy = 1,
            Medium = 2,
            Hard = 3,
            Impossible = 4
        }

        /// <summary>
        ///     Gets the block emoji number for a given digit from 0-8.
        /// </summary>
        public Dictionary<int, string> BlockNumber = new Dictionary<int, string>()
        {
            {0, "\u0030\u20e3" },
            {1, "\u0031\u20e3" },
            {2, "\u0032\u20e3" },
            {3, "\u0033\u20e3" },
            {4, "\u0034\u20e3" },
            {5, "\u0035\u20e3" },
            {6, "\u0036\u20e3" },
            {7, "\u0037\u20e3" },
            {8, "\u0038\u20e3" },
        };

        const string Bomb = "\uD83D\uDCA3";

        private Random random;

        public MinesweeperCommand(MessageRetryService retryService) : base(retryService)
        {
            random = new Random();
        }

        [Command("minesweeper")]
        [Alias("\u1f4a3")]
        public async Task Minesweeper(Difficulty difficulty = Difficulty.Default)
        {
            switch (difficulty)
            {
                case Difficulty.Medium:
                    await Minesweeper(8, 8, 10);
                    break;
                case Difficulty.Hard:
                    await Minesweeper(12, 12, 20);
                    break;
                case Difficulty.Impossible:
                    await Minesweeper(MaxDimension, MaxDimension, ((MaxDimension * MaxDimension) / 2) - 1);
                    break;
                default:
                case Difficulty.Default:
                case Difficulty.Easy:
                    await Minesweeper(5, 8, 5);
                    break;
            }
        }

        const int MinDimension = 1;
        const int MaxDimension = 13;

        const int MinBombs = 1;

        [Command("minesweeper")]
        [Alias("\u1f4a3")]
        public async Task Minesweeper(int x, int y, int numbombs)
        {
            // constrain the dimensions of x and y
            int Constrain(int dimension, int min = MinDimension, int max = MaxDimension)
            {
                if (dimension > max) return max;
                if (dimension < min) return min;
                return dimension;
            }
            x = Constrain(x);
            y = Constrain(y);
            // max is x * y / 2, cannot have that many bombs
            numbombs = Constrain(numbombs, 1, x * y / 2);

            var bombs = GenerateBombs(x, y, numbombs);
            var strs = GenerateOutputGrid(bombs, x, y);

            // generate the string to send
            StringBuilder sb = new StringBuilder();
            sb.Append($"{x} by {y} field with {numbombs} bombs.\n");
            for (int xdim = 0; xdim < x; xdim++)
            {
                for (int ydim = 0; ydim < y; ydim++)
                {
                    sb.Append($"||{strs[xdim, ydim]}||");
                }
                sb.AppendLine();
            }
            // reply with the resulting string
            var msg = await ReplyOrUpdateAsync(sb.ToString());

            if (msg != null)
            {
                // track the puzzle so that we can solve it later
                var channelid = Context.Channel.Id;
                if (LastPuzzleInChannel.ContainsKey(channelid))
                {
                    LastPuzzleInChannel[channelid] = msg.Id;
                }
                else
                {
                    LastPuzzleInChannel.Add(channelid, msg.Id);
                }
            }
        }

        /// <summary>
        ///     Edits the previous message to solve the result.
        /// </summary>
        [Command("Solve")]
        public async Task Solve(IMessage message = null)
        {
            var channelId = Context.Channel.Id;
            ulong messageId;
            if (message == null)
            {
                // get the last puzzle in this channel
                if (LastPuzzleInChannel.ContainsKey(channelId))
                {
                    messageId = LastPuzzleInChannel[channelId];
                }
                else
                {
                    await ReplyOrUpdateAsync("Couldn't find a puzzle to solve.");
                    return;
                }
            }
            else
            {
                messageId = message.Id;
            }

            // messageId is set
            var original = await Context.Channel.GetMessageAsync(messageId);
            if (original != null)
            {
                // reply with a solved copy of the puzzle, without the ||
                var content = original.Content.Replace("|", "");
                content = original.Content.Replace("@", "@​"); // @ with zwsp
                await ReplyOrUpdateAsync(content);
            }
            else
            {
                await ReplyOrUpdateAsync("Couldn't get the last puzzle message.");
            }

        }

        /// <summary>
        ///     Generates a 2d array of emojis used to represent the bomb state
        /// </summary>
        private string[,] GenerateOutputGrid(bool [,] bombs, int xdim, int ydim)
        {
            // gets the count of how many adjacent points are active
            int CountAdjacent((int x, int y) point)
            {
                // get all of the valid x and y coordinates from
                var validx = new List<int>() { point.x };
                var validy = new List<int>() { point.y };

                if (point.x - 1 >= 0)
                    validx.Add(point.x - 1);
                if (point.x + 1 < xdim)
                    validx.Add(point.x + 1);
                if (point.y - 1 >= 0)
                    validy.Add(point.y - 1);
                if (point.y + 1 < ydim)
                    validy.Add(point.y + 1);

                int count = 0;

                foreach (var x in validx)
                {
                    foreach (var y in validy)
                    {
                        if (bombs[x, y])
                            count++;
                    }
                }

                return count;
            }

            var ret = new string[xdim, ydim];

            for (int x = 0; x < xdim; x++)
            {
                for (int y = 0; y < ydim; y++)
                {
                    // if this cell is a bomb
                    if (bombs[x, y])
                    {
                        ret[x, y] = Bomb;
                    }
                    else
                    {
                        // get the count at this point
                        var count = CountAdjacent((x, y));
                        ret[x, y] = BlockNumber[count];
                    }
                }
            }

            return ret;
        }

        /// <summary>
        ///     Generates a 2d array that randomly contains a number of bombs.
        /// </summary>
        private bool[,] GenerateBombs(int xdimension, int ydimension, int numbombs)
        {
            // local function for converting an index to x y coords
            (int x, int y) IndexToCoord(int val, int xlen)
                => (val % xlen, val / xlen);

            // init a 2d array
            var ret = new bool[xdimension, ydimension];

            for (int i = 0; i < numbombs; i++)
            {
                // pick numbombs of bombs and add them
                var index = random.Next(xdimension * ydimension);
                var coord = IndexToCoord(index, xdimension);

                // set this cell to a bomb, if not already
                if (ret[coord.x, coord.y])
                {
                    // did not add bomb on this pass
                    i--;
                }
                else
                {
                    ret[coord.x, coord.y] = true;
                }
            }

            return ret;
        }
    }
}

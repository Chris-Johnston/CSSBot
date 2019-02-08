using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Services
{
    /// <summary>
    ///     Stores the Dictionary of last puzzles in each channel so that the can be recalled with the ?solve command
    /// </summary>
    public class MinesweeperSolutionService
    {
        /// <summary>
        ///     Gets or sets a dictionary keyed by a channel id which has the reference to the last puzzle in the channel.
        /// </summary>
        public Dictionary<ulong, ulong> LastPuzzleInChannel { get; set; } = new Dictionary<ulong, ulong>();
    }
}

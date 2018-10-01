using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Services.TheSpookening.Models
{
    public class SpookedUser
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        [BsonId]
        public int Id { get; set; }
        /// <summary>
        /// The user id of the person that has been spooked
        /// </summary>
        [BsonField]
        public ulong SpookedUserId { get; set; }
        /// <summary>
        /// When they have been spooked
        /// </summary>
        [BsonField]
        public DateTime SpookedTime { get; set; }
        /// <summary>
        /// The user id of the person that spooked them, if any.
        /// </summary>
        [BsonField]
        public ulong? SpookedByUserId { get; set; }

        [BsonField]
        public string OriginalNickName { get; set; } = null;
    }
}

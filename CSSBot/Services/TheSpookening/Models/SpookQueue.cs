using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Services.TheSpookening.Models
{
    public class SpookQueue
    {
        [BsonId]
        public int Id { get; set; }
        /// <summary>
        /// The user that is being spooked
        /// </summary>
        [BsonField]
        public ulong UserToSpookId { get; set; }
        /// <summary>
        /// the user who initiated this spook
        /// </summary>
        [BsonField]
        public ulong SpookedById { get; set; }
        /// <summary>
        /// Has the spook been done
        /// </summary>
        [BsonField]
        public bool Expired { get; set; } = false;
    }
}

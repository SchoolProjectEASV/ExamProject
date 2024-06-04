using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PostgressEntities
{
    /// <summary>
    /// The token entity used with PostgreSQL
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The login id
        /// </summary>
        public int LoginId { get; set; }
        /// <summary>
        /// JwtToken
        /// </summary>
        public string JwtToken { get; set; }
        /// <summary>
        /// The time the token expires
        /// </summary>
        public DateTime ExpiryDate { get; set; }
    }
}

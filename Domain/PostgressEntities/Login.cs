using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PostgressEntities
{
    /// <summary>
    /// The login entity used with PostgreSQL
    /// </summary>
    public class Login
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Username for the login
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Password for the login
        /// </summary>
        public string Password { get; set; }
    }
}

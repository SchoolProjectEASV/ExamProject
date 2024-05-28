using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PostgressEntities
{
    public class Token
    {
        public int Id { get; set; }
        public int LoginId { get; set; }
        public string JwtToken { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

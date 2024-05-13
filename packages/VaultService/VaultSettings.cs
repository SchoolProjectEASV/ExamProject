using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultService
{
    public class VaultSettings
    {
        public string Address { get; set; }
        public UserPassSettings UserPass { get; set; }

        public string CONNECTIONSTRING_MONGODB { get; set; }


        public class UserPassSettings
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}

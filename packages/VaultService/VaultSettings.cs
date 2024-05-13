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
        public AppRoleSettings AppRole { get; set; }
        public UserPassSettings UserPass { get; set; }

        public class AppRoleSettings
        {
            public string RoleId { get; set; }
            public string SecretId { get; set; }
        }

        public class UserPassSettings
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}

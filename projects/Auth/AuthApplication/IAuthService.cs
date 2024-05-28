using Domain.PostgressEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthApplication
{
    public interface IAuthService
    {
        Task<Login> AuthenticateAsync(string username, string password);
        string GenerateToken(Login login);

        public Task<int> RegisterUser(string username, string password);
    }
}

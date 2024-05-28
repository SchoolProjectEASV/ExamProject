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
        /// <summary>
        /// Retrieves the user by username and verifies the password hash
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Returns the login credentials if succesfuld</returns>
        Task<Login> AuthenticateUser(string username, string password);

        /// <summary>
        /// Generates a JWT token for the authenticated user adds the token to the backend and returns the token
        /// </summary>
        /// <param name="login">The login credentials</param>
        /// <returns>returns the generated JWT token</returns>
        string GenerateToken(Login login);

        /// <summary>
        /// Registers a user by checking for existing username, hashing the password and adding the user to the database
        /// </summary>
        /// <param name="username">Username for the new user</param>
        /// <param name="password">password for the new user</param>
        /// <returns>Returns the new users ID</returns>

        public Task<int> RegisterUser(string username, string password);
    }
}

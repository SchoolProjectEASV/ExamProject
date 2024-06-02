using Domain.PostgressEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthApplication
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepo _authRepo;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepo authRepo, IConfiguration configuration)
        {
            _authRepo = authRepo;
            _configuration = configuration;
        }

        public async Task<Login> AuthenticateUser(string username, string password)
        {
            var login = _authRepo.GetUsersByUsername(username);
            if (login == null || !VerifyPasswordHash(password, login.Password))
            {
                return null;
            }
            return login;
        }

        public AuthenticationToken GenerateToken(Login login)
        {
            var claims = new List<Claim>()
            {
            new Claim(JwtRegisteredClaimNames.Sub, login.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("scope", "ProductService.read"),
           // new Claim(JwtRegisteredClaimNames.Iss, "jens-key"),
            new Claim("scope", "ProductService.write"),
            new Claim("scope", "CategoryService.read"),
            new Claim("scope", "CategoryService.write"),
            new Claim("scope", "UserService.read"),
            new Claim("scope", "UserService.write")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var tokenEntity = new Token
            {
                LoginId = login.Id,
                JwtToken = jwtToken,
                ExpiryDate = token.ValidTo
            };
            var authToken = new AuthenticationToken
            {
                Value = jwtToken
            };

            _authRepo.AddTokenToLogin(tokenEntity);
            return authToken;
        }

        public async Task<int> RegisterUser(string username, string password)
        {
            var existingUser = _authRepo.GetUsersByUsername(username);
            if (existingUser != null)
            {
                throw new Exception("User already exists");
            }

            var hashedPassword = HashPassword(password);
            var newUser = new Login { Username = username, Password = hashedPassword };
            var user = _authRepo.AddLogin(newUser);
            return user.Id;
        }

        /// <summary>
        /// Generates a sal and hashes the password using PBKDF2 and HMACSHA1, and returns the salt and hashed password as a string.
        /// </summary>
        /// <param name="password">password being hashed</param>
        /// <returns></returns>
        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string saltString = Convert.ToBase64String(salt);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{saltString}:{hashed}";
        }

        /// <summary>
        /// Verifies the password hash
        /// </summary>
        /// <param name="password">The password being given</param>
        /// <param name="storedHash">the stored hash</param>
        /// <returns></returns>
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hash = parts[1];

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hash == hashed;
        }
    }
}

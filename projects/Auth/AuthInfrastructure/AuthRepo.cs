using Dapper;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;
using Domain.PostgressEntities;

public class AuthRepo : IAuthRepo
{
    
        private readonly string _connectionString;
        private readonly IVaultFactory _vaultFactory;

        public AuthRepo(IVaultFactory vaultFactory)
        {
            _vaultFactory = vaultFactory;
            _connectionString = GetConnectionStringFromVault();
            CreateTablesIfNotExists();
        }

        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        private string GetConnectionStringFromVault()
        {
            var connection = _vaultFactory.GetConnectionStringAuth();
            return connection;
        }

        private void CreateTablesIfNotExists()
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                CREATE TABLE IF NOT EXISTS logins (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(50) NOT NULL,
                    password VARCHAR(500) NOT NULL
                );

                CREATE TABLE IF NOT EXISTS tokens (
                    id SERIAL PRIMARY KEY,
                    login_id INT NOT NULL,
                    jwt_token VARCHAR(500) NOT NULL,
                    expiry_date TIMESTAMP NOT NULL,
                    FOREIGN KEY (login_id) REFERENCES logins(id)
                );";

                connection.Execute(query);
            }
        }

        public void AddTokenToLogin(Token token)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                INSERT INTO tokens (login_id, jwt_token, expiry_date)
                VALUES (@LoginId, @JwtToken, @ExpiryDate)";

                connection.Execute(query, token);
            }
        }

        public Login GetUserByToken(string token)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                SELECT l.* FROM logins l
                INNER JOIN tokens t ON l.id = t.login_id
                WHERE t.jwt_token = @Token";

                var login = connection.Query<Login>(query, new { Token = token }).FirstOrDefault();
                if (login == null)
                {
                    throw new Exception("User not found");
                }
                return login;
            }
        }

        public Login GetUsersByUsername(string username)
        {
            using (var connection = CreateConnection())
            {
                var query = "SELECT * FROM logins WHERE username = @Username";
                return connection.Query<Login>(query, new { Username = username }).FirstOrDefault();
            }
        }

        public Login AddLogin(Login login)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                INSERT INTO logins (username, password)
                VALUES (@Username, @Password)
                RETURNING id";

                login.Id = connection.ExecuteScalar<int>(query, login);
                return login;
            }
        }
    }

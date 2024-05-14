using Dapper;
using Domain.PostgressEntities;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using UserInfrastructure.Interfaces;

namespace UserInfrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        private IVaultFactory _vaultFactory;

        public UserRepository( IVaultFactory vaultFactory)
        {
            _vaultFactory = vaultFactory;
            _connectionString = GetConnectionStringFromVault();
            CreateUsersTableIfNotExists();
        }


        private string GetConnectionStringFromVault()
        {
            var connection = _vaultFactory.GetConnectionStringUser().Result;
            return connection;
        }
        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        private void CreateUsersTableIfNotExists()
        {
            using (var connection = CreateConnection())
            {
                var query = @"
            CREATE TABLE IF NOT EXISTS users (
                id SERIAL PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                email VARCHAR(255) NOT NULL UNIQUE
            );
        ";

                connection.Execute(query);
            }
        }




            public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<User>("SELECT * FROM users");
            }
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = "SELECT * FROM users WHERE id = @Id";
                return (await connection.QueryAsync<User>(query, new { Id = id })).FirstOrDefault();
            }
        }

        public async Task<int> AddUserAsync(User user)
        {
            using (var connection = CreateConnection())
            {
                var query = "INSERT INTO users (name, email) VALUES (@Name, @Email) RETURNING id;";
                var userId = await connection.ExecuteScalarAsync<int>(query, user);
                return userId;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = CreateConnection())
            {
                var query = "UPDATE users SET name = @Name, email = @Email WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, user);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = "DELETE FROM users WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }
    }
}

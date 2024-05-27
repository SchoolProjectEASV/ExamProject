using Dapper;
using Domain.PostgressEntities;
using Npgsql;
using System.Data;
using UserInfrastructure.Interfaces;

namespace UserInfrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        private IVaultFactory _vaultFactory;

        public UserRepository(IVaultFactory vaultFactory)
        {
            _vaultFactory = vaultFactory;
            _connectionString = GetConnectionStringFromVault();
            CreateUsersTableIfNotExists();
        }

        private string GetConnectionStringFromVault()
        {
            var connection = _vaultFactory.GetConnectionStringUser();
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
                    );";

                connection.Execute(query);
            }
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                    SELECT 
                        id, 
                        name, 
                        email 
                    FROM users";

                return await connection.QueryAsync<User>(query);
            }
        }

        public async Task<User> GetUserById(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                    SELECT 
                        id, 
                        name, 
                        email 
                    FROM users 
                    WHERE id = @Id";

                return (await connection.QueryAsync<User>(query, new { Id = id })).FirstOrDefault();
            }
        }

        public async Task<int> AddUser(User user)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                    INSERT INTO users (name, email) 
                    VALUES (@Name, @Email) 
                    RETURNING id;";

                var userId = await connection.ExecuteScalarAsync<int>(query, user);
                return userId;
            }
        }

        public async Task<bool> UpdateUser(User user)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                    UPDATE users 
                    SET name = @Name, email = @Email 
                    WHERE id = @Id";

                var affectedRows = await connection.ExecuteAsync(query, user);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteUser(int id)
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

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
            );";

            connection.Execute(query);
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

using Dapper;
using Npgsql;
using OrderInfrastructure.Interfaces;
using System.Data;
using Microsoft.Extensions.Logging;
using Domain.PostgressEntities;

namespace OrderInfrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IVaultFactory _vaultFactory;
        private readonly string _connectionString;

        public OrderRepository(IVaultFactory vaultFactory)
        {
            _vaultFactory = vaultFactory;
            _connectionString = GetConnectionStringFromVault();
            CreateOrderTableIfNotExists();
        }

        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        private string GetConnectionStringFromVault()
        {
            var connection = _vaultFactory.GetConnectionStringOrder();
            return connection;
        }

        private void CreateOrderTableIfNotExists()
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                    CREATE TABLE IF NOT EXISTS orders (
                        id SERIAL PRIMARY KEY,
                        created_at TIMESTAMP NOT NULL,
                        user_id INTEGER NOT NULL,
                        total_price NUMERIC NOT NULL,
                        shipping_address VARCHAR(255) NOT NULL
                    );";

                connection.Execute(query);
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            using (var connection = CreateConnection())
            {
                var query = @"
            SELECT 
                id, 
                created_at AS CreatedAt, 
                user_id AS UserId, 
                total_price AS TotalPrice, 
                shipping_address AS ShippingAddress 
            FROM orders";

                var orders = await connection.QueryAsync<Order>(query);
                return orders;
            }
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
            SELECT 
                id, 
                created_at AS CreatedAt, 
                user_id AS UserId, 
                total_price AS TotalPrice, 
                shipping_address AS ShippingAddress 
            FROM orders 
            WHERE id = @Id";

                return (await connection.QueryAsync<Order>(query, new { Id = id })).FirstOrDefault();
            }
        }

        public async Task<int> AddOrderAsync(Order order)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
            INSERT INTO orders (created_at, user_id, total_price, shipping_address)
            VALUES (@CreatedAt, @UserId, @TotalPrice, @ShippingAddress)
            RETURNING id;";


                var orderId = await connection.ExecuteScalarAsync<int>(query, order);


                return orderId;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = "DELETE FROM orders WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
                    UPDATE orders 
                    SET user_id = @UserId, total_price = @TotalPrice, shipping_address = @ShippingAddress 
                    WHERE id = @Id";

                var affectedRows = await connection.ExecuteAsync(query, order);
                return affectedRows > 0;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            using (var connection = CreateConnection())
            {
                var query = @"
            SELECT 
                id, 
                created_at AS CreatedAt, 
                user_id AS UserId, 
                total_price AS TotalPrice, 
                shipping_address AS ShippingAddress 
            FROM orders 
            WHERE user_id = @UserId";

                return await connection.QueryAsync<Order>(query, new { UserId = userId });
            }
        }
    }
}

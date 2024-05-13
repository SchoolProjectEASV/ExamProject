using Dapper;
using Domain;
using Npgsql;
using OrderInfrastructure.Interfaces;
using System.Data;

namespace OrderInfrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
            CreateOrderTableIfNotExists();
        }

        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
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
                        total_price DECIMAL(18, 2) NOT NULL,
                        shipping_address VARCHAR(255) NOT NULL,
                        products JSONB
                    );
                    ";


                connection.Execute(query);
            }
        }
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            using (var connection = CreateConnection())
            {
                var query = "SELECT * FROM orders";
                return await connection.QueryAsync<Order>(query);
            }

        }
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = $"SELECT * FROM orders WHERE id = {id}";
                return (await connection.QueryAsync<Order>(query, new { Id = id })).FirstOrDefault();
            }
        }

        public async Task<int> AddOrderAsync(Order order)
        {
            using (var connection = CreateConnection())
            {
                var query = "INSERT INTO orders (created_at, user_id, total_price, shipping_address, products) VALUES (@CreatedAt, @UserId, @TotalPrice, @ShippingAddress, @Products) RETURNING id;";
                var orderId = await connection.ExecuteScalarAsync<int>(query, order);
                return orderId;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = $"DELETE FROM orders WHERE id = {id}";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }


        public async Task<bool> UpdateOrderAsync(Order order)
        {
            using (var connection = CreateConnection())
            {
                var query = "UPDATE orders SET created_at = @CreatedAt, user_id = @UserId, total_price = @TotalPrice, shipping_address = @ShippingAddress, products = @Products WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, order);
                return affectedRows > 0;
            }
        }
    }
}

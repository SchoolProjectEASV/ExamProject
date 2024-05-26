﻿using Dapper;
using Npgsql;
using OrderInfrastructure.Interfaces;
using System.Data;
using Microsoft.Extensions.Logging;
using Domain;

namespace OrderInfrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IVaultFactory _vaultFactory;
        private readonly string _connectionString;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(IVaultFactory vaultFactory, ILogger<OrderRepository> logger)
        {
            _vaultFactory = vaultFactory;
            _logger = logger;
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
                var query = "SELECT * FROM orders";
                return await connection.QueryAsync<Order>(query);
            }
        }
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var query = "SELECT * FROM orders WHERE id = @Id";
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

                _logger.LogInformation("Executing query: {Query} with params: {Params}", query, order);

                var orderId = await connection.ExecuteScalarAsync<int>(query, order);

                _logger.LogInformation("Order added with ID: {OrderId}", orderId);

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

                _logger.LogInformation("Executing query: {Query} with params: {Params}", query, order);

                var affectedRows = await connection.ExecuteAsync(query, order);
                return affectedRows > 0;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            using (var connection = CreateConnection())
            {
                var query = "SELECT * FROM orders WHERE user_id = @UserId";
                return await connection.QueryAsync<Order>(query, new { UserId = userId });
            }
        }
    }
}

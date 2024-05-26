﻿namespace Domain
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public float TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
    }
}
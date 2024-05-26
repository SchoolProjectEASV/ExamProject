﻿using Domain.MongoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.DTO
{
    public class AddOrderDTO
    {
        public int UserId { get; set; }
        public float TotalPrice { get; set; }
        public string ShippingAddress { get; set; }

    }
}

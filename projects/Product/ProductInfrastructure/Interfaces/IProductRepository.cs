using Domain.MongoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductInfrastructure.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> AddProductAsync(Product product);
    }
}

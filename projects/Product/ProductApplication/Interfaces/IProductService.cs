using Domain.MongoEntities;
using ProductApplication.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApplication.Interfaces
{
    public interface IProductService
    {
        Task<Product> AddProductAsync(CreateProductDTO createProductDTO); 
    }
}

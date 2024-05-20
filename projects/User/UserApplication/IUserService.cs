using Domain.PostgressEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserApplication.DTO;

namespace UserApplication
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> GetAllUsersAsync();
        public Task<User> GetUserByIdAsync(int id);

        public Task<int> AddUserAsync(AddUserDTO userDTO);

        public Task<bool> UpdateUserAsync(User user);

        public Task<bool> DeleteUserAsync(int id);


    }
}

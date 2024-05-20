using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Domain.PostgressEntities;
using UserApplication.DTO;
using UserInfrastructure.Interfaces;

namespace UserApplication
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            return user;
        }

        public async Task<int> AddUserAsync(AddUserDTO userDTO)
        {

            var user = _mapper.Map<User>(userDTO);
            var userId = await _userRepository.AddUserAsync(user);
            return userId;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var success = await _userRepository.UpdateUserAsync(user);
            return success;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var success = await _userRepository.DeleteUserAsync(id);
            return success;
        }
    }
}
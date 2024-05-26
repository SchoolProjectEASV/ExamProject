using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Domain;
using Domain.PostgressEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserApplication.DTO;
using UserInfrastructure.Interfaces;

namespace UserApplication
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        private readonly HttpClient _httpClient;

        private readonly string orderServiceUrl;


        public UserService(IUserRepository userRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            orderServiceUrl = configuration["OrderService:Url"];
            _httpClient.BaseAddress = new Uri(orderServiceUrl);

        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users;
        }

        public async Task<GetUserDTO> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return null; // Handle not found scenario appropriately
            }

            var orders = await GetOrdersByUserIdAsync(id);
            var userDto = _mapper.Map<GetUserDTO>(user);
            userDto.orders = orders;

            return userDto;
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

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"Order/user/{userId}");
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(jsonResponse);
            return orders;
        }
    }
}
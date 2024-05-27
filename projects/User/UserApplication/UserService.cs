using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Domain.PostgressEntities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UserApplication.DTO;
using UserInfrastructure.Interfaces;
using StackExchange.Redis;

namespace UserApplication
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly string _orderServiceUrl;
        private readonly IConnectionMultiplexer _redis;

        public UserService(IUserRepository userRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            _orderServiceUrl = configuration["OrderService:Url"];
            _httpClient.BaseAddress = new Uri(_orderServiceUrl);
            _redis = redis;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<GetUserDTO> GetUserByIdAsync(int id)
        {
            var cachedUser = await GetCachedUserAsync(id);
            if (cachedUser != null)
            {
                return cachedUser;
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            var orders = await GetOrdersByUserIdAsync(id);
            var userDto = _mapper.Map<GetUserDTO>(user);
            userDto.orders = orders;

            await CacheUserAsync(userDto);

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
            if (success)
            {
                var updatedUser = await _userRepository.GetUserByIdAsync(user.Id);
                if (updatedUser != null)
                {
                    var userDto = _mapper.Map<GetUserDTO>(updatedUser);
                    await CacheUserAsync(userDto);
                }
            }

            return success;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var success = await _userRepository.DeleteUserAsync(id);
            if (success)
            {
                await RemoveCachedUserAsync(id);
            }
            return success;
        }

        public async Task<List<Domain.PostgressEntities.Order>> GetOrdersByUserIdAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"Order/user/{userId}");
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Domain.PostgressEntities.Order>>(jsonResponse);
            return orders;
        }

        private async Task CacheUserAsync(GetUserDTO userDto)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(GetRedisKeyForUser(userDto.Id), JsonConvert.SerializeObject(userDto));
        }

        private async Task<GetUserDTO> GetCachedUserAsync(int userId)
        {
            var db = _redis.GetDatabase();
            var userJson = await db.StringGetAsync(GetRedisKeyForUser(userId));
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonConvert.DeserializeObject<GetUserDTO>(userJson);
            }
            return null;
        }

        private async Task RemoveCachedUserAsync(int userId)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(GetRedisKeyForUser(userId));
        }

        private string GetRedisKeyForUser(int userId)
        {
            return $"user:{userId}";
        }
    }
}

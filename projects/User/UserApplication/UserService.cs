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
using System.Text;
using Microsoft.Extensions.Logging;

namespace UserApplication
{
    /// <summary>
    /// Contains the logic for the User crud operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly string _orderServiceUrl;
        private readonly IConnectionMultiplexer _redis;
        private readonly HttpClient _authHttpClient;
        private readonly string _authServiceUrl;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration, IConnectionMultiplexer redis, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            _orderServiceUrl = configuration["OrderService:Url"];
            _httpClient.BaseAddress = new Uri(_orderServiceUrl);
            _redis = redis;
            _authHttpClient = httpClientFactory.CreateClient();
            _authServiceUrl = configuration["AuthService:Url"];
            _authHttpClient.BaseAddress = new Uri(_authServiceUrl);
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<GetUserDTO> GetUserById(int id)
        {
            var cachedUser = await GetCachedUserAsync(id);
            if (cachedUser != null)
            {
                return cachedUser;
            }

            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                return null;
            }

            var orders = await GetOrdersByUserId(id);
            var userDto = _mapper.Map<GetUserDTO>(user);
            userDto.orders = orders;

            await CacheUserAsync(userDto);

            return userDto;
        }

        public async Task<int> AddUser(AddUserDTO userDTO)
        {
            var registrationDto = new UserRegistrationDTO
            {
                Username = userDTO.Username,
                Password = userDTO.Password
            };

            try
            {
                _logger.LogInformation("Sending request to Auth Service to register user.");
                var response = await _authHttpClient.PostAsync($"Auth/register", new StringContent(JsonConvert.SerializeObject(registrationDto), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error registering user with Auth Service: {response.StatusCode} - {errorContent}");
                    throw new Exception($"Error registering user with auth service: {response.ReasonPhrase}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response from Auth Service: {responseString}");

                // Deserialize the response to get the userId
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                int userId = responseObject.userId;

                _logger.LogInformation($"User registered successfully with ID: {userId}");

                var user = _mapper.Map<User>(userDTO);
                user.Id = userId;
                await _userRepository.AddUser(user);
                return userId;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HttpRequestException: {ex.Message}");
                throw new Exception("A problem occurred while trying to register the user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                throw;
            }
        }




        public async Task<bool> UpdateUser(User user)
        {
            var success = await _userRepository.UpdateUser(user);
            if (success)
            {
                var updatedUser = await _userRepository.GetUserById(user.Id);
                if (updatedUser != null)
                {
                    var userDto = _mapper.Map<GetUserDTO>(updatedUser);
                    await CacheUserAsync(userDto);
                }
            }

            return success;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var success = await _userRepository.DeleteUser(id);
            if (success)
            {
                await RemoveCachedUserAsync(id);
            }
            return success;
        }

        public async Task<List<Domain.PostgressEntities.Order>> GetOrdersByUserId(int userId)
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

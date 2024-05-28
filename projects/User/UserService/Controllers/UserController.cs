using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain.PostgressEntities;
using UserApplication.DTO;
using UserApplication;
using Serilog;

namespace UserService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                Log.Information("Fetched users", users);
                return Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error("Error fetching users: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Message = "Error fetching users", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var userDto = await _userService.GetUserById(id);
                if (userDto == null)
                {
                    Log.Warning("User not found with ID: {UserId}", id);
                    return NotFound(new { Message = "User not found" });
                }

                Log.Information("User found: {UserId}", id);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                Log.Error("Error fetching user by ID: {UserId}, Error: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { Message = "Error fetching user", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state for adding user");
                return BadRequest(ModelState);
            }

            try
            {
                var userId = await _userService.AddUser(userDTO);
                Log.Information("User added successfully: {UserId}", userId);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, userId);
            }
            catch (Exception ex)
            {
                Log.Error("Error adding user: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Message = "Error adding user", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                Log.Warning("User ID mismatch for update. Provided ID: {UserId}, User ID: {ProvidedUserId}", id, user.Id);
                return BadRequest(new { Message = "User ID mismatch" });
            }

            try
            {
                var success = await _userService.UpdateUser(user);
                if (!success)
                {
                    Log.Warning("User not found for update with ID: {UserId}", id);
                    return NotFound(new { Message = "User not found" });
                }

                Log.Information("User updated successfully: {UserId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error("Error updating user: {UserId}, Error: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { Message = "Error updating user", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var success = await _userService.DeleteUser(id);
                if (!success)
                {
                    Log.Warning("User not found for deletion with ID: {UserId}", id);
                    return NotFound(new { Message = "User not found" });
                }

                Log.Information("User deleted successfully: {UserId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error("Error deleting user: {UserId}, Error: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { Message = "Error deleting user", Error = ex.Message });
            }
        }
    }
}

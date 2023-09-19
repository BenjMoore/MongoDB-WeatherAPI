using ICTPRG553.Models.DTOs;
using ICTPRG553.Models.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoNotesAPI.Middleware;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.DTOs;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Repositories;
using System.Security.Cryptography.X509Certificates;

namespace MongoNotesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class UserController : ControllerBase
    {
        //Stores a reference to our repository so we can request database actions
        private readonly IUserRepository _userRepository;
        public UserRoleUpdateDTO update;
        //Requests our repository from the dependency injection and stores it in our 
        //readonly field.
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        [ApiKey("ADMIN")]

        public ActionResult CreateUser(string apiKey, UserCreateDTO userDTO)
        {
            //Ckeck if the user apiKey meets the required level(Admin Access) to add a
            //new user to the system.

            var user = new ApiUser
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Role = userDTO.Role,
                Created = userDTO.Created,
                Active = true

            };

            var result = _userRepository.CreateUser(user);
            return Ok();
        }


        [HttpPatch("UpdateRole")]
        public ActionResult UpdateRole(UserRoleUpdateDTO update)
        {
            // Check if a valid set of update details was provided.
            if (update == null)
            {
                return BadRequest();
            }

            // Check if at least one of the update fields has details to send to the database
            if (update.createdBefore == null && update.createdAfter == null)
            {
                return BadRequest();
            }

            // Call a method to update user roles based on the provided date range and new role
            var result = _userRepository.UpdateRole(update);

            if (result.WasSuccessful)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }



        [HttpDelete("DeleteUser")]
        [ApiKey("ADMIN")]
        public ActionResult DeleteUser(ApiUser user, string apiKey, String id)
        {
            //Ckeck if the user apiKey meets the required level(Admin Access) to add a
            //new user to the system.

            var result = _userRepository.DeleteUser(user, id);
            return Ok();
        }
        // public ActionResult DeleteUser(string apiKey, UserDeleteDTO userDTO)
        // {
        //Ckeck if the user apiKey meets the required level(Admin Access) to add a
        //new user to the system.
        // find by id 
        //var result = _userRepository.CreateUser(user);
        //
        //  return Ok();

        [HttpDelete("DeleteOlderThan30Days")]
        [ApiKey("ADMIN")]
        public ActionResult DeleteOlderThan30Days()
        {
            int days = 30;

            // Create a filter to match users who haven't logged in for more than 30 days
            UserFilter filter = new UserFilter
            {
                CreatedBefore = DateTime.Now.AddDays(-days),
                CreatedAfter = null // This ensures that all users older than 30 days are selected
            };

            // Call the repository method to delete these users
            var result = _userRepository.DeleteMany(filter);

            if (result.WasSuccessful)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

    }
}
 